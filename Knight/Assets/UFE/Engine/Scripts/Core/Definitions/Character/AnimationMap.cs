using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class AnimationMap
    {
        public int frame;
        public HitBoxMap[] hitBoxMaps = new HitBoxMap[0];
        public FPVector deltaDisplacement;
    }
}