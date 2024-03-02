// SPDX-License-Identifier: MIT

using System;
using fennecs;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.UI.Controls;
using Environment = System.Environment;
using Vector3 = Stride.Core.Mathematics.Vector3;

namespace Cubes;

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
public class CubeDemo : SyncScript
{
    //  Config: Maximum # of Entities that can be spawned. For brevity, made const instead
    // of [Export] so we don't have to pass it in as an additional uniform.
    private const int MaxEntities = 313370;

    //  Calculation: Internal Speed of the Simulation.
    private const float BaseTimeScale = 0.0005f;

    //  Fennecs: The World that will contain the Entities.
    private readonly World _world = new();

    //  Calculation: Visible CubeCount (can be smoothed to be passed in as an uniform).
    private float _cubeCount = 1;
    private Vector3 _currentAmplitude;

    //  Calculation: Smoothed values expressing the portion of Entities that are visible.
    private float _currentRenderedFraction;
    private float _currentTimeScale = BaseTimeScale;

    //  Calculation: Smoothed values for the simulation.
    private Vector3 _goalAmplitude;

    //  Fennecs: The Query that will be used to interact with the Entities.
    private Query<Matrix, Vector3, int> _query;

    // ?? Boilerplate: Array used to copy the Entity Transform data into Godot's MultiMesh.
    private Matrix[] _submissionArray = Array.Empty<Matrix>();


    //  Calculation: Elapsed time value for the simulation.
    private float _time;

    //  Godot: The main MultiMeshInstance3D that will be used to render the cubes.
    public InstancingUserArray InstancingArray;
    public float MaxAmplitude = 400;

    //  Config: Size of the simulation space
    public float MinAmplitude = 250;

    //  Godot: Exports to interact with the UI
    public Slider RenderedSlider;
    public Slider SimulatedSlider;

    //  Godot: Read by the UI to show the simulated Entity count. (not just the visible ones)
    private int QueryCount => _query.Count;

    //  Facade: Sets and reads the MultiMesh's InstanceCount.
    private int InstanceCount
    {
        get => InstancingArray.InstanceCount;
        set => InstancingArray.UpdateWorldMatrices(_submissionArray, value);
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
                .Add<Matrix>()
                .Add<Vector3>();

        // Cut off excess entities, if any.
        _query.Truncate(spawnCount);
    }


    /// <summary>
    ///     Stride Start() method, sets up our simulation.
    /// </summary>
    public override void Start()
    {
        var component = Entity.Get<InstancingComponent>();
        InstancingArray = (InstancingUserArray) component.Type;

        component.Type = InstancingArray;

        //  Workaround for Godot not accepting oversize Arrays or Spans.
        Array.Resize(ref _submissionArray, MaxEntities);


        //  Boilerplate: Prepare our Query that we'll use to interact with the Entities.
        _query = _world.Query<Matrix, Vector3, int>().Build();

        //  Boilerplate: Users can change the number of entities, so pre-warm the memory allocator a bit.
        SetEntityCount(MaxEntities);

        //  Boilerplate: Apply the initial state of the UI.
        _on_simulated_slider_value_changed(.5);
        _on_rendered_slider_value_changed(.5);
    }


    /// <summary>
    ///     Stride Update() method, updates the simulation and passes data to Stride.
    /// </summary>
    public override void Update()
    {
        //  Calculation: Convert the delta time to a float (preferred use here).
        var dt = (float) Game.UpdateTime.Elapsed.TotalSeconds;

        //  Calculation: Accumulate the total elapsed time by adding the current frame time.
        _time += dt * _currentTimeScale;

        //  Calculation: Determine the number of entities that will be displayed (also used to smooth out animation).
        _cubeCount = (int) Math.Floor(_currentRenderedFraction * _query.Count);

        //  Calculation: A desirable size of each work item to spread it across available CPU cores.
        var chunkSize = Math.Max(_query.Count / Environment.ProcessorCount, 128);

        // -----------------------  HERE'S WHERE THE SIMULATION WORK IS RUN ------------------------
        //  Update Transforms and Positions of all Cube Entities.
        //  We decided to put the code for this into a static method.
        // -------------------------------------------------------------------------------------------
        _query.Job(UpdatePositionForCube, (_time, _currentAmplitude, _cubeCount, dt), chunkSize);

        //  Make the cloud of cubes denser if there are more cubes.
        var amplitudePortion = Math.Clamp(1.0f - _query.Count / (float) MaxEntities, 0f, 1f);
        _goalAmplitude = MathUtil.Lerp(MinAmplitude, MaxAmplitude, amplitudePortion) * Vector3.One;
        _currentAmplitude = _currentAmplitude * 0.9f + 0.1f * _goalAmplitude;

        //  Engine: Communicate the Number of Visible Entities to Godot's MultiMesh.
        //InstanceCount = (int) _cubeCount;

        // ------------------------  HERE IS WHERE THE DATA IS SENT TO GODOT ------------------------
        //  Copy transforms into Multimesh
        //  Note that this is a static anonymous method: It doesn't have the allocation baggage of a lambda's closure.
        //  We're saving a few keystrokes by using a method on the Query with only the first Stream Type (Matrix4X3).
        //  But fennecs doesn't limit us. We can use any Instance or Static method, lambda, or delegate here.
        // -------------------------------------------------------------------------------------------
        _query.Raw(static delegate(Memory<Matrix> transforms, (InstancingUserArray instancingArray, Matrix[] submissionArray, int cubeCount) uniform)
        {
            transforms.CopyTo(uniform.submissionArray);
            uniform.instancingArray.UpdateWorldMatrices(uniform.submissionArray, uniform.cubeCount);
        }, (InstancingArray, _submissionArray, (int) _cubeCount));
    }


    // -----------------------  HERE'S WHERE THE SIMULATION WORK IS RUN ------------------------
    //  Update Transforms and Positions of all Cube Entities.
    //  We decided to put the code for this into a static method to keep _Process() clean.
    // -------------------------------------------------------------------------------------------
    private static void UpdatePositionForCube(
        ref Matrix transform,
        ref Vector3 position,
        ref int index,
        (float Time, Vector3 Amplitude, float CubeCount, float dt) uniform)
    {
        #region Motion Calculations (just generic math for the cube motion)

        //  Calculation: Apply a chaotic Lissajous-like motion for the cubes
        var motionIndex = (index + uniform.Time * MathF.Tau * 69f) % uniform.CubeCount - uniform.CubeCount / 2f;

        var entityRatio = uniform.CubeCount / MaxEntities;

        var phase1 = motionIndex * MathF.Sin(motionIndex / 1500f * MathF.Tau) * 7f * MathF.Tau / uniform.CubeCount;
        var phase2 = motionIndex * MathF.Sin(motionIndex / 1700f * MathF.Tau) * (MathF.Sin(uniform.Time * 23f) + 1.5f) * 5f * MathF.Tau / uniform.CubeCount;
        var phase3 = motionIndex * MathF.Sin(motionIndex / 1000f * MathF.Tau) * (MathF.Sin(uniform.Time * 13f) + 1.5f) * 11f * entityRatio * MathF.Tau / uniform.CubeCount;

        var vector = new Vector3
        {
            X = MathF.Sin(phase1 + uniform.Time * 2f + motionIndex / 1500f),
            Y = MathF.Sin(phase2 + uniform.Time * 3f + motionIndex / 1000f),
            Z = MathF.Sin(phase3 + uniform.Time * 5f + motionIndex / 2000f),
        };


        var cubic = MathF.Sin(uniform.Time * 100f * MathF.Tau) * 0.5f + 0.5f;
        var shell = Math.Clamp(vector.Length(), 0, 1);
        vector = (1.0f - cubic) * shell * vector / vector.Length() + cubic * vector;

        #endregion


        //  Update Component: Store position state, smoothing it to illustrate accumulative operations using data from the past frame.
        position = Fir(position, vector, 0.99f, uniform.dt);

        //  Update Component: Build & store Matrix Transform (for the MultiMesh), scaling sizes between 1 and 3
        var scale = 2f * (1.5f - MathF.Sqrt(uniform.CubeCount / MaxEntities));
        transform = new Matrix
        {
            TranslationVector = position * uniform.Amplitude,
            ScaleVector = scale * Vector3.One,
        };
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
        _currentTimeScale = BaseTimeScale / MathF.Max((float) value, 0.3f);
    }


    /// <summary>
    ///     Godot: Signal Handler
    /// </summary>
    private void _on_simulated_slider_value_changed(double value)
    {
        // Set the number of entities to simulate
        var count = (int) Math.Ceiling(Math.Pow(value, MathF.Sqrt(2)) * MaxEntities);
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

        var alpha = MathF.Pow(k, exponent);

        return alpha * from + to * (1.0f - alpha);
    }


    /// <summary>
    ///     Calculation: A basic finite impulse response filter... for Vectors!
    /// </summary>
    private static Vector3 Fir(Vector3 from, Vector3 to, float k, float dt)
    {
        var exponent = dt * 120f; // reference frame rate, it's 2024, for fox sake!

        var alpha = MathF.Pow(k, exponent);

        return alpha * from + to * (1.0f - alpha);
    }

    #endregion
}