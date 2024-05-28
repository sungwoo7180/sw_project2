using UnityEngine;
using UnityEditor;

namespace UFE3D
{
	[CustomEditor(typeof(MoveInfo))]
	[CanEditMultipleObjects]
	public class MoveEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Move Editor"))
				MoveEditorWindow.Init();

		}
	}
}