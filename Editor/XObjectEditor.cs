using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flexy.JsonXs;
using Flexy.Utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Flexy.Core.Editor
{
    [CustomEditor( typeof(XObject), true), CanEditMultipleObjects]
    public class XObjectEditor : ScriptableEditor
    {
		protected   		XObject			_target => (XObject)target;
		private				Int32			_dirtyCount;
		private				GUIStyle		_mainComponentStyle;
		private				GUIStyle		_additionalComponentStyle;

		public override		VisualElement	CreateInspectorGUI		( )		
		{
			MakeObjectEditable( );
			var vi = base.CreateInspectorGUI();
			
			vi.hierarchy[0].style.display = DisplayStyle.None;
			
			//currently we draw it using IMGUI, until rewrite to UITK just hide it
			vi.hierarchy[1].style.display = DisplayStyle.None;
			
			//var componentsCi = vi.hierarchy[1];
			//vi.hierarchy.RemoveAt(1);
			//vi.hierarchy.Add(componentsCi);

			
			vi.RegisterCallback<GeometryChangedEvent>( CollapseHeader_OnGeometryChanged );

			var componentsRoot = new VisualElement { name = "XObject Components" };
			
			vi.Add( componentsRoot );
			
			CreateComponentsUI( componentsRoot );
            
			return vi;
		}

		private void CollapseHeader_OnGeometryChanged(GeometryChangedEvent evt)
		{
			var e = (VisualElement)evt.currentTarget;
			e.UnregisterCallback<GeometryChangedEvent>( CollapseHeader_OnGeometryChanged );

			try { e.parent.parent.hierarchy[0].style.marginTop = -55; }
			catch (Exception exception) { Debug.LogException( exception ); }
		}

		private Int32 componentToRemove = -1;
		
		protected virtual	String			ReplaceUnderscoreWith	=> null;
		protected virtual	Boolean			ShowScriptProperty		=> true;
		protected virtual	void			OnEnable				( )		
        {
			if ( targets.Length != 1 )
				return;

			if( _target.GetType( ).GetCustomAttribute<JsonXSerializeAttribute>( true ) == null )
				return;
			
			var path = AssetDatabase.GetAssetPath( _target );

			if ( !path.EndsWith( ".asset" ) )
				return;

			try		{ AssetDatabase.StartAssetEditing( );
			{
				var ext		= XAssetImporter.GetExtensionForType( _target.GetType( ) );
				var from	= path;
				var to		= path[..^6] + ext;
				
				AssetDatabase.MoveAsset( from, to );
				JsonX.ToJsonFile( _target, to );
			}}
			finally { AssetDatabase.StopAssetEditing( ); }
		}
		protected override	void			OnHeaderGUI				( )		
		{
			MakeObjectEditable( );
			base.OnHeaderGUI( );
			
			var position = GUILayoutUtility.GetRect( 100, 10000, 1, 1 );

			position.yMin -= 28;
			position.yMax -= 9;
			position.xMin += 45;
			
			var guidPosition = position;
			
			if( ShowScriptProperty )
				guidPosition.width *= 0.4f;
			
			var guid = ((XObject) target).Guid;
			
			GUI.Label( guidPosition, guid == default ? "null" : guid.ToString( ) );
			
			if( ShowScriptProperty )
			{
				var scriptPosition = position;
				scriptPosition.xMin += position.width * 0.4f + 15;
				scriptPosition.xMax -= 50;
				EditorGUI.PropertyField( scriptPosition, serializedObject.FindProperty("m_Script"), GUIContent.none );
			}
			
			var buttonPosition = position;
			buttonPosition.xMin = buttonPosition.xMax - 50;
			buttonPosition.xMax -= 4;
			
			if( GUI.Button( buttonPosition, "Dirty" ) )
			{
				foreach ( var o in targets )
				{
					if ( o == null )
						continue;

					EditorUtility.SetDirty( o );
				}
				
				MarkDirty();
			}
		}
		public override		void			OnInspectorGUI			( )		
		{
			base.OnInspectorGUI( );
			
			if( targets.Any( EditorUtility.IsDirty ) )
				MarkDirty( );
			
			if( componentToRemove >= 0 )
			{
				if( _target.Components.IsNull || _target.Components.Count <= componentToRemove )
					return;
				
				Undo.RecordObject( _target, "Remove XComponent: " + _target.Components[componentToRemove]?.GetType().Name );
				_target.RemoveComponentAt( (UInt32)componentToRemove );
				
				try					{ XObject.Internal.OnValidate( _target ); }
				catch (Exception e) { Debug.LogException(e); }
			
				if( _target.GetType( ).GetCustomAttribute<JsonXSerializeAttribute>( true ) == null )
					return;

				if( _target.GetType( ).GetCustomAttribute<JsonXSerializeAttribute>( true ) != null )
				{	
					XAssetImporter.SetDirty( _target );
					XAssetImporter.SaveDirtyAssetsToDisc( );
				}
				
				EditorUtility.SetDirty( _target );
				AssetDatabase.SaveAssetIfDirty( _target );
			}
		}
		
		private				void			OnClickAddComponentMenu	( Object componentTypeObj )	
		{
			var componentType = (Type)componentTypeObj;
			var newInstance = (XComponent)Activator.CreateInstance( componentType ) ;
			Undo.RecordObject( _target, "Add XComponent: " + componentType.Name );
			
			_target.AddComponent( newInstance );

			try					{ XObject.Internal.OnValidate( _target ); }
			catch (Exception e) { Debug.LogException(e); }
			
			if( _target.GetType( ).GetCustomAttribute<JsonXSerializeAttribute>( true ) != null )
			{	
				XAssetImporter.SetDirty( _target );
				XAssetImporter.SaveDirtyAssetsToDisc( );
			}
			
			EditorUtility.SetDirty( _target );
			AssetDatabase.SaveAssetIfDirty( _target );
		}

        protected			void			MarkDirty				( )		
        {
            //Mark objects as dirty
            foreach ( var o in targets )
            {
                if ( o == null )
                    continue;

				if( _target.GetType( ).GetCustomAttribute<JsonXSerializeAttribute>( true ) == null )
					continue;
				
                XAssetImporter.SetDirty( (XObject) o );
            }
        }
        protected			void			MakeObjectEditable		( )		
        {
            // Make imported object editable in inspector
            foreach ( var o in targets )
            {
                if ( o == null )
                    continue;

                o.hideFlags = HideFlags.None;
            }
        }
        
		private				void			CreateComponentsUI		( VisualElement componentsRoot )		
		{
			var isComponentBasedAttr	= target.GetType( ).GetCustomAttribute<XComponentBasedAttribute>( );
			var isComponentBased		= isComponentBasedAttr != null;
			var noMainComponent			= isComponentBasedAttr is { NoMainComponent: true };
	
			//Components list
			var components = serializedObject.FindProperty( "_components" );
			var isOneComponent = components.arraySize==1;

			for ( var i = 0; i < components.arraySize; i++ )
			{
				var comp          = components.GetArrayElementAtIndex( i );
				var componentPath = comp.propertyPath;

				if ( comp.managedReferenceValue == null )				
					componentToRemove = i;

				var componentElement = new VisualElement( ){ name = $"Component {i:D2}" };
				componentsRoot.Add( componentElement );
				
				var index = i;
				
				var copy = comp.Copy();
                copy.Next(true);
				var path = copy.propertyPath;
				
				componentElement.Add( new IMGUIContainer( () => CompoenntHeaderIMGUI(index, comp, isComponentBased, noMainComponent, isOneComponent ) ){ name = "Header", style = { marginLeft = -6, marginRight = -8 }} );
				
				var properties = new PropertyField(comp, "") { name = "PropertyDrawer:" + comp.propertyPath };

				if( index != 0 || noMainComponent )
				{
					properties.style.borderLeftColor = (Color)new Color32(25, 25, 25, 255);
					properties.style.borderLeftWidth = 1;
					properties.style.paddingTop = 2;
					properties.style.paddingLeft = 5;
				}
				
				componentElement.Add( properties );
			}
			
			// Add Component Button
			if( isComponentBased )
				componentsRoot.Add( new IMGUIContainer( AddComponentButtonIMGUI ) { name = "Add Component Button" } );
		}
		private				void			CompoenntHeaderIMGUI( Int32 i, SerializedProperty comp, Boolean iscb, Boolean noMain, Boolean theOnly )
		{
			GUILayout.Space(10);
				
			var isMainComponent = iscb && !noMain && i==0;
			if( isMainComponent )
			{
				GUILayout.BeginHorizontal( );	
				if( _mainComponentStyle == null )
				{
					_mainComponentStyle = new (EditorStyles.boldLabel);
					_mainComponentStyle.fontSize = 30;
				}
				
				var typeName = ObjectNames.NicifyVariableName( comp.managedReferenceFullTypename.Split( '.' ).Last( ) );
				if( ReplaceUnderscoreWith != null ) typeName.Replace("_", ReplaceUnderscoreWith);
				GUILayout.Label( typeName, _mainComponentStyle, GUILayout.Height( 40 ) );	
				
				if( theOnly )
				{
					GUILayout.FlexibleSpace();
					GUI.color = new Color32(255, 200, 200, 255);
					if ( GUILayout.Button( "X", GUILayout.Width( 30 ), GUILayout.Height(30) ) )					
						componentToRemove = i;
					GUI.color = Color.white;
				}
				GUILayout.EndHorizontal( );
				GUILayout.Space(10);
			}
			else
			{
                GUILayout.BeginHorizontal( EditorStyles.helpBox );								// Component title
			
				var typeName = ObjectNames.NicifyVariableName( comp.managedReferenceFullTypename.Split( '.' ).Last( ) );
				if( ReplaceUnderscoreWith != null ) typeName.Replace("_", ReplaceUnderscoreWith);
				
				// if( isComponentBased & i==0 )
				// {
				// 	
				// }
				// else
				{
					if( _additionalComponentStyle == null )
					{
						_additionalComponentStyle = new (EditorStyles.boldLabel);
						_additionalComponentStyle.fontSize = 16;
					}
					GUILayout.Label( typeName, _additionalComponentStyle );
				}
				
				
				
				// if ( GUILayout.Button( "?", GUILayout.Width( 20 ) ) )                                           
				// {
				// 	var scriptId = AssetDatabase.FindAssets( typeName ).FirstOrDefault();
				// 	if ( !String.IsNullOrEmpty( scriptId ) )
				// 	{
				// 		var scriptAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( AssetDatabase.GUIDToAssetPath( scriptId ) );
				// 		EditorGUIUtility.PingObject( scriptAsset );
				// 	}
				// }
				
				//if( (i!=0 | components.arraySize == 1) )
				{
					GUILayout.FlexibleSpace();
					GUI.color = new Color32(255, 200, 200, 255);
					if ( GUILayout.Button( "X", GUILayout.Width( 20 ), GUILayout.Height(20) ) )					
						componentToRemove = i;
					GUI.color = Color.white;
				}
				
				GUILayout.EndHorizontal( );
			}
		}
		private				void			AddComponentButtonIMGUI	( )
		{
			GUILayout.Space( 10 );
			GUILayout.BeginHorizontal(  );
			GUILayout.FlexibleSpace( );
					
			if( GUILayout.Button( " Add Component " , GUILayout.Width( 300 ), GUILayout.Height(35)) )
			{
				var componentsTypesList	= GetComponentTypes( _target );
				var menu				= new GenericMenu(  );
						
				if( componentsTypesList.Count == 0 )
				{
					menu.AddItem( new GUIContent("Empty"), false, null );
				}
				else
				{
					foreach ( var componentType in componentsTypesList )
					{
						menu.AddItem( new GUIContent(componentType.Name), false, OnClickAddComponentMenu, componentType );	
					}
				}
						
				menu.ShowAsContext(  );
			}
			GUILayout.FlexibleSpace( );
			GUILayout.EndHorizontal(  );
		}
		
		protected virtual IReadOnlyCollection<Type> GetComponentTypes( XObject gdiObject )
		{
			var result	= new List<Type>( );
			var list	= gdiObject.GetType(  ).GetCustomAttributes<XComponentAllowAttribute>( );
			
			foreach ( var componentRequiredAttribute in list )
			{
				result.AddRange( TypeCache.GetTypesDerivedFrom( componentRequiredAttribute.BaseXComponentType )
										  .Append( componentRequiredAttribute.BaseXComponentType)
										  .Where( t => !t.IsAbstract && (componentRequiredAttribute.AllowDuplicateComponents || gdiObject.Components.All( c => c.GetType(  ) != t)) ) );
			}

			return result.Distinct( ).ToArray( );
		}
    }
	
	[CustomPropertyDrawer(typeof(XComponent))]
	public class XComponentDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var copy	= property.Copy();
			var propsVE	= new VisualElement(  ){ name = "PropsContainer" };
	
			if( !String.IsNullOrWhiteSpace( preferredLabel ) )
				propsVE.Add( new Label(preferredLabel) );
		
			var depth = copy.depth + 1;
			
			for ( var enterChildren = true; copy.NextVisible( enterChildren ) && copy.depth >= depth; enterChildren = false )
				propsVE.Add( new PropertyField(copy) );
		
			return propsVE;
		}
	}
}
