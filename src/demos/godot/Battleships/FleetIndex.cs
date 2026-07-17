using System;
using System.Collections.Generic;
using Godot;
using NVec2 = System.Numerics.Vector2;

namespace fennecs.demos.godot.Battleships;

/// <summary>
/// Orthogonal, cache-friendly view of the fighting fleet, rebuilt once per
/// frame: one contiguous array per component field the combat systems test
/// against, bulk-copied block-wise out of the ECS with Stream.Raw (the Query
/// is counted first so the arrays are sized up front). A spatial hash grid
/// over the same indices reduces proximity queries from "every ship afloat"
/// to a few grid cells.
/// </summary>
internal sealed class FleetIndex
{
	private const float CellSize = 512f;

	public int Count;
	public Ship[] Nodes = new Ship[256];
	public NVec2[] Positions = new NVec2[256];
	public NVec2[] Velocities = new NVec2[256];
	public float[] Radii = new float[256];
	public Admiralty[] Factions = new Admiralty[256];
	public float MaxRadius;

	private readonly Dictionary<long, List<int>> _grid = [];
	private readonly List<int> _nearby = [];


	public void Rebuild(Stream<Ship, MotionState> ships)
	{
		var expected = ships.Count;
		if (Nodes.Length < expected) Grow(expected);

		Count = 0;
		MaxRadius = 0f;

		// Raw runs once per Archetype and hands over its contiguous component
		// storage — each call appends one block to the arrays.
		ships.Raw(this, static (index, shipMemory, motionMemory) =>
		{
			var shipBlock = shipMemory.Span;
			var motionBlock = motionMemory.Span;
			for (var i = 0; i < shipBlock.Length; i++)
			{
				var ship = shipBlock[i];
				if (!GodotObject.IsInstanceValid(ship)) continue;

				var motion = motionBlock[i];
				var n = index.Count++;
				index.Nodes[n] = ship;
				index.Positions[n] = motion.Position;
				index.Velocities[n] = new NVec2(MathF.Cos(motion.Course), MathF.Sin(motion.Course)) * motion.Speed;
				index.Radii[n] = ship.Radius;
				index.Factions[n] = ship.Faction;
				index.MaxRadius = MathF.Max(index.MaxRadius, ship.Radius);
			}
		});

		// Re-bin the spatial hash; cell lists are pooled across frames.
		foreach (var cell in _grid.Values) cell.Clear();
		for (var i = 0; i < Count; i++)
		{
			var key = KeyFor(Positions[i]);
			if (!_grid.TryGetValue(key, out var cell)) _grid[key] = cell = [];
			cell.Add(i);
		}
	}


	/// <summary>
	/// Indices of all ships whose centers lie in the grid cells covering the
	/// given circle — a small superset of the true neighbors; callers still
	/// apply their exact distance test. Returns shared scratch: consume it
	/// before the next query.
	/// </summary>
	public List<int> Nearby(NVec2 position, float radius)
	{
		_nearby.Clear();
		var minX = (int) MathF.Floor((position.X - radius) / CellSize);
		var maxX = (int) MathF.Floor((position.X + radius) / CellSize);
		var minY = (int) MathF.Floor((position.Y - radius) / CellSize);
		var maxY = (int) MathF.Floor((position.Y + radius) / CellSize);

		for (var cy = minY; cy <= maxY; cy++)
		for (var cx = minX; cx <= maxX; cx++)
		{
			if (_grid.TryGetValue(((long) cx << 32) | (uint) cy, out var cell)) _nearby.AddRange(cell);
		}
		return _nearby;
	}


	private static long KeyFor(NVec2 position) =>
		((long) (int) MathF.Floor(position.X / CellSize) << 32) | (uint) (int) MathF.Floor(position.Y / CellSize);


	private void Grow(int needed)
	{
		var size = Nodes.Length;
		while (size < needed) size *= 2;
		Array.Resize(ref Nodes, size);
		Array.Resize(ref Positions, size);
		Array.Resize(ref Velocities, size);
		Array.Resize(ref Radii, size);
		Array.Resize(ref Factions, size);
	}
}
