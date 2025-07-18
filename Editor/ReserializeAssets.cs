using UnityEditor;

namespace Flexy.Core.Editor;

public static class ReserializeAssets
{
	[MenuItem( "Tools/Flexy/Reserialize Project", priority = 2001 )]
	private	static		void		ReserializeProject		( )		
	{
		try
		{
			AssetDatabase.StartAssetEditing		( );
			AssetDatabase.ForceReserializeAssets( );
		}
		finally
		{
			AssetDatabase.StopAssetEditing		( );
		}
	}
}