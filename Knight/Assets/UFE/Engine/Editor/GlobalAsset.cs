using UnityEditor;

namespace UFE3D
{
    public class GlobalAsset
    {
        [MenuItem("Assets/Create/UFE/Config File")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<GlobalInfo>();
        }
    }
}