using System;
using System.Reflection;
using Flexy.Utils;
using Flexy.Utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Flexy.Core.Editor
{
    [CustomEditor( typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class RuntimeUIEditor : UnityEditor.Editor 
    {
	    protected	VisualElement	_root;
	    
	    protected virtual	Boolean	DrawDefaultInspector => true;
	    
        public override VisualElement CreateInspectorGUI( )		
        {
	        _root = new VisualElement{ name = "FlexyContainer:Object Editor Root Element" } ;

	       FillRoot( );
	        
	        return _root;
        }
        
        protected			void	FillRoot			( )		
        {
	        if( DrawDefaultInspector )
		        InspectorElement.FillDefaultInspector( _root, this.serializedObject , this );
		
	        _root.hierarchy.Add( new IMGUIContainer( ExposedPropsAndMethodsGUI ){ name = "FlexyContainer:Exposed Properties And Methods" } );
	        
	        var ac = (Action)OnInspectorGUI;
	        
	        if( ac.Method.DeclaringType != typeof(ObjectEditor) )
		        _root.hierarchy.Add( new IMGUIContainer( DrawInspectorGUI ){ name = "FlexyContainer:On Inspector GUI" } ); 
        }
        
        private  			void	DrawInspectorGUI	( )		
        {
	        serializedObject.UpdateIfRequiredOrScript( );
	        OnInspectorGUI( );
	        serializedObject.ApplyModifiedProperties( );
        }
        public  override	void	OnInspectorGUI		( )		{ }
        
        public				void	ExposedPropsAndMethodsGUI	( )				
        {
			if( targets.Length == 1 )
				DrawRuntimeGUI(target);
        }
        public				void	DrawRuntimeGUI			( Object obj )	
        { 
			var methodInfos		= obj.GetType( ).GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
 
			foreach ( var methodInfo in methodInfos )
			{
				var attribute = methodInfo.GetCustomAttribute<RuntimeInspectorUIAttribute>( );
				if ( attribute != null )
				{
					methodInfo.Invoke( obj, Array.Empty<Object>() );
					
					if( attribute.Repaint )
						Repaint( );
				}
			}
		}
	}
}