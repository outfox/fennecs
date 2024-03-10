using Godot;

namespace fennecs.demos.godot.Battleships;

[GlobalClass]
public partial class BattleShipsDemo : Node2D
{
	public readonly World World = new();

	private double _fps = 120;
	public override void _Process(double delta)
	{
		_fps = _fps * 0.99 + 0.01 * (1.0/delta);


		var ships = World.Query<Ship>().Build();
		ships.For((ref Ship ship) =>
		{
			ship.Position += ship.Transform.X * (float) delta * ship.Speed;
		});


		var guns = World.Query<Gun>().Build();
		var pos = GetGlobalMousePosition();
		guns.For((ref Gun gun, Vector2 aim) =>
		{
			gun.Aim = aim;
			gun.LookAt(gun.Aim);
		}, pos);



		GetNode<Label>("Ui Layer/Label").Text = $"Ships: {ships.Count} Guns: {guns.Count}\n FPS {Mathf.RoundToInt(_fps)}";
	}
}
