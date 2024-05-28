using System;

namespace UFE3D
{
    [Serializable]
    public class ChallengeModeOptions : ICloneable
    {
        public string challengeName = "";
        public string description = "";
        public UFE3D.CharacterInfo character;
        public UFE3D.CharacterInfo opCharacter;
        public SimpleAIBehaviour ai;
        public bool isCombo;
        public bool aiOpponent;
        public ChallengeAutoSequence challengeSequence;
        public bool autoMoveNext;
        public bool reloadMatch;
        public bool skipOutro;
        public float nextChallengeDelay = .6f;
        public bool actionListToggle;
        public ActionSequence[] actionSequence = new ActionSequence[0];

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}