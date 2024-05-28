using UnityEngine;
using UnityEditor;

namespace UFE3D
{
	[CustomEditor(typeof(GlobalInfo))]
	public class GlobalEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open UFE Global Config"))
				GlobalEditorWindow.Init();

		}
	}
}