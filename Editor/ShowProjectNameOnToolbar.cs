using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor
{
	[InitializeOnLoad]
	public class ProjectNameOnToolbar
	{
		static ProjectNameOnToolbar()
		{
			UnityEditorTopToolbar.AddIMGUIContainerToLeftPocket( "ProjectName", OnToolbarGUI, UnityEditorTopToolbar.EPlace.Center );
		}

		static void OnToolbarGUI()
		{
			var style = EditorStyles.label;
			style.richText = true;
		
			if( EditorGUIUtility.isProSkin )
				GUILayout.Label( $"<size=16><color=#888888><b>{Application.productName}</b></color></size>", style );
			else
				GUILayout.Label( $"<size=16><color=#000000><b>{Application.productName}</b></color></size>", style );
		}
	}
}