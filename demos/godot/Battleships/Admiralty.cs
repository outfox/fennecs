using System;
using System.Collections.Generic;
using Godot;

namespace fennecs.demos.godot.Battleships;

/// <summary>
/// AI for the game, controls the ships and their objectives.
/// </summary>
public partial class Admiralty : Node2D
{
    [Export] public PackedScene[] Blueprints = [];
    
    [Export] public int MaxFleetValue = 200;
    
    [Export] public float SpawnRadius = 1000f;
    
    private int _fleetValue;

    [Export]    
    public float MinSpawnTime = 0.1f;
    [Export]    
    public float MaxSpawnTime = 1f;
    
    private float _spawnTimer;

    public int Score;
    
    public readonly List<Objective> Objectives = [];
    public Objective FleetObjective;
    
    [Export]
    public Color Color;


    public override void _Ready()
    {
        var ob = GetTree().GetNodesInGroup("Objective");
         
        foreach (var o in ob)
        {
            if (o is not Objective objective) continue;
            RegisterObjective(objective);
        }

        GD.Print(Name, " has ", ob.Count, " objectives");
    }


    public override void _Process(double delta)
    {
        PickFleetObjective();

        RequisitionShips();
    }

    public void RequisitionShips()
    {
        if (_fleetValue >= MaxFleetValue) return;

        _spawnTimer -= (float) GetProcessDeltaTime();
        if (_spawnTimer > 0) return;
        
        _spawnTimer = Random.Shared.NextSingle() * MaxSpawnTime + MinSpawnTime;
        
        var blueprint = Blueprints[Random.Shared.Next(Blueprints.Length)];
        AddShip(blueprint);
    }
    
    public void AddShip(PackedScene blueprint)
    {
        var ship = blueprint.Instantiate<Ship>();
        ship.Faction = this;
        _fleetValue += ship.Points;
        var spawnPosition = GlobalPosition + new Vector2(
            (float) GD.RandRange(-SpawnRadius, SpawnRadius),
            (float) GD.RandRange(-SpawnRadius, SpawnRadius)
            );
        ship.GlobalPosition = spawnPosition;
        AddSibling(ship);
    }
    
    public void RegisterObjective(Objective objective)
    {
        Objectives.Add(objective);
    }
    
    public void ShuffleObjectives()
    {
        // Fisher-Yates shuffle
        for (var i = Objectives.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (Objectives[i], Objectives[j]) = (Objectives[j], Objectives[i]);
        }
    }


    private void PickFleetObjective()
    {
        // Already have an objective. No retreat! No prisoners!
        if (FleetObjective != null && FleetObjective.Owner != this) return;
        
        ShuffleObjectives();

        foreach (var objective in Objectives)
        {
            if (objective == FleetObjective) continue;
            if (objective.Owner == this) continue;
            
            FleetObjective = objective;
            GD.Print("Fleet Objective Updated!", objective.Name);
            return;
        }
    }
}