using System;
using fennecs;
using Godot;

namespace examples.godot.SpaceBattle;

public struct Faction
{
    public float Hue;
}

[GlobalClass]
public partial class Battle : Node3D
{
    public readonly World World = new();

    [Export] public int Factions = 3;

    [Export] private PackedScene _fighterPrefab;
    
    private int _wave;
    private Entity[] _factions;

    public void SpawnWave(Identity factionIdentity)
    {
        var faction = World.GetComponent<Faction>(factionIdentity);
        
        _wave++;

        for (var i = 0; i < _wave; i++)
        {
//            var fighter = ResourceLoader.Pre<Fighter>("res://SpaceBattle/Fighter.tscn");
        
            World.Spawn()
                .Add(faction, factionIdentity)
                .Id();
        }
    }
    
    public override void _Ready()
    {
        _factions = new Entity[Factions];
        for (var i = 0; i < Factions; i++)
        {
            _factions[i] = World.Spawn()
                .Add(new Faction {Hue = i / (float) Factions})
                .Id();
        }
    }
}
