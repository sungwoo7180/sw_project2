using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class AnimationMapData : ScriptableObject
    {
        public AnimationMaps[] animationMaps = new AnimationMaps[0];
    }

    [System.Serializable]
    public class AnimationMaps
    {
        public string id;
        public AnimationMap[] animationMaps = new AnimationMap[0];
    }
}