using UnityEditor;

namespace UFE3D
{
    public class MoveAsset
    {
        [MenuItem("Assets/Create/UFE/Move File")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<MoveInfo>();
        }
    }
}