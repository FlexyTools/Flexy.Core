using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor.ToolbarControls
{
	[InitializeOnLoad]
	public class ProjectNameLabel
	{
		static ProjectNameLabel()
		{
			UnityEditorTopToolbar.AddIMGUIContainerToLeftPocket( "ProjectName", OnToolbarGUI, UnityEditorTopToolbar.EPlace.Center );
		}

		static Single _lastTimeCheck;
		static String? _lastProjectName; 

		static void OnToolbarGUI()
		{
			var style = EditorStyles.label;
			style.richText = true;
		
			if (_lastProjectName == null || Time.realtimeSinceStartup > _lastTimeCheck && !EditorApplication.isPlaying)
			{
				_lastProjectName = Application.productName;
				_lastTimeCheck = Time.realtimeSinceStartup + 10;
				
				if (File.Exists("UserSettings/ProjectName.txt"))
				{
					_lastProjectName = File.ReadAllText("UserSettings/ProjectName.txt");
				}
			} 
		
			if( EditorGUIUtility.isProSkin )
				GUILayout.Label( $"<size=16><color=#888888><b>{_lastProjectName}</b></color></size>", style );
			else
				GUILayout.Label( $"<size=16><color=#000000><b>{_lastProjectName}</b></color></size>", style );
		}
	}
}