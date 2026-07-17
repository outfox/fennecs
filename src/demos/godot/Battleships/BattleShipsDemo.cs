using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NVec2 = System.Numerics.Vector2;

namespace fennecs.demos.godot.Battleships;

[GlobalClass]
public partial class BattleShipsDemo : Node2D
{
	public readonly World World = new();

	// Systems are just cached Streams plus methods — no scheduler needed.
	// The Wreck tag partitions the fleet: fighting ships in one stream,
	// sinking hulls in the other.
	private Stream<Ship, MotionState> _ships;
	private Stream<Ship, MotionState> _wrecks;
	private Stream<Gun> _guns;
	private Stream<Projectile> _shells;
	private Stream<Vfx> _vfx;

	// One MultiMeshInstance2D per effect family: thousands of shells and
	// explosions cost one draw call each, and add zero scene tree nodes.
	private InstanceBuffer _tracers;
	private InstanceBuffer _flashes;
	private InstanceBuffer _hits;
	private InstanceBuffer _smoke;
	private InstanceBuffer _explosions;

	// Per-frame snapshot of the live fleet, shared by targeting, avoidance,
	// collision, and objective control.
	private readonly List<ShipInfo> _armada = [];

	// Spawns queued while runners iterate, applied between systems.
	private readonly List<Projectile> _salvo = [];
	private readonly List<Vfx> _bursts = [];

	private readonly List<Admiralty> _admiralties = [];
	private readonly List<Objective> _objectives = [];
	private readonly Dictionary<Admiralty, int> _presence = [];

	private Label _hud;
	private double _fps = 120;
	private float _time;

	internal readonly record struct ShipInfo(Ship Node, NVec2 Position, NVec2 Velocity, float Radius, Admiralty Faction);


	public override void _Ready()
	{
		_ships = World.Query<Ship, MotionState>().Not<Wreck>().Stream();
		_wrecks = World.Query<Ship, MotionState>().Has<Wreck>().Stream();
		_guns = World.Query<Gun>().Stream();
		_shells = World.Query<Projectile>().Stream();
		_vfx = World.Query<Vfx>().Stream();

		_hud = GetNode<Label>("Ui Layer/Label");
		foreach (var node in GetTree().GetNodesInGroup("Admiralty"))
			if (node is Admiralty admiralty) _admiralties.Add(admiralty);
		foreach (var node in GetTree().GetNodesInGroup("Objective"))
			if (node is Objective objective) _objectives.Add(objective);

		var stripAdd = StripShader("blend_add");
		var stripMix = StripShader("blend_mix");

		_tracers = new InstanceBuffer(this, "Tracers", null, TracerShader(), hframes: 1, zIndex: 10);
		_hits = new InstanceBuffer(this, "Hits", Sheet("sheet_expl_05.png"), stripAdd, hframes: 24, zIndex: 12);
		_flashes = new InstanceBuffer(this, "MuzzleFlashes", Sheet("sheet_rotated_muzzle2.png"), stripAdd, hframes: 24, zIndex: 14);
		_smoke = new InstanceBuffer(this, "Smoke", Sheet("sheet_puff_smoke_01.png"), stripMix, hframes: 32, zIndex: 16);
		_explosions = new InstanceBuffer(this, "Explosions", Sheet("sheet_expl_09.png"), stripAdd, hframes: 32, zIndex: 18);
	}


	public override void _Process(double delta)
	{
		var dt = (float) delta;
		_time += dt;
		_fps = _fps * 0.95 + 0.05 / Math.Max(delta, 0.0001);

		SnapshotFleets();
		AcquireTargets();

		_tracers.Begin();
		_flashes.Begin();
		_hits.Begin();
		_smoke.Begin();
		_explosions.Begin();

		SteerShips(dt);
		DriftWrecks(dt);
		LayGuns(dt);
		FlyShells(dt);
		AgeAndRenderVfx(dt);

		// Entities queued while the runners were iterating spawn now.
		foreach (var shell in _salvo) World.Spawn().Add(shell);
		foreach (var burst in _bursts) World.Spawn().Add(burst);
		_salvo.Clear();
		_bursts.Clear();

		_tracers.Commit();
		_flashes.Commit();
		_hits.Commit();
		_smoke.Commit();
		_explosions.Commit();

		UpdateObjectives(dt);
		UpdateHud();
	}


	private void SnapshotFleets()
	{
		_armada.Clear();
		_ships.For(_armada, (armada, ref ship, ref motion) =>
		{
			if (!IsInstanceValid(ship)) return;
			armada.Add(new ShipInfo(ship, motion.Position, ship.CurrentVelocity, ship.Radius, ship.Faction));
		});
	}


	// Fire control: each ship designates its nearest enemy once per frame,
	// and every turret aboard trains on that designation — one O(N²) pass
	// instead of one per gun.
	private void AcquireTargets()
	{
		for (var i = 0; i < _armada.Count; i++)
		{
			var self = _armada[i];
			var best = -1;
			var bestSq = float.MaxValue;
			for (var j = 0; j < _armada.Count; j++)
			{
				if (_armada[j].Faction == self.Faction) continue;
				var ds = NVec2.DistanceSquared(self.Position, _armada[j].Position);
				if (ds >= bestSq) continue;
				bestSq = ds;
				best = j;
			}
			self.Node.TargetIndex = best;
		}
	}


	// 1) Every ship holds a drifting station around the fleet objective,
	//    wanders a little, and shies away from anyone too close.
	private void SteerShips(float dt)
	{
		_ships.For((this, dt), (u, ref ship, ref motion) =>
		{
			var (demo, step) = u;
			if (!IsInstanceValid(ship)) return;

			var desired = motion.Course;
			var objective = ship.Faction?.FleetObjective;
			if (objective != null)
			{
				ship.OrbitAngle += ship.OrbitSpeed * step;
				var op = objective.GlobalPosition;
				var station = new NVec2(
					op.X + MathF.Cos(ship.OrbitAngle) * ship.OrbitRadius,
					op.Y + MathF.Sin(ship.OrbitAngle) * ship.OrbitRadius);
				var to = station - motion.Position;
				desired = MathF.Atan2(to.Y, to.X);
			}

			// A lazy hand on the wheel breaks up straight lines.
			desired += MathF.Sin(demo._time * ship.WanderFrequency + ship.WanderPhase) * 0.3f;

			// Keep formation with friends; merely avoid scraping hulls with
			// enemies — fleets should close to brawling range, not stand off.
			var avoid = NVec2.Zero;
			var crowding = 0f;
			foreach (var other in demo._armada)
			{
				if (other.Node == ship) continue;
				var friendly = other.Faction == ship.Faction;
				var away = motion.Position - other.Position;
				var range = (ship.Radius + other.Radius) * (friendly ? 4.5f : 2.2f);
				var distSq = away.LengthSquared();
				if (distSq >= range * range || distSq < 1e-3f) continue;
				var dist = MathF.Sqrt(distSq);
				var weight = 1f - dist / range;
				avoid += away / dist * weight;
				if (friendly) crowding += weight;
			}

			if (avoid != NVec2.Zero)
			{
				var heading = new NVec2(MathF.Cos(desired), MathF.Sin(desired)) + avoid * 2.5f;
				desired = MathF.Atan2(heading.Y, heading.X);
			}

			var diff = Mathf.Wrap(desired - motion.Course, -Mathf.Pi, Mathf.Pi);
			motion.Course += Mathf.Clamp(diff, -ship.TurnRate * step, ship.TurnRate * step);

			// Ships bleed speed in hard turns, ease off a little in a crowd of
			// friends, and surge back on open water.
			var throttle = 1f - 0.5f * Mathf.Min(1f, Mathf.Abs(diff) / Mathf.Pi);
			throttle *= Mathf.Clamp(1f - crowding * 0.25f, 0.55f, 1f);
			motion.Speed = Mathf.MoveToward(motion.Speed, ship.Speed * throttle, ship.Speed * 2f * step);

			var dir = new NVec2(MathF.Cos(motion.Course), MathF.Sin(motion.Course));
			motion.Position += motion.Speed * step * dir;

			ship.CurrentVelocity = dir * motion.Speed;
			ship.GlobalPosition = new Vector2(motion.Position.X, motion.Position.Y);
			ship.Rotation = motion.Course;

			// Wounded ships trail smoke from the stern.
			if (ship.Health <= ship.MaxHealth / 2)
			{
				ship.SmokeTimer -= step;
				if (ship.SmokeTimer <= 0f)
				{
					var rand = Random.Shared;
					var severity = 1f - (float) ship.Health / ship.MaxHealth;
					ship.SmokeTimer = Mathf.Lerp(0.5f, 0.12f, severity);
					demo._bursts.Add(new Vfx
					{
						Kind = VfxKind.Smoke,
						Position = motion.Position - dir * (ship.Radius * 0.9f),
						Velocity = -dir * motion.Speed * 0.1f + new NVec2(rand.NextSingle() * 12f - 6f, rand.NextSingle() * 12f - 6f),
						Rotation = rand.NextSingle() * Mathf.Tau,
						Spin = rand.NextSingle() - 0.5f,
						Scale = ship.Radius * 0.5f,
						Growth = 16f,
						Lifetime = 1.2f + rand.NextSingle(),
						Tint = new Color(0.25f, 0.24f, 0.23f, 0.3f + 0.5f * severity),
					});
				}
			}
		});
	}


	// 1b) Wrecks are out of the fight but not out of the water: they coast
	//     on their last heading, slowly losing way as they settle.
	private void DriftWrecks(float dt)
	{
		_wrecks.For(dt, (step, ref ship, ref motion) =>
		{
			if (!IsInstanceValid(ship)) return;

			motion.Speed *= MathF.Exp(-0.9f * step);
			var dir = new NVec2(MathF.Cos(motion.Course), MathF.Sin(motion.Course));
			motion.Position += motion.Speed * step * dir;

			ship.CurrentVelocity = dir * motion.Speed;
			ship.GlobalPosition = new Vector2(motion.Position.X, motion.Position.Y);
		});
	}


	// 2) Turrets traverse within their mounted arc at finite speed, lead
	//    their target, and fire only once the barrel has come to bear.
	private void LayGuns(float dt)
	{
		_guns.For((this, dt), (u, ref gun) =>
		{
			var (demo, step) = u;
			if (!IsInstanceValid(gun)) return;

			var ship = gun.OwnerShip;
			if (ship == null || !IsInstanceValid(ship) || ship.Sinking) return;

			gun.Cooldown = Mathf.Max(0f, gun.Cooldown - step);

			// The barrel kicks back on firing and eases home.
			gun.Recoil = Mathf.Max(0f, gun.Recoil - step * 3f);
			gun.Offset = gun.BaseOffset - new Vector2(5f * gun.Recoil * gun.Recoil, 0f);

			// The ship's designated target, if this gun can reach it.
			var found = ship.TargetIndex >= 0 && ship.TargetIndex < demo._armada.Count;
			ShipInfo target = default;
			var bestSq = 0f;
			var origin = gun.GlobalPosition;
			if (found)
			{
				target = demo._armada[ship.TargetIndex];
				var dx = target.Position.X - origin.X;
				var dy = target.Position.Y - origin.Y;
				bestSq = dx * dx + dy * dy;
				found = bestSq < gun.Range * gun.Range;
			}

			var restAngle = ship.Rotation + gun.MountRotation;
			float goal;
			if (found)
			{
				// Lead the target: shells are slow, ships are not.
				var distance = MathF.Sqrt(bestSq);
				var predicted = target.Position + target.Velocity * (distance / gun.BulletSpeed);
				goal = MathF.Atan2(predicted.Y - origin.Y, predicted.X - origin.X);
			}
			else
			{
				goal = restAngle; // train back to rest
			}

			var arcLocal = Mathf.Clamp(Mathf.Wrap(goal - restAngle, -Mathf.Pi, Mathf.Pi), gun.FiringArc.X, gun.FiringArc.Y);
			var traverse = Mathf.Wrap(restAngle + arcLocal - gun.GlobalRotation, -Mathf.Pi, Mathf.Pi);
			gun.Rotation += Mathf.Clamp(traverse, -gun.Turning * step, gun.Turning * step);

			if (!found || gun.Cooldown > 0f) return;

			// Hold fire until the barrel actually bears on the target.
			var offTarget = Mathf.Abs(Mathf.Wrap(goal - gun.GlobalRotation, -Mathf.Pi, Mathf.Pi));
			if (offTarget > 0.1f) return;

			gun.Cooldown = gun.ReloadTime * (0.8f + 0.4f * Random.Shared.NextSingle());
			gun.Recoil = 1f;
			demo.FireGun(gun, ship, MathF.Sqrt(bestSq));
		});
	}


	private void FireGun(Gun gun, Ship ship, float distance)
	{
		var rand = Random.Shared;
		var spread = Mathf.DegToRad(gun.SpreadDegrees);
		var barrels = Math.Max(1, gun.Muzzles.Count);

		for (var barrel = 0; barrel < barrels; barrel++)
		{
			var muzzle = gun.Muzzles.Count > 0
				? gun.Muzzles[barrel].GlobalPosition
				: gun.ToGlobal(new Vector2(gun.BarrelLength, 0f));

			var angle = gun.GlobalRotation + (rand.NextSingle() * 2f - 1f) * spread;
			var speed = gun.BulletSpeed * (0.95f + 0.1f * rand.NextSingle());

			// Ranging error: shells fall short or sail long, bracketing the
			// target with splashes the way a real salvo would.
			var life = distance / speed * (0.9f + 0.25f * rand.NextSingle());

			var position = new NVec2(muzzle.X, muzzle.Y);
			var velocity = new NVec2(MathF.Cos(angle), MathF.Sin(angle)) * speed;

			_salvo.Add(new Projectile
			{
				Position = position,
				Velocity = velocity,
				Life = life,
				MaxLife = life,
				Damage = gun.Damage,
				Faction = ship.Faction,
				Color = ship.Faction.Color.Lightened(0.4f),
			});

			_bursts.Add(new Vfx
			{
				Kind = VfxKind.MuzzleFlash,
				Position = position,
				Velocity = ship.CurrentVelocity,
				Rotation = angle,
				Scale = 22f + rand.NextSingle() * 8f,
				Lifetime = 0.18f,
				Tint = Colors.White,
			});

			if (rand.NextSingle() < 0.6f)
			{
				_bursts.Add(new Vfx
				{
					Kind = VfxKind.Smoke,
					Position = position,
					Velocity = ship.CurrentVelocity * 0.6f + velocity * 0.03f,
					Rotation = rand.NextSingle() * Mathf.Tau,
					Spin = rand.NextSingle() - 0.5f,
					Scale = 10f,
					Growth = 18f,
					Lifetime = 0.8f + rand.NextSingle() * 0.6f,
					Tint = new Color(0.85f, 0.85f, 0.88f, 0.45f),
				});
			}
		}
	}


	// 3) Shells fly, hit, or fall into the sea — and every one draws itself
	//    as a single tracer instance in the batch.
	private void FlyShells(float dt)
	{
		_shells.For((this, dt), (u, in entity, ref shell) =>
		{
			var (demo, step) = u;

			shell.Position += shell.Velocity * step;
			shell.Life -= step;

			if (shell.Life <= 0f)
			{
				// Fell in the water — a miss still tells a story.
				demo.AddSplash(shell.Position);
				entity.Despawn();
				return;
			}

			foreach (var target in demo._armada)
			{
				if (target.Faction == shell.Faction) continue;
				if (!IsInstanceValid(target.Node) || target.Node.Sinking) continue;
				if (NVec2.DistanceSquared(shell.Position, target.Position) > target.Radius * target.Radius) continue;

				target.Node.TakeDamage(shell.Damage);
				demo.AddHit(shell.Position);
				entity.Despawn();
				return;
			}

			// A quad stretched along the flight path, fading in at the muzzle
			// and out as the shell falls.
			var heading = MathF.Atan2(shell.Velocity.Y, shell.Velocity.X);
			var fade = Mathf.Min(shell.Life * 4f, 1f) * Mathf.Min((shell.MaxLife - shell.Life) * 6f + 0.2f, 1f);
			var color = shell.Color;
			color.A *= fade;
			demo._tracers.Add(new Vector2(shell.Position.X, shell.Position.Y), heading, new Vector2(30f, 8f), color);
		});
	}


	// 4) Effects age, drift, grow, and despawn; each visible one becomes an
	//    instance in its family's MultiMesh with its frame picked in-shader.
	private void AgeAndRenderVfx(float dt)
	{
		_vfx.For((this, dt), (u, in entity, ref fx) =>
		{
			var (demo, step) = u;

			fx.Age += step;
			if (fx.Age >= fx.Lifetime)
			{
				entity.Despawn();
				return;
			}

			if (fx.Age < 0f) return; // still on its fuse

			fx.Position += fx.Velocity * step;
			fx.Rotation += fx.Spin * step;
			fx.Scale += fx.Growth * step;

			var progress = fx.Age / fx.Lifetime;
			var position = new Vector2(fx.Position.X, fx.Position.Y);

			switch (fx.Kind)
			{
				case VfxKind.MuzzleFlash:
					demo._flashes.Add(position, fx.Rotation, new Vector2(fx.Scale * 1.5f, fx.Scale), fx.Tint, progress);
					break;

				case VfxKind.Hit:
					demo._hits.Add(position, fx.Rotation, new Vector2(fx.Scale, fx.Scale), fx.Tint, progress);
					break;

				case VfxKind.Explosion:
					demo._explosions.Add(position, fx.Rotation, new Vector2(fx.Scale, fx.Scale), fx.Tint, progress);
					break;

				default: // Smoke and Splash share the puff sheet, tinted differently
					var tint = fx.Tint;
					tint.A *= 1f - progress;
					demo._smoke.Add(position, fx.Rotation, new Vector2(fx.Scale, fx.Scale), tint, progress);
					break;
			}
		});
	}


	internal void AddHit(NVec2 position)
	{
		var rand = Random.Shared;
		_bursts.Add(new Vfx
		{
			Kind = VfxKind.Hit,
			Position = position,
			Rotation = rand.NextSingle() * Mathf.Tau,
			Scale = 20f + rand.NextSingle() * 14f,
			Growth = 30f,
			Lifetime = 0.4f + rand.NextSingle() * 0.2f,
			Tint = Colors.White,
		});
	}


	internal void AddSplash(NVec2 position)
	{
		var rand = Random.Shared;
		_bursts.Add(new Vfx
		{
			Kind = VfxKind.Splash,
			Position = position,
			Rotation = rand.NextSingle() * Mathf.Tau,
			Spin = (rand.NextSingle() - 0.5f) * 2f,
			Scale = 8f + rand.NextSingle() * 10f,
			Growth = 26f,
			Lifetime = 0.6f + rand.NextSingle() * 0.5f,
			Tint = new Color(0.8f, 0.9f, 1f, 0.8f),
		});
	}


	// A kill is an event: main blast, secondaries marching down the hull on
	// staggered fuses, and a smoke column that outlives the wreck.
	internal void OnShipSunk(Ship ship)
	{
		var rand = Random.Shared;
		var position = new NVec2(ship.GlobalPosition.X, ship.GlobalPosition.Y);
		var along = new NVec2(MathF.Cos(ship.Rotation), MathF.Sin(ship.Rotation));

		_bursts.Add(new Vfx
		{
			Kind = VfxKind.Explosion,
			Position = position,
			Rotation = rand.NextSingle() * Mathf.Tau,
			Scale = ship.Radius * 3f,
			Growth = ship.Radius,
			Lifetime = 0.9f,
			Tint = Colors.White,
		});

		var secondaries = 2 + (int) (ship.Radius / 15f);
		for (var i = 0; i < secondaries; i++)
		{
			var offset = along * ((rand.NextSingle() * 2f - 1f) * ship.Radius)
				+ new NVec2(-along.Y, along.X) * ((rand.NextSingle() * 2f - 1f) * ship.Radius * 0.3f);
			_bursts.Add(new Vfx
			{
				Kind = VfxKind.Explosion,
				Position = position + offset,
				Velocity = ship.CurrentVelocity * 0.3f,
				Rotation = rand.NextSingle() * Mathf.Tau,
				Scale = ship.Radius * (1f + rand.NextSingle()),
				Growth = ship.Radius * 0.5f,
				Age = -(0.1f + rand.NextSingle() * 0.8f),
				Lifetime = 0.6f + rand.NextSingle() * 0.3f,
				Tint = Colors.White,
			});
		}

		for (var i = 0; i < 5; i++)
		{
			_bursts.Add(new Vfx
			{
				Kind = VfxKind.Smoke,
				Position = position + new NVec2(rand.NextSingle() * 2f - 1f, rand.NextSingle() * 2f - 1f) * ship.Radius * 0.5f,
				Velocity = new NVec2(rand.NextSingle() * 16f - 8f, rand.NextSingle() * 16f - 18f),
				Rotation = rand.NextSingle() * Mathf.Tau,
				Spin = rand.NextSingle() - 0.5f,
				Scale = ship.Radius * 0.8f,
				Growth = 22f,
				Age = -rand.NextSingle() * 1.2f,
				Lifetime = 2.5f + rand.NextSingle() * 1.5f,
				Tint = new Color(0.18f, 0.17f, 0.16f, 0.85f),
			});
		}
	}


	// 5) Flag capture: each objective tracks faction dominance within its radius.
	private void UpdateObjectives(float dt)
	{
		foreach (var objective in _objectives)
		{
			_presence.Clear();
			var op = objective.GlobalPosition;
			var total = 0;
			foreach (var ship in _armada)
			{
				if (ship.Faction == null) continue;
				var dx = ship.Position.X - op.X;
				var dy = ship.Position.Y - op.Y;
				if (dx * dx + dy * dy > objective.Radius * objective.Radius) continue;
				_presence.TryGetValue(ship.Faction, out var count);
				_presence[ship.Faction] = count + 1;
				total++;
			}

			Admiralty leader = null;
			var leaderCount = 0;
			foreach (var (faction, count) in _presence)
			{
				if (count > leaderCount)
				{
					leaderCount = count;
					leader = faction;
				}
			}

			// Strict majority required — contested objectives stall the timer.
			var uncontested = leader != null && leaderCount > total - leaderCount;

			if (uncontested && objective.Controller != leader)
			{
				objective.Timer += dt;
				if (objective.Timer >= Objective.CaptureTime)
				{
					objective.Controller = leader;
					objective.Timer = 0f;
					objective.Modulate = leader.Color;
					leader.Score++;
				}
			}
			else
			{
				objective.Timer = Mathf.Max(0f, objective.Timer - dt);
			}
		}
	}


	private void UpdateHud()
	{
		var scores = string.Join("   ", _admiralties.Select(a => $"{a.Name} {a.Score}"));
		_hud.Text = $"Ships {_ships.Count}  Shells {_shells.Count}  Effects {_vfx.Count}  FPS {Mathf.RoundToInt(_fps)}\n{scores}";
	}


	private static Texture2D Sheet(string file) =>
		GD.Load<Texture2D>($"res://Battleships/Sprites/Projectiles/{file}");


	// Picks the animation frame from a horizontal strip; INSTANCE_CUSTOM.x
	// carries each instance's 0..1 progress through its lifetime.
	private static Shader StripShader(string blendMode) => new()
	{
		Code = $$"""
			shader_type canvas_item;
			render_mode {{blendMode}}, unshaded;

			uniform float hframes = 1.0;

			void vertex() {
				float frame = clamp(floor(INSTANCE_CUSTOM.x * hframes), 0.0, hframes - 1.0);
				UV = vec2((UV.x + frame) / hframes, UV.y);
			}
			""",
	};


	// Textureless glowing tracer: faction color with a white-hot core.
	private static Shader TracerShader() => new()
	{
		Code = """
			shader_type canvas_item;
			render_mode blend_add, unshaded;

			void fragment() {
				vec2 p = UV * 2.0 - 1.0;
				float d = length(p);
				float glow = pow(max(0.0, 1.0 - d), 1.5);
				float core = pow(max(0.0, 1.0 - d * 1.6), 3.0);
				COLOR = vec4(COLOR.rgb * 1.5 + vec3(core), COLOR.a * glow);
			}
			""",
	};
}
