using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Flexy.Core.Editor;

[InitializeOnLoad]
public static class FastActionsDropdown
{
	static FastActionsDropdown( )
	{
		UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( OnFastActionGUI, UnityEditorTopToolbar.EPlace.Right );
	}
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void CleanUpRun( ) => _fastActions = new( );
	
	private static List<FastAction> _fastActions = new ( );
	
	public static void		AddFastAction			( String path, Action action )	
	{
		_fastActions.Add( new( path, action ) );	
	}
	public static void		RemoveFastAction		( String path )					
	{
		for ( var i = 0; i < _fastActions.Count; i++ )
		{
			if ( _fastActions[i].Path != path ) 
				continue;
			
			_fastActions.RemoveAt( i );
			return;
		}	
	}
	
	private static void		OnFastActionGUI			( )	
	{
		if( _fastActions.Count == 0 )
			return;
		
		if( GUILayout.Button( "Fast Actions", EditorStyles.toolbarPopup, GUILayout.Width(100) ) )
		{
			var menu = new GenericMenu();
			
			foreach ( var fa in _fastActions )
				menu.AddItem( new( fa.Path ), true, ( ) => fa.Action( ) );
			
			menu.ShowAsContext( );
		}
	}

	private record struct FastAction ( String Path, Action Action );
}