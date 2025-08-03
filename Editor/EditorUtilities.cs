using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor;

public static class EditorUtilities
{
	[MenuItem("Assets/Copy Guid", false, 20)]
	private static void CopyGuid( )
	{
		var assetPath = AssetDatabase.GetAssetPath( Selection.activeObject );
		if (String.IsNullOrEmpty( assetPath ))
		{
			Debug.LogWarning( "No valid asset selected." );
			return;
		}

		var guid = AssetDatabase.AssetPathToGUID( assetPath );
		if (!String.IsNullOrEmpty( guid ))
		{
			EditorGUIUtility.systemCopyBuffer = guid;
			Debug.Log($"Guid copied to clipboard: {guid}");
		}
		else
		{
			Debug.LogWarning("Could not retrieve Guid.");
		}
	}

	[MenuItem("Assets/Copy Guid", true)]
	private static Boolean ValidateCopyGuid( )
	{
		return Selection.activeObject != null && !String.IsNullOrEmpty( AssetDatabase.GetAssetPath( Selection.activeObject ) );
	}
	
	[MenuItem( "Assets/Reserialize Assets", priority = 40 )]
	private	static		void		ReserializeAssets		( )		
	{
		var path	= AssetDatabase.GetAssetPath(Selection.activeObject);
		var paths	= new List<String>();
		
		if (!AssetDatabase.IsValidFolder(path))
		{
			paths.Add( path );
		}
		else
        {
			var guids = AssetDatabase.FindAssets("", new[] { path });
			paths.AddRange( guids.Select( AssetDatabase.GUIDToAssetPath ) );
		}
			
		try
		{
			AssetDatabase.StartAssetEditing		();
			AssetDatabase.ForceReserializeAssets(paths);
		}
		finally
		{
			AssetDatabase.StopAssetEditing		();
		}
	}
}