// SPDX-License-Identifier: MIT

using System;
using System.Runtime.InteropServices;
using Godot;
using Environment = System.Environment;
using Vector3 = System.Numerics.Vector3;

namespace fennecs.demos.godot;

/// <summary>
///     <para>
///         DemoCubes (Godot version)
///     </para>
///     <para>
///         All motion  is 100% CPU simulation (no GPU). Here, we demonstrate a simple case how to update the positions of a large number of Entities.
///     </para>
///     <para>
///         State is stored in Components on the Entities:
///     </para>
///     <ul>
///         <li>1x System.Numerics.Vector3 (as Position)</li>
///         <li>1x Matrix4x3 (custom struct, as Transform)</li>
///         <li>1x integer (as a simple identifier)</li>
///     </ul>
///     <para>
///         The state is transferred into the Godot Engine in bulk each frame using Query.Raw and submitting just the Matrix4x3 structs directly to a MultiMesh.
///     </para>
///     <para>
///         That static buffer is then used by Godot's Renderer to display the Cubes.
///     </para>
/// </summary>
[GlobalClass]
[Icon("res://icon.svg")]
public partial class DemoCubes : Node
{
	// Config: Maximum # of Entities that can be spawned. For brevity, made const instead
	// of [Export] so we don't have to pass it in as an additional uniform.
	private const int MaxEntities = 313370;

	// Calculation: Internal Speed of the Simulation.
	private const float BaseTimeScale = 0.0005f;

	// Fennecs: The World that will contain the Entities.
	private readonly World _world = new(MaxEntities);

	// Calculation: Visible CubeCount (can be smoothed to be passed in as an uniform).
	private float _cubeCount = 1;
	private Vector3 _currentAmplitude;

	// Calculation: Smoothed values expressing the portion of Entities that are visible.
	private float _currentRenderedFraction;
	private float _currentTimeScale = BaseTimeScale;

	// Calculation: Smoothed values for the simulation.
	private Vector3 _goalAmplitude;

	// Fennecs: The Query that will be used to interact with the Entities.
	private Query<Matrix4X3, Vector3, int> _query;

	// ??Boilerplate: Array used to copy the Entity Transform data into Godot's MultiMesh.
	private float[] _submissionArray = [];


	// Calculation: Elapsed time value for the simulation.
	private float _time;

	// Godot: Exports to interact with the UI
	[Export] public Camera3D Camera;

	// Godot: The main MultiMeshInstance3D that will be used to render the cubes.
	[Export] public MultiMeshInstance3D MeshInstance;

	// Config: Size of the simulation space
	[Export] public float MaxAmplitude = 400;
	[Export] public float MinAmplitude = 250;
	[Export] public Slider RenderedSlider;
	[Export] public Slider SimulatedSlider;

	// Godot: Read by the UI to show the simulated Entity count. (not just the visible ones)
	private int QueryCount => _query.Count;

	// Facade: Sets and reads the MultiMesh's InstanceCount.
	private int InstanceCount
	{
		get => MeshInstance.Multimesh.InstanceCount;
		set => MeshInstance.Multimesh.InstanceCount = value;
	}


	/// <summary>
	///     Spawn or Remove Entities to match the desired count.
	/// </summary>
	/// <param name="spawnCount">the count of entities to simulate</param>
	private void SetEntityCount(int spawnCount)
	{
		// Spawn new entities if needed.
		for (var i = _query.Count; i < spawnCount; i++)
			_world.Spawn().Add(i)
				.Add<Matrix4X3>()
				.Add<Vector3>();

		// Cut off excess entities, if any.
		_query.Truncate(spawnCount);
	}


	/// <summary>
	///     Godot _Ready() method, sets up our simulation.
	/// </summary>
	public override void _Ready()
	{
		// Boilerplate: Prepare our Query that we'll use to interact with the Entities.
		_query = _world.Query<Matrix4X3, Vector3, int>().Compile();

		// Boilerplate: Users can change the number of entities, so pre-warm the memory allocator a bit.
		SetEntityCount(MaxEntities);

		// Boilerplate: Apply the initial state of the UI.
		_on_simulated_slider_value_changed(SimulatedSlider.Value);
		_on_rendered_slider_value_changed(RenderedSlider.Value);
	}


	/// <summary>
	///     This is the Method that simulates the motion of the cubes and sends the data to Godot.
	/// </summary>
	/// <param name="delta">time elapsed since last tick, in seconds</param>
	public override void _Process(double delta)
	{
		// Calculation: Convert the delta time to a float (preferred use here).
		var dt = (float) delta;

		// Calculation: Accumulate the total elapsed time by adding the current frame time.
		_time += dt * _currentTimeScale;

		// Calculation: Determine the number of entities that will be displayed (also used to smooth out animation).
		_cubeCount = Mathf.FloorToInt(_currentRenderedFraction * _query.Count);

		// Calculation: A desirable size of each work item to spread it across available CPU cores.
		var chunkSize = Math.Max(_query.Count / Environment.ProcessorCount, 128);

		// ----------------------- HERE'S WHERE THE SIMULATION WORK IS RUN ------------------------
		// Update Transforms and Positions of all Cube Entities.
		//  We decided to put the code for this into a static method.
		// -------------------------------------------------------------------------------------------
		_query.Job(UpdatePositionForCube, (_time, _currentAmplitude, _cubeCount, dt));

		// Workaround for Godot not accepting oversize Arrays or Spans.
		Array.Resize(ref _submissionArray, (int) (_cubeCount * Matrix4X3.SizeInFloats));

		// Make the cloud of cubes denser if there are more cubes.
		var amplitudePortion = Mathf.Clamp(1.0f - _query.Count / (float) MaxEntities, 0f, 1f);
		_goalAmplitude = Mathf.Lerp(MinAmplitude, MaxAmplitude, amplitudePortion) * Vector3.One;
		_currentAmplitude = _currentAmplitude * 0.9f + 0.1f * _goalAmplitude;

		// Engine: Communicate the Number of Visible Entities to Godot's MultiMesh.
		InstanceCount = (int) _cubeCount;

		// ------------------------ HERE IS WHERE THE DATA IS SENT TO GODOT ------------------------
		// Copy transforms into Multimesh
		// Note that this is a static anonymous method: It doesn't have the allocation baggage of a lambda's closure.
		// We're saving a few keystrokes by using a method on the Query with only the first Stream Type (Matrix4X3).
		// But fennecs doesn't limit us. We can use any Instance or Static method, lambda, or delegate here.
		// -------------------------------------------------------------------------------------------
		_query.Raw(static delegate(Memory<Matrix4X3> transforms, (Rid mesh, float[] submission) uniform)
		{
			var floatSpan = MemoryMarshal.Cast<Matrix4X3, float>(transforms.Span);

			// We must copy the data manually once, into our pre-created array.
			// ISSUE : (Godot) It cannot come from an ArrayPool because it needs to have the exact size.
			// ISSUE : (Godot) It cannot come from a Span because the API doesn't accept it (yet).
			// Upvote: https://github.com/godotengine/godot-proposals/issues/9083
			floatSpan[..uniform.submission.Length].CopyTo(uniform.submission);
			RenderingServer.MultimeshSetBuffer(uniform.mesh, uniform.submission);

			// Dream way - raw Query to pass Memory<T>, Godot Memory<TY overload not yet available.
			// _query.Raw(transforms => RenderingServer.MultimeshSetBuffer(uniform.mesh, transforms));
			// or, in line with Godot's internal Marshalling:
			// _query.Raw(transforms => RenderingServer.MultimeshSetBuffer(uniform.mesh, transforms.Span));
			// ISSUE: Calling Span.ToArray() makes an expensive allocation; and is unusable for this purpose.
		}, (MeshInstance.Multimesh.GetRid(), _submissionArray));
	}


	// ----------------------- HERE'S WHERE THE SIMULATION WORK IS RUN ------------------------
	// Update Transforms and Positions of all Cube Entities.
	//  We decided to put the code for this into a static method to keep _Process() clean.
	// -------------------------------------------------------------------------------------------
	private static void UpdatePositionForCube(
		ref Matrix4X3 transform,
		ref Vector3 position,
		ref int index,
		(float Time, Vector3 Amplitude, float CubeCount, float dt) uniform)
	{
		#region Motion Calculations (just generic math for the cube motion)

		// Calculation: Apply a chaotic Lissajous-like motion for the cubes
		var motionIndex = (index + uniform.Time * Mathf.Tau * 69f) % uniform.CubeCount - uniform.CubeCount / 2f;

		var entityRatio = uniform.CubeCount / MaxEntities;

		var phase1 = motionIndex * Mathf.Sin(motionIndex / 1500f * Mathf.Tau) * 7f * Mathf.Tau / uniform.CubeCount;
		var phase2 = motionIndex * Mathf.Sin(motionIndex / 1700f * Mathf.Tau) * (Mathf.Sin(uniform.Time * 23f) + 1.5f) * 5f * Mathf.Tau / uniform.CubeCount;
		var phase3 = motionIndex * Mathf.Sin(motionIndex / 1000f * Mathf.Tau) * (Mathf.Sin(uniform.Time * 13f) + 1.5f) * 11f * entityRatio * Mathf.Tau / uniform.CubeCount;

		var vector = new Vector3
		{
			X = Mathf.Sin(phase1 + uniform.Time * 2f + motionIndex / 1500f),
			Y = Mathf.Sin(phase2 + uniform.Time * 3f + motionIndex / 1000f),
			Z = Mathf.Sin(phase3 + uniform.Time * 5f + motionIndex / 2000f),
		};

		var cubic = Mathf.Sin(uniform.Time * 100f * Mathf.Tau) * 0.5f + 0.5f;
		var shell = Mathf.Clamp(vector.Length(), 0, 1);
		vector = (1.0f - cubic) * shell * vector / vector.Length() + cubic * vector;

		#endregion


		// Update Component: Store position state, smoothing it to illustrate accumulative operations using data from the past frame.
		position = Fir(position, vector, 0.99f, uniform.dt);

		// Update Component: Build & store Matrix Transform (for the MultiMesh), scaling sizes between 1 and 3
		var scale = 2f * (1.5f - Mathf.Sqrt(uniform.CubeCount / MaxEntities));
		transform = new Matrix4X3(position * uniform.Amplitude, scale);
	}


	#region Signal Handlers

	/// <summary>
	///     Godot: Signal Handler
	/// </summary>
	private void _on_rendered_slider_value_changed(double value)
	{
		// Set the number of entities to render
		_currentRenderedFraction = (float) value;

		// Move cubes faster if there are fewer visible
		_currentTimeScale = BaseTimeScale / Mathf.Max((float) value, 0.3f);
	}


	/// <summary>
	///     Godot: Signal Handler
	/// </summary>
	private void _on_simulated_slider_value_changed(double value)
	{
		// Set the number of entities to simulate
		var count = (int) Math.Ceiling(Math.Pow(value, Mathf.Sqrt2) * MaxEntities);
		count = Math.Clamp((count / 100 + 1) * 100, 0, MaxEntities);
		SetEntityCount(count);
	}

	#endregion


	#region Math Helpers

	/// <summary>
	///     Calculation: A basic finite impulse response filter.
	/// </summary>
	private static float Fir(float from, float to, float k, float dt)
	{
		var exponent = dt * 120f; // reference frame rate, it's 2024, for fox sake!

		var alpha = Mathf.Pow(k, exponent);

		return alpha * from + to * (1.0f - alpha);
	}


	/// <summary>
	///     Calculation: A basic finite impulse response filter... for Vectors!
	/// </summary>
	private static Vector3 Fir(Vector3 from, Vector3 to, float k, float dt)
	{
		var exponent = dt * 120f; // reference frame rate, it's 2024, for fox sake!

		var alpha = Mathf.Pow(k, exponent);

		return alpha * from + to * (1.0f - alpha);
	}

	#endregion
}
