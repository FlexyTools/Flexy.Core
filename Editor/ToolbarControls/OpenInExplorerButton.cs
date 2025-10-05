using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class OpenInExplorerButton
{
	static OpenInExplorerButton( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Open In Explorer", OnGUI, UnityEditorTopToolbar.EPlace.Right ); }
	
	private static void		OnGUI			( )	
	{
		#if UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
		var text = "Finder";
		#else
		var text = "Explorer";
		#endif
		
		if (GUILayout.Button( text, EditorStyles.toolbarButton, GUILayout.Height(14) ))
		{
			Application.OpenURL( Application.dataPath.Replace( "/Assets", "" ) );
		}
	}
}