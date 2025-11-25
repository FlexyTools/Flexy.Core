#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Flexy.Core
{
    [CustomEditor( typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class Editor_WithRuntimeGui : Editor 
    {
	    protected	VisualElement	_root = null!;
	    
	    protected virtual	Boolean	DoDrawDefaultInspector => true;
	    
        public override VisualElement CreateInspectorGUI( )		
        {
			_root = new VisualElement{ name = "FlexyContainer:Object Editor Root Element" } ;

			FillRoot( );
	        
			return _root;
        }
        
        protected			void	FillRoot			( )		
        {
	        if( DoDrawDefaultInspector )
		        InspectorElement.FillDefaultInspector( _root, serializedObject, this );
		
	        var ac = (Action)OnInspectorGUI;
	        
	        if( ac.Method.DeclaringType != typeof(Editor_WithRuntimeGui) )
		        _root.hierarchy.Add( new IMGUIContainer( DrawInspectorGUI ){ name = "FlexyContainer:On Inspector GUI" } );
		        
	        _root.hierarchy.Add( new IMGUIContainer( DrawRuntimeGui ){ name = "Flexy Runtime On Gui" } ); 
        }
        
        private  			void	DrawInspectorGUI	( )		
        {
			EditorGUIUtility.labelWidth = 0;
	        EditorGUIUtility.fieldWidth = 0;
	        EditorGUIUtility.hierarchyMode = true;        
	        serializedObject.UpdateIfRequiredOrScript( );
	        OnInspectorGUI( );
	        serializedObject.ApplyModifiedProperties( );
        }
        public  override	void	OnInspectorGUI		( )		{ }
        private				void	DrawRuntimeGui		( )		
        {
	        if (targets.Length != 1 || !EditorApplication.isPlaying || !((MonoBehaviour)target).gameObject.scene.IsValid())
				return;	
         
			var obj = target;
			var type = obj.GetType( );
        
			while ( type != null && type != typeof(Object) )
			{
				var methodInfos		= type.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic );
	 
				foreach ( var methodInfo in methodInfos )
				{
					var attribute = methodInfo.GetCustomAttribute<RuntimeInspectorGuiAttribute>( );
					if (attribute == null) 
						continue;
					
					try{ methodInfo.Invoke( obj, Array.Empty<Object>() ); }
					catch(Exception ex){ Debug.LogException(ex); }
						
					if( attribute.Repaint )
						Repaint( );
				}
				
				type = type.BaseType;
			}
		}
	}
}
#endif