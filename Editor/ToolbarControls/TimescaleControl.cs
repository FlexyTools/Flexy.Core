using System;
using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class TimescaleControl
{
	static TimescaleControl( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Timescale", OnTestRunGUI, UnityEditorTopToolbar.EPlace.Center ); }
	
	private static void		OnTestRunGUI			( )	
	{
		GUILayout.BeginHorizontal( GUILayout.MaxWidth(300), GUILayout.Height(14) );
		{
			GUILayout.Label( "Timescale" );
			
			var x = (Single)Math.Log10(Time.timeScale); 
			
			var newval = GUILayout.HorizontalSlider( x, -3, 2, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true) );
			
			if( !Mathf.Approximately(newval, x) )
				Time.timeScale = (Single)Math.Pow(10, newval); 
			
			var ts = Time.timeScale;
			
			if( ts >= 10 )
			{
				ts = (Single)Math.Round(ts);
				GUILayout.Label( $"x{ts:F0}", GUILayout.Width(45) );
			}
			else if( ts >= 3 )
			{
				ts = (Single)Math.Round(ts*10)/10f;
				GUILayout.Label( $"x{ts:F1}", GUILayout.Width(45) );
			}
			else if( ts > 0.01 )
			{
				GUILayout.Label( $"x{Time.timeScale:F2}", GUILayout.Width(45) );
			}
			else
			{
				GUILayout.Label( $"x{Time.timeScale:F3}", GUILayout.Width(45) );
			}
			
			if( GUILayout.Button( "R", EditorStyles.toolbarButton, GUILayout.Height(14), GUILayout.Width(20) ) )
			{
				Time.timeScale = 1;
			}
		}
		GUILayout.EndHorizontal( );
	}
}