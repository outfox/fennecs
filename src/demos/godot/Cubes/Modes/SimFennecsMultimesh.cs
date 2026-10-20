// SPDX-License-Identifier: MIT

using System;
using System.Runtime.InteropServices;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace fennecs.demos.godot.Cubes;

/// <summary>
///     <para>
///         Modes g+h) fennecs + MultiMesh: state is stored in Components on the Entities
///         (Position, Matrix4X3 Transform, and an integer index).
///     </para>
///     <para>
///         The motion math runs single-threaded via Stream.For (mode f), or spread across all CPU
///         cores via Stream.Job when <see cref="UseJobs" /> is set (mode g). Either way, the
///         transforms are then transferred to Godot in bulk using Stream.Raw, submitting the raw
///         Matrix4X3 structs directly to the MultiMesh. Driven via Call() by <see cref="DemoCubes" />.
///     </para>
/// </summary>
public partial class SimFennecsMultimesh : Node
{
    [Export] public MultiMeshInstance3D MeshInstance = null!;

    // Mode g runs the simulation as a parallel Job; mode f uses a single-threaded For.
    [Export] public bool UseJobs;

    // Fennecs: The World that will contain the Entities.
    private readonly World _world = new();

    // Fennecs: The Stream used to simulate the Entities.
    private Stream<Matrix4X3, Vector3, int> _stream;

    // Fennecs: A view into the same World that only exports the Matrix4X3 transforms to Godot.
    private Stream<Matrix4X3> _exportStream;


    public override void _Ready()
    {
        _stream = _world.Query<Matrix4X3, Vector3, int>().Stream();
        _exportStream = _world.Query<Matrix4X3>().Stream();
    }


    public void Activate() => MeshInstance.Visible = true;


    public void Deactivate()
    {
        _stream.Query.Despawn();
        MeshInstance.Visible = false;
    }


    public void SetSimulatedCount(int count)
    {
        var difference = count - _stream.Count;

        if (difference > 0)
        {
            using var template = _world.Template()
                .Add<int>()
                .Add<Matrix4X3>()
                .Add<Vector3>();
            template.Spawn(difference);
        }

        if (difference < 0) _stream.Query.Truncate(count);

        // Keep indices contiguous 0..n-1 so the motion pattern matches the array-based modes.
        var i = 0;
        _stream.For((ref Matrix4X3 _, ref Vector3 _, ref int index) => index = i++);
    }


    public void UpdateSim(float time, Godot.Vector3 amplitude, float cubeCount, float dt)
    {
        var uniform = (time,
            new Vector3(amplitude.X, amplitude.Y, amplitude.Z),
            cubeCount,
            CubeMotion.CubeScale(cubeCount),
            dt);

        // ----------------------- HERE'S WHERE THE SIMULATION WORK IS RUN ------------------------
        // The same static method either runs on one core (For) or all of them (Job).
        // Job is unsupported on browser/WASM, where this mode degrades to For.
        // ----------------------------------------------------------------------------------------
        if (UseJobs) _stream.Job(uniform, UpdateCube);
        else _stream.For(uniform, UpdateCube);

        // Engine: Communicate the number of visible Entities to Godot's MultiMesh.
        var visibleCount = (int) cubeCount;
        MeshInstance.Multimesh.InstanceCount = visibleCount;

        // Submitting an empty buffer is illegal in the Godot API.
        if (visibleCount == 0) return;

        // ------------------------ HERE IS WHERE THE DATA IS SENT TO GODOT -----------------------
        // Copy transforms into the MultiMesh in bulk, one contiguous memory block per Archetype.
        // Note the static anonymous method: it has no allocation baggage of a lambda's closure.
        // ----------------------------------------------------------------------------------------
        _exportStream.Raw(
            uniform: (MeshInstance.Multimesh.GetRid(), visibleCount * Matrix4X3.SizeInFloats),
            action: static ((Rid mesh, int count) uniform, Memory<Matrix4X3> transforms) =>
            {
                var floatSpan = MemoryMarshal.Cast<Matrix4X3, float>(transforms.Span);
                RenderingServer.MultimeshSetBuffer(uniform.mesh, floatSpan[..uniform.count]);
            });
    }


    private static void UpdateCube(
        (float Time, Vector3 Amplitude, float CubeCount, float Scale, float Dt) uniform,
        ref Matrix4X3 transform,
        ref Vector3 position,
        ref int index)
    {
        CubeMotion.Simulate(index, uniform.Time, uniform.CubeCount, uniform.Dt, ref position);

        // Build & store the Matrix Transform for the MultiMesh.
        transform = new Matrix4X3(position * uniform.Amplitude, uniform.Scale);
    }
}
