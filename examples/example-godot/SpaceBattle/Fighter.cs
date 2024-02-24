using System;
using fennecs;
using Godot;

namespace examples.godot.SpaceBattle;

[GlobalClass]
public partial class Fighter : Node3D, IEntityNode
{
	private Color _color = new(1, 1, 1);
	public Identity identity { get; set; }
	
	public override void _Ready()
	{
		_color = Color.FromOkHsl(Random.Shared.NextSingle(), 0.7f, 0.5f, 1);
		GetNode<MeshInstance3D>("MeshInstance3D").SetInstanceShaderParameter("Albedo", _color);

		// TODO: find out if can be set on Instantiate.
		Console.WriteLine($"Fighter._Ready(): {identity}");
	}

}
