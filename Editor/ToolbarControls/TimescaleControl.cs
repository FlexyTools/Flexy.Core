using System;
using UnityEditor;
#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class TimescaleControl
{
	#if UNITY_6000_3_OR_NEWER
	[MainToolbarElement("Flexy/Timescale", defaultDockPosition = MainToolbarDockPosition.Middle)]
	public static MainToolbarElement CreateToolbarElement	( )	
	{
		var type = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element = (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
	#else
	static TimescaleControl( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Timescale", OnTestRunGUI, UnityEditorTopToolbar.EPlace.Center ); }
	#endif
	
	private static VisualElement	Creator			( )		
	{
		return new IMGUIContainer(OnTestRunGUI){style = { marginLeft = 10, marginRight = 10}};
	}
	private static void				OnTestRunGUI	( )		
	{
		GUILayout.BeginHorizontal( GUILayout.MaxWidth(300), GUILayout.Height(14) );
		{
			GUILayout.Label( "Ts:" );
			
			var x = (Single)Math.Log10(Time.timeScale); 
			
			var newval = GUILayout.HorizontalSlider( x, -3, 2, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true) );
			
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