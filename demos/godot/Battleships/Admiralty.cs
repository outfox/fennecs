using System.Collections.Generic;
using Godot;

namespace fennecs.demos.godot.Battleships;

/// <summary>
/// AI for the game, controls the ships and their objectives.
/// </summary>
public class Admiralty
{
    public readonly List<Objective> Objectives = [];
    public Objective CurrentObjective;
    
    public Color Color;
    
    public void AddShip(Ship ship)
    {
        ship.Faction = this;
    }
    
    public void AddObjective(Objective objective)
    {
        Objectives.Add(objective);
    }
    
    public void PickObjective()
    {
        if (Objectives.Count == 0) return;

        if (CurrentObjective == null || CurrentObjective.Owner != this) return;

        foreach (var objective in Objectives)
        {
            if (objective.Owner != this) continue;
            if (objective == CurrentObjective) continue;
            
            CurrentObjective = objective;
            return;
        }
    }
}