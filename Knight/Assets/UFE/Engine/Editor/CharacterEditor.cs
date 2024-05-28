using UnityEngine;
using UnityEditor;

namespace UFE3D
{
	[CustomEditor(typeof(CharacterInfo))]
	public class CharacterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Character Editor"))
				CharacterEditorWindow.Init();

		}
	}
}