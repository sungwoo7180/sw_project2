using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class StanceInfo : ScriptableObject
    {
        public CombatStances combatStance = CombatStances.Stance1;

        public BasicMoves basicMoves = new BasicMoves();
        public MoveInfo[] attackMoves = new MoveInfo[0];
        public AnimationMapData AM_File; // List of animation maps

        public MoveSetData ConvertData()
        {
            MoveSetData moveSet = new MoveSetData();
            moveSet.combatStance = this.combatStance;
            moveSet.basicMoves = this.basicMoves;
            moveSet.attackMoves = this.attackMoves;
            moveSet.AM_File = this.AM_File;

            return moveSet;
        }
    }
}