using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor;

[InitializeOnLoad]
public static class ToolbarTimescale
{
	static ToolbarTimescale( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( OnTestRunGUI, UnityEditorTopToolbar.EPlace.Center ); }
	
	private static void		OnTestRunGUI			( )	
	{
		GUILayout.BeginHorizontal( GUILayout.MaxWidth(300), GUILayout.Height(14) );
		{
			GUILayout.Label( "Timescale" );
			
			Time.timeScale = GUILayout.HorizontalSlider( Time.timeScale, 0, 10, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true) );
			
			GUILayout.Label( $"{Time.timeScale:F2}" );
			
			if( GUILayout.Button( "R", EditorStyles.toolbarButton, GUILayout.Height(14), GUILayout.Width(20) ) )
			{
				Time.timeScale = 1;
			}
		}
		GUILayout.EndHorizontal( );
	}
}