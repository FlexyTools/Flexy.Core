using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flexy.AssetRefs;
using Flexy.JsonXs;
using Flexy.Utils.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using File = System.IO.File;

namespace Flexy.Core.Editor
{
	[ScriptedImporter(1, "xasset")]
    public class XAssetImporter : ScriptedImporter
    {
	    [InitializeOnLoadMethod]
	    static void Init( ){ _extensions.Add( ".xasset" ); }

	    internal static readonly	Dictionary<Type,String>	_associations	= new ( );
	    internal static readonly	HashSet<String>			_extensions		= new ( );
	    private static readonly		HashSet<XObject>        _dirtyObjects	= new ( );

	    protected static void RegisterExtensionForAssetType<T>( String extension ) where T: XObject
	    {
		    _extensions.Add( extension );
		    _associations.Add( typeof(T), extension );
	    }
	    internal static String GetExtensionForType( Type type )
	    {
		    while (type != typeof(object))
		    {
			    if( _associations.TryGetValue( type, out var ext ) )
				    return ext;
			    
			    type = type.BaseType;
		    }
		    
		    return ".xasset";
	    }
	    
        public static void SetDirty( XObject obj )
        {
            _dirtyObjects.Add( obj );
        }

        public override void OnImportAsset( AssetImportContext ctx )
        {
	        try
	        {
		        XObject.GlobalDisableValidate ++;
		        
	            var text        = File.ReadAllText( ctx.assetPath );
	            var instance    = (XObject)JsonX.FromJson( text );
				
	            ctx.AddObjectToAsset	( "main", instance );
	            ctx.SetMainObject		( instance );
	        }
	        finally
	        {
		        XObject.GlobalDisableValidate --;
	        }
        }

        public static void SaveDirtyAssetsToDisc()
        {
            if ( _dirtyObjects.Count <= 0 )
                return;

            try
            {
                AssetDatabase.DisallowAutoRefresh(  );
            
                var wasException = false;
                foreach ( var xObject in _dirtyObjects.ToArray( ) )
                {
                    if( !xObject )
                        continue;
					    
                    try
                    {
                        var path = AssetDatabase.GetAssetPath( xObject );

                        //if we try to save object in memory or not imported by this importer than skip it
                        //if ( String.IsNullOrEmpty( path ) || !path.EndsWith( ".gdi" w) )
                        if ( String.IsNullOrEmpty( path )/* || !path.EndsWith( ".xasset") */)
                            continue;

                        //ServerSettingsExporter.Serialize( gdiObject, path );
                        JsonX.ToJsonFile( xObject, path );
                        EditorUtility.ClearDirty(xObject);
                    }
                    catch ( Exception ex )
                    {
                        Debug.LogException( ex );
                        wasException = true;
                    }
                }

                if ( !wasException )
                    _dirtyObjects.Clear( );
            }
            finally
            {
                AssetDatabase.AllowAutoRefresh( );
				AssetDatabase.Refresh( );
            }
        }
    }
    
    internal class GdiPostProcessor : AssetPostprocessor
    {
    	private static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
    	{
    		//Check guids (because of duplicate asset)
    		foreach ( var importedAsset in importedAssets ) 
            {
	            var extension = Path.GetExtension(importedAsset);
	            
	            if ( !XAssetImporter._extensions.Contains( extension ) ) 
		            continue;
	            
	            var xObject		= AssetDatabase.LoadAssetAtPath<XObject>( importedAsset );
	            var metaGuid	= AssetDatabase.GUIDFromAssetPath( importedAsset ).ToHash( );
                
				GUID.TryParse( xObject.Guid, out var xObjectGuid );
				
	            if( xObjectGuid.ToHash( ) != metaGuid )
	            {
		            //Fix asset guid
		            XObject.Editor.ResetGuid( xObject, metaGuid );
		            XAssetImporter.SetDirty( xObject );

		            Debug.Log( $"[GdiPostProcessor]-[OnPostprocessAllAssets] Fixed guid for asset {importedAsset}" );
	            }
            }

    		XAssetImporter.SaveDirtyAssetsToDisc( );
    	}
    }

    internal class SaveGdiDirtyAssets : AssetModificationProcessor
    {
    	static String[] OnWillSaveAssets( String[] paths )
    	{
    		//Just save all dirty objects on Ctrl+S(and like) operation in editor
    		try						{ XAssetImporter.SaveDirtyAssetsToDisc( ); }
    		catch ( Exception ex )	{ Debug.LogException( ex ); }

    		return paths;
    	}
    }
}