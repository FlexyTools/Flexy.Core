using System;
using System.IO;
using UnityEditor;
#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor.ToolbarControls
{
	[InitializeOnLoad]
	public class ProjectNameLabel
	{
		#if UNITY_6000_3_OR_NEWER
		[MainToolbarElement("Flexy/Project Name", defaultDockPosition = MainToolbarDockPosition.Left)]
		public static MainToolbarElement CreateToolbarElement()
		{
			var type = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
			var element = (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
			return element;
		}
		#else
		static ProjectNameLabel(){ UnityEditorTopToolbar.AddIMGUIContainerToLeftPocket( "ProjectName", OnToolbarGUI, UnityEditorTopToolbar.EPlace.Center ); }
		#endif

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
					_lastProjectName = File.ReadAllText("UserSettings/ProjectName.txt");
			} 
		
			if (EditorGUIUtility.isProSkin)
				GUILayout.Label($"<size=16><color=#888888><b>{_lastProjectName}</b></color></size>", style);
			else
				GUILayout.Label($"<size=16><color=#000000><b>{_lastProjectName}</b></color></size>", style);
		}
		
		private static VisualElement Creator()
		{
			var root = new VisualElement(){style = { flexGrow = 1, alignContent = Align.Center }};
			root.Add(new IMGUIContainer(OnToolbarGUI){style = { flexGrow = 1, marginLeft = 50, marginRight = 50}});
			return root;
		}
	}
}