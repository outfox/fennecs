using Godot;

namespace fennecs.demos.godot.Battleships;

[GlobalClass]
public partial class BattleShipsDemo : Node2D
{
    public World World;
    
    public override void _Ready()
    {
        base._Ready();
        World = new World();
    }
}