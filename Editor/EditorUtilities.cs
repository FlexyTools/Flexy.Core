namespace Flexy.Core.Editor;

public static class EditorUtilities
{
	[MenuItem( "Assets/Copy Guid", priority = 20 )]
	private static	void	CopyGuid			( )		
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

	[MenuItem( "Assets/Copy Guid", true )]
	private static	Boolean	ValidateCopyGuid	( )		
	{
		return Selection.activeObject != null && !String.IsNullOrEmpty( AssetDatabase.GetAssetPath( Selection.activeObject ) );
	}
	
	[MenuItem( "Assets/Reserialize All Assets", priority = 40 )]
	private	static	void	ReserializeAllAssets		( )		
	{
		try
		{
			AssetDatabase.StartAssetEditing		();
			AssetDatabase.ForceReserializeAssets();
		}
		finally
		{
			AssetDatabase.StopAssetEditing		();
		}
	}
	[MenuItem( "Assets/Reserialize Selected Assets", priority = 41 )]
	private	static	void	ReserializeSelectedAssets	( )		
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
	
	[MenuItem( "CONTEXT/Object/Ping!" )]
	private static	void	Ping				( MenuCommand command )	
	{
		EditorGUIUtility.PingObject( command.context );
	}
}