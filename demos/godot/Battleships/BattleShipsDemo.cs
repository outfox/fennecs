using System.Collections.Generic;
using Godot;
using NVec2 = System.Numerics.Vector2;
using NMat3x2 = System.Numerics.Matrix3x2;

namespace fennecs.demos.godot.Battleships;

[GlobalClass]
public partial class BattleShipsDemo : Node2D
{
	[Export] public int FactionCount = 4;

	public readonly World World = new();

	private double _fps = 120;

	internal Dictionary<int, HashSet<SpatialClient>> SpatialHash = new();


	public override void _Process(double delta)
	{
		_fps = _fps * 0.99 + 0.01 * (1.0 / delta);

		var dt = (float) delta;

		var ships = World.Query<Ship, MotionState>().Stream();
		var guns = World.Query<Gun>().Stream();
		var bullets = World.Query<Bullet>().Stream();

		// 1) Steer each ship toward its fleet objective, then advance one step.
		ships.For(dt, (float step, ref Ship ship, ref MotionState motion) =>
		{
			var target = ship.Faction?.FleetObjective;
			if (target != null)
			{
				var dx = target.GlobalPosition.X - motion.Position.X;
				var dy = target.GlobalPosition.Y - motion.Position.Y;
				var desired = Mathf.Atan2(dy, dx);
				var diff = Mathf.Wrap(desired - motion.Course, -Mathf.Pi, Mathf.Pi);
				var maxTurn = ship.TurnRate * step;
				motion.Course += Mathf.Clamp(diff, -maxTurn, maxTurn);
			}

			var dir = NVec2.Transform(NVec2.UnitX, NMat3x2.CreateRotation(motion.Course));
			motion.Position += motion.Speed * step * dir;

			ship.GlobalPosition = new Vector2(motion.Position.X, motion.Position.Y);
			ship.Rotation = motion.Course;
		});

		// 2) Snapshot live ships for the O(N²) gun/bullet targeting passes.
		var shipList = new List<Ship>(ships.Count);
		ships.For(shipList, (List<Ship> list, ref Ship s, ref MotionState _) =>
		{
			if (s != null && IsInstanceValid(s)) list.Add(s);
		});

		// 3) Guns: cool down, pick nearest enemy in range + arc, aim, fire if loaded.
		var spawnQueue = new List<(Gun gun, Ship self, float angle)>();
		guns.For((shipList, spawnQueue, dt),
			((List<Ship> list, List<(Gun, Ship, float)> queue, float step) u, ref Gun gun) =>
			{
				if (!IsInstanceValid(gun)) return;
				var self = gun.OwnerShip;
				if (self == null || !IsInstanceValid(self)) return;

				gun.Cooldown = Mathf.Max(0f, gun.Cooldown - u.step);

				Ship best = null;
				var bestSq = gun.Range * gun.Range;
				var origin = self.GlobalPosition;
				foreach (var other in u.list)
				{
					if (other == self) continue;
					if (!IsInstanceValid(other)) continue;
					if (other.Faction == self.Faction) continue;
					var ds = origin.DistanceSquaredTo(other.GlobalPosition);
					if (ds < bestSq)
					{
						bestSq = ds;
						best = other;
					}
				}

				if (best == null) return;

				var globalAngle = (best.GlobalPosition - gun.GlobalPosition).Angle();
				var localAngle = Mathf.Wrap(globalAngle - self.Rotation, -Mathf.Pi, Mathf.Pi);
				if (localAngle < gun.FiringArc.X || localAngle > gun.FiringArc.Y) return;

				gun.Aim = best.GlobalPosition;
				gun.LookAt(gun.Aim);

				if (gun.Cooldown <= 0f)
				{
					gun.Cooldown = gun.ReloadTime;
					u.queue.Add((gun, self, globalAngle));
				}
			});

		// Bullets are spawned after iteration so structural changes don't disturb the gun stream.
		foreach (var (gun, self, angle) in spawnQueue) SpawnBullet(gun, self, angle);

		// 4) Move bullets, detect hits, queue kills.
		var bulletKills = new List<Entity>();
		bullets.For((shipList, bulletKills, dt),
			((List<Ship> list, List<Entity> dead, float step) u, ref Bullet bullet) =>
			{
				if (!IsInstanceValid(bullet)) return;

				bullet.Position += bullet.Velocity * u.step;
				bullet.Life -= u.step;

				if (bullet.Life <= 0f)
				{
					u.dead.Add(bullet.Entity);
					bullet.QueueFree();
					return;
				}

				foreach (var other in u.list)
				{
					if (!IsInstanceValid(other)) continue;
					if (other.Faction == bullet.Faction) continue;
					var dx = bullet.Position.X - other.GlobalPosition.X;
					var dy = bullet.Position.Y - other.GlobalPosition.Y;
					if (dx * dx + dy * dy > other.Radius * other.Radius) continue;

					other.TakeDamage(bullet.Damage);
					u.dead.Add(bullet.Entity);
					bullet.QueueFree();
					return;
				}
			});

		foreach (var e in bulletKills) e.Despawn();

		// 5) Flag capture: each objective tracks faction dominance within its radius.
		UpdateObjectives(dt, shipList);

		GetNode<Label>("Ui Layer/Label").Text =
			$"Ships: {ships.Count} Guns: {guns.Count} Bullets: {bullets.Count}\n FPS {Mathf.RoundToInt(_fps)}";
	}


	private void SpawnBullet(Gun gun, Ship self, float angle)
	{
		var bullet = new Bullet
		{
			GlobalPosition = gun.GlobalPosition,
			Rotation = angle,
			Faction = self.Faction,
			Damage = gun.Damage,
			Velocity = Vector2.FromAngle(angle) * gun.BulletSpeed,
			Life = gun.Range / gun.BulletSpeed,
		};
		AddChild(bullet);

		var entity = World.Spawn();
		entity.Add(bullet);
		bullet.Entity = entity;
	}


	private void UpdateObjectives(float dt, List<Ship> ships)
	{
		foreach (var node in GetTree().GetNodesInGroup("Objective"))
		{
			if (node is not Objective obj) continue;

			var counts = new Dictionary<Admiralty, int>();
			var op = obj.GlobalPosition;
			var total = 0;
			foreach (var s in ships)
			{
				if (!IsInstanceValid(s)) continue;
				if (s.Faction == null) continue;
				if (s.GlobalPosition.DistanceSquaredTo(op) > obj.Radius * obj.Radius) continue;
				counts.TryGetValue(s.Faction, out var c);
				counts[s.Faction] = c + 1;
				total++;
			}

			Admiralty leader = null;
			var leaderCount = 0;
			foreach (var (f, c) in counts)
			{
				if (c > leaderCount)
				{
					leaderCount = c;
					leader = f;
				}
			}

			// Strict majority required — contested objectives stall the timer.
			var uncontested = leader != null && leaderCount > total - leaderCount;

			if (uncontested && obj.Controller != leader)
			{
				obj.Timer += dt;
				if (obj.Timer >= Objective.CaptureTime)
				{
					obj.Controller = leader;
					obj.Timer = 0f;
					leader.Score++;
				}
			}
			else
			{
				obj.Timer = Mathf.Max(0f, obj.Timer - dt);
			}
		}
	}
}
