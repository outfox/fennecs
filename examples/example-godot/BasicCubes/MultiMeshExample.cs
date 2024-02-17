using System;
using System.Runtime.InteropServices;
using fennecs;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace examples.godot.BasicCubes;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Matrix4X3
{
	public float M00;
	public float M01;
	public float M02;
	public float M03;

	public float M10;
	public float M11;
	public float M12;
	public float M13;

	public float M20;
	public float M21;
	public float M22;
	public float M23;

	public Matrix4X3()
	{
		M00 = 1;
		M01 = 0;
		M02 = 0;
		M03 = 0;
		M10 = 0;
		M11 = 1;
		M12 = 0;
		M13 = 0;
		M20 = 0;
		M21 = 0;
		M22 = 1;
		M23 = 0;
	}

	public Matrix4X3(Vector3 origin)
	{
		M00 = 1;
		M01 = 0;
		M02 = 0;
		M03 = origin.X;
		M10 = 0;
		M11 = 1;
		M12 = 0;
		M13 = origin.Y;
		M20 = 0;
		M21 = 0;
		M22 = 1;
		M23 = origin.Z;
	}

	public Matrix4X3(Vector3 bX, Vector3 bY, Vector3 bZ, Vector3 origin)
	{
		M00 = bX.X;
		M01 = bX.Y;
		M02 = bX.Z;
		M03 = origin.X;
		M10 = bY.X;
		M11 = bY.Y;
		M12 = bY.Z;
		M13 = origin.Y;
		M20 = bZ.X;
		M21 = bZ.Y;
		M22 = bZ.Z;
		M23 = origin.Z;
	}

	public override string ToString()
	{
		return $"Matrix4X3({M00}, {M01}, {M02}, {M03}, {M10}, {M11}, {M12}, {M13}, {M20}, {M21}, {M22}, {M23})";
	}
}


[GlobalClass]
public partial class MultiMeshExample : Node
{
	[Export] public int SpawnCount = 10_000;
	[Export] public MultiMeshInstance3D MeshInstance;

	private int InstanceCount => MeshInstance.Multimesh.InstanceCount;

	private readonly Vector3 _amplitude = new(120f, 90f, 120f);
	private const float TimeScale = 0.001f;

	private readonly World _world = new();
	private double _time;

	private void SpawnWave(int spawnCount)
	{
		for (var i = 0; i < spawnCount; i++)
		{
			_world.Spawn()
				.Add(i+ MeshInstance.Multimesh.InstanceCount)
				.Add<Matrix4X3>()
				.Id();
		}

		MeshInstance.Multimesh.InstanceCount += spawnCount;
		Array.Resize(ref _submissionArray, InstanceCount * 12);
	}

	public override void _Ready()
	{
		MeshInstance.Multimesh = new MultiMesh();
		MeshInstance.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		MeshInstance.Multimesh.Mesh = new BoxMesh();
		MeshInstance.Multimesh.Mesh.SurfaceSetMaterial(0, ResourceLoader.Load<Material>("res://BasicCubes/box_material.tres"));

		MeshInstance.Multimesh.VisibleInstanceCount = -1;

		SpawnWave(200_000);
		
		query = _world.Query<int, Matrix4X3>().Build();
	}

	private float[] _submissionArray = Array.Empty<float>();
	private Query<int, Matrix4X3> query;

	public override void _Process(double delta)
	{
		_time += delta * TimeScale;

		//Update positions
		query.Job(static (ref int index, ref Matrix4X3 transform, (float time, Vector3 amplitude) uniform) => 
		{
			var phase1 = index / 5000f * 2f;
			var group1 = 1 + (index / 1000)%5;

			var phase2 = index / 3000f * 2f;
			var group2 = 1 + (index / 1000)%3;

			var phase3 = index / 1000f * 2f;
			var group3 = 1 + (index / 1000)%10;

			var value1 = phase1 * Mathf.Pi * (group1 + Mathf.Sin(uniform.time) * 1f);
			var value2 = phase2 * Mathf.Pi * (group2 + Mathf.Sin(uniform.time * 1f) * 3f) ;
			var value3 = phase3 * Mathf.Pi * group3;

			var scale1 = 3f;
			var scale2 = 5f - group2;
			var scale3 = 4f;

			var vector = new Vector3
			{
				X = (float)Math.Sin(value1 + uniform.time * scale1),
				Y = (float)Math.Sin(value2 + uniform.time * scale2),
				Z = (float)Math.Sin(value3 + uniform.time * scale3),
			};

			transform = new Matrix4X3(vector * uniform.amplitude);
		}, ((float) _time, _amplitude), chunkSize: 4096);
		
		// Write transforms into Multimesh
		query.Span(static (Span<int> _, Span<Matrix4X3> transforms, (Rid mesh, float[] submission) uniform) =>
		{
			var floatSpan = MemoryMarshal.Cast<Matrix4X3, float>(transforms);

			//We must copy the data manually once, into a pooled array.
			floatSpan.CopyTo(uniform.submission);
			RenderingServer.MultimeshSetBuffer(uniform.mesh, uniform.submission);

			// Ideal way - raw query to pass Memory<T>, Godot Memory<TY overload not yet available.
			// query.Raw((_, transforms) => RenderingServer.MultimeshSetBuffer(MeshInstance.Multimesh.GetRid(), transforms));
			
			// This variant is also fast, but it doesn't work with the Godot API as that expects an array.
			// We're waiting on a change to the Godot API to expose the Span<float> overloads, which actually
			// match the internal API 1:1 (the System.Array parameter is the odd one out).
			// Calling Span.ToArray() makes an expensive allocation; and is unusable for this purpose.
			// RenderingServer.MultimeshSetBuffer(MeshInstance.Multimesh.GetRid(), floatSpan);
		}, (MeshInstance.Multimesh.GetRid(), _submissionArray));
	}

	private void _on_button_pressed()
	{
		SpawnWave(SpawnCount);
	}
}


