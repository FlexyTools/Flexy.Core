#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
	        
	        if( ac.Method.DeclaringType != typeof(RuntimeUIEditor) )
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
			if( targets.Length == 1 && EditorApplication.isPlaying )
				DrawRuntimeGUI(target);
        }
        public				void	DrawRuntimeGUI			( Object obj )	
        { 
			var type = obj.GetType( );
        
			while ( type != null && type != typeof(Object) )
			{
				var methodInfos		= type.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic );
	 
				foreach ( var methodInfo in methodInfos )
				{
					var attribute = methodInfo.GetCustomAttribute<RuntimeInspectorUIAttribute>( );
					if ( attribute != null )
					{
						methodInfo.Invoke( obj, Array.Empty<System.Object>() );
						
						if( attribute.Repaint )
							Repaint( );
					}
				}
				
				type = type.BaseType;
			}
		}
	}
}
#endif