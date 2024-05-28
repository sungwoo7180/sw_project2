using UnityEngine;
using UnityEditor;

namespace UFE3D
{
    [CustomEditor(typeof(StanceInfo))]
    public class StanceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Stance File");
            if (GUILayout.Button("Open Character Editor"))
                CharacterEditorWindow.Init();

        }
    }
}