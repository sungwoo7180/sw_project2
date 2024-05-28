using UnityEngine;
using UnityEditor;

namespace UFE3D
{
	[CustomEditor(typeof(AIInfo))]
	public class AIEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open A.I. Editor"))
				AIEditorWindow.Init();

		}
	}

}