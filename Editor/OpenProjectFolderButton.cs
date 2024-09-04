using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor;

[InitializeOnLoad]
public static class OpenProjectFolderButton
{
	static OpenProjectFolderButton( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( OnTestRunGUI, UnityEditorTopToolbar.EPlace.Right ); }
	
	private static void		OnTestRunGUI			( )	
	{
		if( GUILayout.Button( "Open Project Folder", EditorStyles.toolbarButton, GUILayout.Height(14) ) )
		{
			Application.OpenURL( Application.dataPath.Replace( "/Assets", "" ) );
		}
	}
}