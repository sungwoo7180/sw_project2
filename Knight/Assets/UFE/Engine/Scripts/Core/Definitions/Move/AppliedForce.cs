using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class AppliedForce : ICloneable
    {
        public int castingFrame;
        public bool resetPreviousVertical;
        public bool resetPreviousHorizontal;
        public bool affectAssists;
        public bool affectCharacter = true;
        public FPVector _force;

        #region trackable definitions
        public bool casted { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}