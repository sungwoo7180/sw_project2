using UnityEngine;
using UnityEditor;

namespace UFE3D
{

    [CustomEditor(typeof(CustomHitBoxesInfo))]
    public class HitBoxEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Hit Box Editor"))
                HitBoxEditorWindow.Init();
        }
    }
}