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

	[Export] public Slider SimulatedSlider;
	[Export] public Slider RenderedSlider;


	[Signal]
	public delegate void MySignalEventHandler(string willSendsAString);


	private int QueryCount => _query?.Count ?? 0;
	private int InstanceCount => MeshInstance.Multimesh.InstanceCount;

	private const float MinAmplitude = 100;
	private const float MaxAmplitude = 300;

	private Vector3 _goalAmplitude;
	private Vector3 _currentAmplitude;

	private const float BaseTimeScale = 0.0005f;
	private float _currentTimeScale;

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

		while (spawnCount < _query.Count)
		{
			_query.Pop();
		}
	}

	public override void _Ready()
	{
		MeshInstance.Multimesh = new MultiMesh();
		MeshInstance.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		MeshInstance.Multimesh.Mesh = new BoxMesh();
		MeshInstance.Multimesh.Mesh.SurfaceSetMaterial(0, ResourceLoader.Load<Material>("res://BasicCubes/box_material.tres"));

		MeshInstance.Multimesh.VisibleInstanceCount = -1;

		_query = _world.Query<int, Matrix4X3, Vector3>().Build();

		_on_simulated_slider_value_changed(SimulatedSlider.Value);
		_on_rendered_slider_value_changed(RenderedSlider.Value);

		_Process(0);
	}


	public override void _Process(double delta)
	{
		_time += delta * _currentTimeScale;

		//Size of entities rendered
		MeshInstance.Multimesh.InstanceCount = Mathf.FloorToInt(_currentRenderedFraction * _query.Count);

		// Soft count for the cubes so they move even smoother.
		_smoothCount = _smoothCount * 0.9f + 0.1f * MeshInstance.Multimesh.InstanceCount;

		//Update positions <-- THIS IS WHERE THE HARD WORK IS DONE
		var chunkSize = Math.Max(_query.Count / 20, 128);
		_query.Job(UpdatePositionForCube, ((float) _time, _currentAmplitude, _smoothCount), chunkSize: chunkSize);

		//Workaround for Godot not accepting oversize arrays or Spans.
		Array.Resize(ref _submissionArray, MeshInstance.Multimesh.InstanceCount * 12);

		// Make the cloud of cubes denser if there are more cubes
		var amplitudePortion = Mathf.Clamp((1.0f -_query.Count * _currentRenderedFraction / MaxEntities), 0.1f, 1f);
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
	private static void UpdatePositionForCube(ref int index, ref Matrix4X3 transform, ref Vector3 position, (float time, Vector3 amplitude, float SmoothCount) uniform)
	{
		//var offset = Mathf.Tau(uniform.time / 100f)
		var phase1 = index * Mathf.Sin(index % 7 + uniform.time) * 17f * Mathf.Tau / uniform.SmoothCount;
		var phase2 = index * Mathf.Sin(index % 3 + uniform.time * 2f) * 13f * Mathf.Tau / uniform.SmoothCount;
		var phase3 = index * Mathf.Sin(index % 2 + uniform.time * 3f) * 11f * Mathf.Tau / uniform.SmoothCount;

		//group1 = group2 = group3 = 0;

		var value1 = phase1; //* Mathf.Pi * (group1 + Mathf.Sin(uniform.time) * 1f);
		var value2 = phase2; //* Mathf.Pi * (group2 + Mathf.Sin(uniform.time * 1f) * 3f);
		var value3 = phase3; //* Mathf.Pi * group3;

		var scale1 = 1f;
		var scale2 = 2f;
		var scale3 = 3f;

		var vector = new Vector3
		{
			X = Mathf.Sin(value1 + uniform.time * scale1 + index / 1500f),
			Y = Mathf.Sin(value2 + uniform.time * scale2 + index / 1000f),
			Z = Mathf.Sin(value3 + uniform.time * scale3 + index / 2000f),
		};

		position = position * 0.99f + 0.01f * vector;
		transform = new Matrix4X3(position * uniform.amplitude);
	}


	private void _on_button_pressed()
	{
		SetEntityCount(MaxEntities);
	}


	private void _on_rendered_slider_value_changed(double value)
	{
		// Set the number of entities to render
		_currentRenderedFraction = (float) value;

		// Move cubes faster if there are fewer visible
		_currentTimeScale = BaseTimeScale / Mathf.Max((float) value, 0.1f);
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
}
