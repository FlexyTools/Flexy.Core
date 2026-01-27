using UnityEditor;
#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif
using UnityEngine;

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class OpenInExplorerButton
{
	#if UNITY_6000_3_OR_NEWER
	[MainToolbarElement("Flexy/Open In Explorer", defaultDockPosition = MainToolbarDockPosition.Right)]
	public static MainToolbarElement CreateToolbarElement()
	{
#if UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS
		var text = "Finder";
#else
		var text = "Explorer";
#endif
	
		return new MainToolbarButton(new(text), () => Application.OpenURL( Application.dataPath.Replace( "/Assets", "" ) ));
	}
	#else
	static OpenInExplorerButton(){ UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Open In Explorer", OnGUI, UnityEditorTopToolbar.EPlace.Right ); }
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
	#endif
}