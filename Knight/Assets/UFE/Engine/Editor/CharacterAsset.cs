using UnityEditor;

namespace UFE3D
{
    public class CharacterAsset
    {
        [MenuItem("Assets/Create/UFE/Character File")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<UFE3D.CharacterInfo>();
        }
    }
}