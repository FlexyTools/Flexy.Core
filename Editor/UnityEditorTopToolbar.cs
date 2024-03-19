using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[InitializeOnLoad]
	public static class UnityEditorTopToolbar
	{
		static UnityEditorTopToolbar( )
		{
			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;
		}

		public static void AddIMGUIContainerToLeftPocket	( Action onGUI, EPlace place ) => AddVisualElementToLeftPocket	( new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1, flexDirection = FlexDirection.Row}}, place );
		public static void AddIMGUIContainerToRightPocket	( Action onGUI, EPlace place ) => AddVisualElementToRightPocket	( new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1, flexDirection = FlexDirection.Row}}, place );
		public static void AddVisualElementToLeftPocket		( VisualElement e, EPlace place )
		{
			( place switch 
			{
				EPlace.Left		=> LeftPocket_Left,
				EPlace.Center	=> LeftPocket_Center,
				EPlace.Right	=> LeftPocket_Right,
			} ).Add( e );
		}
		public static void AddVisualElementToRightPocket	( VisualElement e, EPlace place )
		{
			( place switch 
			{
				EPlace.Left		=> RightPocket_Left,
				EPlace.Center	=> RightPocket_Center,
				EPlace.Right	=> RightPocket_Right,
			} ).Add( e );
		}

		private static readonly VisualElement LeftPocket_Left	= new( ){ name = "Left Pocket - Left",		style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement LeftPocket_Center	= new( ){ name = "Left Pocket - Center",	style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement LeftPocket_Right	= new( ){ name = "Left Pocket - Right",		style = { flexGrow = 1, flexDirection = FlexDirection.RowReverse } };
		
		private static readonly VisualElement RightPocket_Left	= new( ){ name = "Right Pocket - Left",		style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement RightPocket_Center= new( ){ name = "Right Pocket - Center",	style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement RightPocket_Right	= new( ){ name = "Right Pocket - Right",	style = { flexGrow = 1, flexDirection = FlexDirection.RowReverse } };

		private static ScriptableObject _editorToolbarPanel;		
		
		private static void ImguiUI		( Action onGUI )	
		{
			GUILayout.BeginHorizontal( );
			try						{ onGUI( ); }
			catch (Exception ex)	{ Debug.LogException( ex ); }
			GUILayout.EndHorizontal( );
		}
		private static void EditorUpdate( )					
		{
			if ( _editorToolbarPanel != null ) 
				return;
			
			var toolbars		= Resources.FindObjectsOfTypeAll( typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.Toolbar" ) );
			_editorToolbarPanel	= toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;

			if ( _editorToolbarPanel == null ) 
				return;
			
			var root		= _editorToolbarPanel.GetType( ).GetField( "m_Root", BindingFlags.NonPublic | BindingFlags.Instance );
			var rootElement	= root.GetValue( _editorToolbarPanel ) as VisualElement;
			
			var leftToolbar = rootElement.Q( "ToolbarZoneLeftAlign" );
			leftToolbar.Add( LeftPocket_Left );
			leftToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			leftToolbar.Add( LeftPocket_Center );
			leftToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			leftToolbar.Add( LeftPocket_Right );
            
			var rightToolbar = rootElement.Q( "ToolbarZoneRightAlign" );
			
			rightToolbar.Add( RightPocket_Right );
			rightToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			rightToolbar.Add( RightPocket_Center );
			rightToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			rightToolbar.Add( RightPocket_Left );
		}
		
		public enum EPlace
		{
			Left, 
			Center,
			Right
		}
	}
}
