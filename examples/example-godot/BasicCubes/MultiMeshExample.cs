using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using fennecs;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace examples.godot.BasicCubes;

[GlobalClass]
public partial class MultiMeshExample : Node3D
{
	private const int MaxEntities = 420_069;

	[Export] public MultiMeshInstance3D MeshInstance;

	[Export] public Camera3D Camera;
	[Export] public Slider SimulatedSlider;
	[Export] public Slider RenderedSlider;


	[Signal]
	public delegate void MySignalEventHandler(string willSendsAString);


	private int QueryCount => _query?.Count ?? 0;
	private int InstanceCount => MeshInstance.Multimesh.InstanceCount;

	private const float MinAmplitude = 200;
	private const float MaxAmplitude = 300;

	private Vector3 _goalAmplitude;
	private Vector3 _currentAmplitude;

	private const float BaseTimeScale = 0.0005f;
	private float _currentTimeScale = BaseTimeScale;

	private float _smoothCount = 1;

	private float _currentRenderedFraction;

	private readonly World _world = new();
	private Query<int, Matrix4X3, Vector3> _query;

	private double _time;
	private float[] _submissionArray = Array.Empty<float>();


	private void SetEntityCount(int spawnCount)
	{
		for (var i = _query.Count; i < spawnCount; i++)
		{
			_world.Spawn().Add(i)
				.Add<Matrix4X3>()
				.Add<Vector3>();
		}

		_query.Truncate(spawnCount);
	}

	public override void _Ready()
	{
		MeshInstance.Multimesh.VisibleInstanceCount = -1;

		_query = _world.Query<int, Matrix4X3, Vector3>().Build();

		SetEntityCount(MaxEntities);

		_on_simulated_slider_value_changed(SimulatedSlider.Value);
		_on_rendered_slider_value_changed(RenderedSlider.Value);

		_Process(0);
	}


	public override void _Process(double delta)
	{
		var dt = (float) delta;

		_time += delta * _currentTimeScale;

		//Size of entities rendered
		MeshInstance.Multimesh.InstanceCount = Mathf.FloorToInt(_currentRenderedFraction * _query.Count);

		// Soft count for the cubes so they move even smoother.
		_smoothCount = MeshInstance.Multimesh.InstanceCount;

		//Update positions <-- THIS IS WHERE THE HARD WORK IS DONE
		var chunkSize = Math.Max(_query.Count / 20, 128);
		_query.Job(UpdatePositionForCube, ((float) _time, _currentAmplitude, _smoothCount, dt), chunkSize: chunkSize);

		//Workaround for Godot not accepting oversize arrays or Spans.
		Array.Resize(ref _submissionArray, MeshInstance.Multimesh.InstanceCount * 12);

		// Make the cloud of cubes denser if there are more cubes
		var amplitudePortion = Mathf.Clamp((1.0f -_query.Count / (float) MaxEntities), 0f, 1f);
		_goalAmplitude = Mathf.Lerp(MinAmplitude, MaxAmplitude, amplitudePortion) * Vector3.One;
		_currentAmplitude = _currentAmplitude * 0.9f + 0.1f * _goalAmplitude;

		// Copy transforms into Multimesh <-- THIS IS WHERE THE DATA IS COPIED TO GODOT
		_query.Raw(static (Memory<int> _, Memory<Matrix4X3> transforms, (Rid mesh, float[] submission) uniform) =>
		{
			var floatSpan = MemoryMarshal.Cast<Matrix4X3, float>(transforms.Span);

			//We must copy the data manually once, into a pre-created array.
			//ISSUE: (Godot) It cannot come from an ArrayPool because it needs to have the exact size.
			floatSpan.Slice(0, uniform.submission.Length).CopyTo(uniform.submission);
			RenderingServer.MultimeshSetBuffer(uniform.mesh, uniform.submission);

			// Ideal way - raw Query to pass Memory<T>, Godot Memory<TY overload not yet available.
			//_query.Raw((_, transforms) => RenderingServer.MultimeshSetBuffer(MeshInstance.Multimesh.GetRid(), transforms));

			// This variant is also fast, but it doesn't work with the Godot API as that expects an array.
			// We're waiting on a change to the Godot API to expose the Span<float> overloads, which actually
			// match the internal API 1:1 (the System.Array parameter is the odd one out).
			// Calling Span.ToArray() makes an expensive allocation; and is unusable for this purpose.
			//RenderingServer.MultimeshSetBuffer(uniform.mesh, floatSpan);
		}, (MeshInstance.Multimesh.GetRid(), _submissionArray));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void UpdatePositionForCube(ref int index, ref Matrix4X3 transform, ref Vector3 position, (float Time, Vector3 Amplitude, float SmoothCount, float dt) uniform)
	{

		// Apply a chaotic Lissajous-like motion for the cubes
		var motionIndex = (index + uniform.Time * Mathf.Tau * 69f) % uniform.SmoothCount - uniform.SmoothCount/2f;
		
		var entityRatio = uniform.SmoothCount / MaxEntities;
		
		var phase1 = motionIndex * Mathf.Sin(motionIndex / 1500f * Mathf.Tau) * 7f * Mathf.Tau / uniform.SmoothCount;
		var phase2 = motionIndex * Mathf.Sin(motionIndex / 1700f * Mathf.Tau) * 5f * Mathf.Tau / uniform.SmoothCount;
		var phase3 = motionIndex * Mathf.Sin(motionIndex / 1000f * Mathf.Tau) * 11f * entityRatio * Mathf.Tau / uniform.SmoothCount;

		var vector = new Vector3
		{
			X = Mathf.Sin(phase1 + uniform.Time * 2f + motionIndex / 1500f),
			Y = Mathf.Sin(phase2 + uniform.Time * 3f + motionIndex / 1000f),
			Z = Mathf.Sin(phase3 + uniform.Time * 5f + motionIndex / 2000f),
		};

		var cubic = Mathf.Sin(uniform.Time * 100f * Mathf.Tau) * 0.5f + 0.5f;
		var shell = Mathf.Clamp(vector.Length(), 0, 1);
		vector = (1.0f-cubic) * shell * vector / vector.Length() + cubic * vector;

		// Write back some state and write the Transform that our MultiMesh is going to be receiving
		position = Fir(position, vector, 0.99f, uniform.dt);

		var scale = 1.5f - uniform.SmoothCount / MaxEntities;
		transform = new Matrix4X3(position * uniform.Amplitude, scale);
	}


	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion)
		{
			var screenPos = inputEvent.Get("position").AsVector2();

			rayOrigin = Camera.ProjectRayOrigin(screenPos);
			rayDirection = Camera.ProjectRayNormal(screenPos);
		}
	}


	public Godot.Vector3 rayOrigin { get; set; }
	public Godot.Vector3 rayDirection { get; set; }


	private void _on_rendered_slider_value_changed(double value)
	{
		// Set the number of entities to render
		_currentRenderedFraction = (float) value;

		// Move cubes faster if there are fewer visible
		_currentTimeScale = BaseTimeScale / Mathf.Max((float) value, 0.3f);
	}


	private void _on_simulated_slider_value_changed(double value)
	{
		// Set the number of entities to simulate
		var count = (int) Math.Ceiling(Math.Pow(value, Mathf.Sqrt2) * MaxEntities);
		count = Math.Clamp((count / 100 + 1) * 100, 0, MaxEntities);
		SetEntityCount(count);
	}

	private void _on_simulated_slider_drag_ended(bool valueChanged)
	{
		if (valueChanged) _on_simulated_slider_value_changed(SimulatedSlider.Value);
	}


	/// <summary>
	/// A basic finite impulse response filter.
	/// </summary>
	private static float Fir(float from, float to, float k, float dt)
	{
		var exponent = dt * 120f; // reference frame rate, it's 2024, for fox sake!

		var alpha = Mathf.Pow(k, exponent);

		return alpha * from + to * (1.0f - alpha);
	}


	/// <summary>
	/// A basic finite impulse response filter... for Vectors!
	/// </summary>
	private static Vector3 Fir(Vector3 from, Vector3 to, float k, float dt)
	{
		var exponent = dt * 120f; // reference frame rate, it's 2024, for fox sake!

		var alpha = Mathf.Pow(k, exponent);

		return alpha * from + to * (1.0f - alpha);
	}
}
