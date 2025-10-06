using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flexy.Core.Binding;
using UnityEditor;
using UnityEngine;
using Binder = Flexy.Core.Binding.Binder;
using Object = System.Object;

namespace Flexy.Core.Editor.Binding
{
	[CustomPropertyDrawer(typeof(Binder.BindSource), true)]
	public class BindSourcePropertyDrawer : PropertyDrawer
	{
		private		List<Component>		_properties 		= null!;
		private		List<String>		_propertyNames 		= null!;
		private		String[]			_propertyNamesNice 	= null!;
		private		Type				_bindType 			= null!;

		public override Single	GetPropertyHeight	( SerializedProperty property, GUIContent label )
		{
			return 0;
		}
		public override void	OnGUI				( Rect position, SerializedProperty property, GUIContent label )
		{
			if( _bindType == null )
			{
				var attrs			= fieldInfo.GetCustomAttributes ( typeof(BindToAttribute), true );
				
				if( attrs != null && attrs.Length > 0 )
				{
					var attr = (BindToAttribute)attrs[0];
					if( attr.BindToType == typeof(void) )
						attrs = property.serializedObject.targetObject.GetType( ).GetCustomAttributes ( typeof(BindToAttribute), true );
			
					if( attrs != null && attrs.Length > 0 )
					{
						attr			= (BindToAttribute)attrs[0];
						_bindType		= attr.BindToType;
					}
				}
			}
			
			if( _bindType == null )
			{
				GUI.color = Color.red;
				GUILayout.Label( "Property " + property.displayName + " has no BindToAttribute! Add one to go on." );
				GUI.color = Color.white;
				return;
			}

			//property.serializedObject.Update( );

			if( _properties == null || _properties.Count == 0 )
				UpdateMethods( property );

			
			DrawProp( property );
			//property.serializedObject.ApplyModifiedProperties( );
		}
		
		public static void	DrawProp		( SerializedProperty property, Type bindType, out Type memberType, ref List<Component> properties, ref List<String> propertyNames, ref String[] propertyNamesNice )	
		{
			GUILayout.BeginVertical( property.displayName, GUI.skin.window, GUILayout.Height( 20 ) );
			{
				memberType = typeof(void);
				
				var componentProp				= property.FindPropertyRelative( "Component" );
				var memberNameProp				= property.FindPropertyRelative( "MemberName" );
				var paramsProp					= property.FindPropertyRelative( "Params" );
      
				EditorGUI.BeginChangeCheck		( );
				EditorGUILayout.PropertyField	( componentProp, false );

				var targetChanged				= false;

				if( EditorGUI.EndChangeCheck ( ) )
				{
					UpdateMethods	( componentProp, bindType, out properties, out propertyNames, out propertyNamesNice );
					targetChanged	= true;
				}

				if( propertyNames.Count == 0 )
				{
					GUI.color						= Color.red;
					EditorGUILayout.LabelField		( componentProp.objectReferenceValue != null ? "Target Has No Bindable Properties" : "Choose Target First!!!" );
					GUI.color						= Color.white;
				}
				else
				{
					var index		= propertyNames.IndexOf( memberNameProp.stringValue );

					if( index == -1 )
					{
						index								= 0;
						memberNameProp.stringValue			= propertyNames[index];
						componentProp.objectReferenceValue	= properties[index];
					}
					else if( targetChanged )
					{
						componentProp.objectReferenceValue	= properties[index];
					}

					EditorGUI.BeginChangeCheck		( );
					index							= EditorGUILayout.Popup	( "Property", index, propertyNamesNice );

					if( EditorGUI.EndChangeCheck ( ) )
					{
						Undo.RecordObjects						( property.serializedObject.targetObjects, "Target Property Chaged" );
						memberNameProp.stringValue				= propertyNames[index];
						componentProp.objectReferenceValue		= properties[index];
						paramsProp.stringValue					= "";
					}

					var bindTargetType = componentProp.objectReferenceValue.GetType ( );
					var memberName = memberNameProp.stringValue;

					while( bindTargetType != typeof(Object) )
					{
						var method = bindTargetType.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault( methodInfo => methodInfo.Name == memberName && bindType.IsAssignableFrom( methodInfo.ReturnType ) );
						if( method != null )
						{
							var @params = method.GetParameters ( );
							if( @params.Length == 1 )
							{
								var desc = method.GetCustomAttribute<BindableAttribute>( true );
								if( desc != null )
									EditorGUILayout.HelpBox( desc.Description, desc.IsWarning ? MessageType.Warning : MessageType.Info );

								var paramType = @params[0].ParameterType;
								
								if( paramType == typeof(String) )
								{
									EditorGUI.BeginChangeCheck		( );
									var val  	= EditorGUILayout.TextField ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue );
									
									if( EditorGUI.EndChangeCheck ( ) )
										paramsProp.stringValue = val;
								}
								
								else if( paramType == typeof(Single) )
								{
									var val  	= EditorGUILayout.FloatField( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0.0f : Single.Parse( paramsProp.stringValue ) ).ToString( );
									
									if( EditorGUI.EndChangeCheck ( ) )
										paramsProp.stringValue = val;
								}
						
								else if( paramType == typeof(Boolean) )
								{
									var val  	= EditorGUILayout.Toggle( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue != "" && Boolean.Parse( paramsProp.stringValue ) ).ToString( );
									
									if( EditorGUI.EndChangeCheck ( ) )
										paramsProp.stringValue = val;
								}

								else if ( paramType.IsEnum )
								{
									var type = paramType;
									var val  	= Convert.ToInt32( EditorGUILayout.EnumPopup( ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse( type, String.IsNullOrEmpty( paramsProp.stringValue ) ? Enum.GetNames(type)[0] : paramsProp.stringValue ) ) ).ToString( );
									
									if( EditorGUI.EndChangeCheck ( ) )
										paramsProp.stringValue = val;
								}

								else if( paramType == typeof(Int32) )
								{
									var val  	= EditorGUILayout.IntField  ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0 : Int32.Parse( paramsProp.stringValue ) ).ToString( );
									
									if( EditorGUI.EndChangeCheck ( ) )
										paramsProp.stringValue = val;
								}
								
								else if( paramType == typeof(GameObject) || paramType == typeof(BindableDataStore) )
								{
									EditorGUILayout.HelpBox( $"Method arameter '{@params[0].ParameterType.Name} {@params[0].Name}' is automatically provided from this GO", MessageType.Info );
									paramsProp.stringValue = "";
								}

								else
								{
									EditorGUILayout.HelpBox( "Method arameter type '"+ @params[0].ParameterType +"' is unsupported", MessageType.Error );
									paramsProp.stringValue = "";
								}
							}
							else
							{
								EditorGUILayout.HelpBox( "Methods with more than 'ONE' parameter is unsupported", MessageType.Error );
								paramsProp.stringValue = "";
							}

							memberType = method.ReturnType;
							
							break;
						}
					
						var propertyInfo = bindTargetType.GetProperty ( memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
						if( propertyInfo != null )
						{
							paramsProp.stringValue = "";

							memberType = propertyInfo.PropertyType;

							break;
						}

						bindTargetType = bindTargetType.BaseType;
					}
				}
			}
			GUILayout.EndVertical( );
		}
		public static void	UpdateMethods	( SerializedProperty componentProp, Type bindType, out List<Component> properties, out List<String> propertyNames, out String[] propertyNamesNice )					
		{
			if( componentProp.propertyType != SerializedPropertyType.ObjectReference )
			{ 
				Debug.Log		( $"[BindSourcePropertyDrawer] - UpdateMethods: Unsupported Type: {componentProp.propertyType}" );

				properties			= new List<Component>	( );
				propertyNames		= new List<String>		( );
				propertyNamesNice	= new String[0];

				return;
			}

			if( componentProp.objectReferenceValue == null )
			{
				properties			= new List<Component>	( );
				propertyNames		= new List<String>		( );
				propertyNamesNice	= new String[0];
			}
			else
			{
				var obj				= ((Component)componentProp.objectReferenceValue).gameObject;
				properties			= new List<Component>( );
				propertyNames		= new List<String>( );
				var nicedNames		= new List<String>( );

				foreach ( var component in obj.GetComponents<MonoBehaviour>( ) )
				{
					if( component == null )
						continue;
					
					var type = component.GetType( );
					do
					{
						foreach( var propertyInfo in type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
						{
							var attrs = propertyInfo.GetCustomAttributes( typeof(BindableAttribute), true );

							if( attrs.Length == 0 )
								continue;

							if( !bindType.IsAssignableFrom( propertyInfo.PropertyType ) )
								continue;

							properties.Add		( component );
							propertyNames.Add	( propertyInfo.Name );
							nicedNames.Add		( ObjectNames.NicifyVariableName( component.GetType( ).Name + " - " + propertyInfo.Name ) );
						}
						type = type.BaseType;
					}
					while( type != typeof(Object) );

					type = component.GetType( );
					do
					{
						foreach( var methodInfo in component.GetType( ).GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).Where( methodInfo => bindType.IsAssignableFrom( methodInfo.ReturnType ) && methodInfo.GetCustomAttributes( typeof(BindableAttribute), true ).Length > 0 ) )
						{
							properties.Add		( component );
							propertyNames.Add	( methodInfo.Name );

							var @params = methodInfo.GetParameters ( );
							if( @params.Length == 0 )
								nicedNames.Add		( ObjectNames.NicifyVariableName( component.GetType( ).Name + " - " + methodInfo.Name + "( )" ) );
							else
								nicedNames.Add		( ObjectNames.NicifyVariableName( component.GetType( ).Name + " - " + methodInfo.Name + "( " + @params[0].Name + " )" ) );
						}
						
						type = type.BaseType;
					}
					while( type != typeof(Object) );
				}

				propertyNamesNice = nicedNames.ToArray( );
			}
		}
		
		private void	DrawProp		( SerializedProperty property )	
		{
			Type memberType;
			DrawProp	( property, _bindType, out memberType, ref _properties, ref _propertyNames, ref _propertyNamesNice );
		}
		private	void	UpdateMethods	( SerializedProperty property )	
		{
			var componentProp	= property.FindPropertyRelative( "Component" );

			UpdateMethods( componentProp, _bindType, out _properties, out _propertyNames, out _propertyNamesNice );
		}
	}
}