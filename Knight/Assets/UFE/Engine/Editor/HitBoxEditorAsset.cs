using UnityEditor;

namespace UFE3D
{
    public class HitBoxEditorAsset
    {
        [MenuItem("Assets/Create/UFE/Custom Hit Boxes")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CustomHitBoxesInfo>();
        }
    }
}