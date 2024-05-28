using UnityEditor;

namespace UFE3D
{
    public class AIAsset
    {
        [MenuItem("Assets/Create/UFE/AI File")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<AIInfo>();
        }
    }

}