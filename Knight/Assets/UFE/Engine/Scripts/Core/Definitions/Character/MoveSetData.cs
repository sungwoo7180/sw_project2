using UnityEngine;
using System;
using UFE3D;

[System.Serializable]
public class MoveSetData : ICloneable
{
    public CombatStances combatStance = CombatStances.Stance1; // This move set combat stance

    public BasicMoves basicMoves = new BasicMoves(); // List of basic moves
    public MoveInfo[] attackMoves = new MoveInfo[0]; // List of attack moves
    public AnimationMapData AM_File; // List of animation maps

    [HideInInspector] public bool enabledBasicMovesToggle;
    [HideInInspector] public bool basicMovesToggle;
    [HideInInspector] public bool attackMovesToggle;


    public StanceInfo ConvertData()
    {
        StanceInfo stanceData = ScriptableObject.CreateInstance<StanceInfo>();
        stanceData.combatStance = this.combatStance;
        stanceData.basicMoves = this.basicMoves;
        stanceData.attackMoves = this.attackMoves;
        stanceData.AM_File = this.AM_File;

        return stanceData;
    }

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}