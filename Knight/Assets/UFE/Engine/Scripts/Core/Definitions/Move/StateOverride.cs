using System;

namespace UFE3D
{
    [Serializable]
    public class StateOverride : ICloneable
    {
        public int castingFrame;
        public int endFrame;
        public PossibleStates state;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}