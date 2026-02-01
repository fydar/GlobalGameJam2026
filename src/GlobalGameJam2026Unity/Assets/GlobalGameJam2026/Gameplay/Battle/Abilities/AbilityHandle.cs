using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ReticuleTile
{
    public BattleTile Tile { get; set; }
    public int Mode { get; set; }
}

public class ReticuleBuilder
{
    public List<ReticuleTile> Reticule { get; } = new();

    public void AddToReticle(BattleTile tile, int mode)
    {
        Reticule.Add(new ReticuleTile { Tile = tile, Mode = mode });
    }

    public void AddToReticle(IEnumerable<BattleTile> tiles, int mode)
    {
        foreach (var tile in tiles)
        {
            Reticule.Add(new ReticuleTile { Tile = tile, Mode = mode });
        }
    }
}

public class AbilityHandle
{
    public Combatant Combatant { get; }
    public Ability Ability { get; }
    public bool IsUnitSelected { get; set; }
    public bool IsButtonHovered { get; set; }
    public bool IsClicked { get; set; }
    public bool IsCapturedControl { get; set; }
    public bool IsCapturedGameflow { get; set; }

    public BattleTile HoveredTile { get; set; }

    public Action<ReticuleBuilder> BuildReticule;

    public Action<ReticuleBuilder> BuildPreviewReticule;

    public Func<AbilityHandle, BattleTile, IEnumerator> CastCoroutine;

    public AbilityHandle(Combatant combatant, Ability ability)
    {
        Combatant = combatant;
        Ability = ability;
    }
}


