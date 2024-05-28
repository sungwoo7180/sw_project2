using UnityEngine;
using UnityEditor;

namespace UFE3D
{
	public static class PlayerPrefsEditor
	{
		[MenuItem("Window/UFE/Clear PlayerPrefs")]
		public static void Clear()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}