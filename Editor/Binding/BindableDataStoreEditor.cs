using System;
using System.Collections.Generic;
using System.Reflection;
using Flexy.Core.Binding;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Flexy.Core.Editor.Binding
{
	//[CustomEditor(typeof(BindableDataStore), true), CanEditMultipleObjects]
	public class BindableDataStoreEditor : UnityEditor.Editor
	{
		protected	List<Component>		_properties;
		protected	List<String>		_propertyNames;
		protected	String[]			_propertyNamesNice;
		protected	String[]			_excludingProperties = {"m_Script", "_target", "_memberName", "_params"};
		protected	Type				_memberType;

		public override		void				OnInspectorGUI		( )				
		{
			serializedObject.Update			( );

			var componentProp				= serializedObject.FindProperty( "_target" );
			var propertyProp				= serializedObject.FindProperty( "_memberName" );
			var paramsProp					= serializedObject.FindProperty( "_params" );

			EditorGUI.BeginChangeCheck		( );
			EditorGUILayout.PropertyField	( componentProp );

			var targetChanged				= false;

			if( EditorGUI.EndChangeCheck ( ) )
			{
				UpdateMethods	( );
				targetChanged	= true;
			}

			if( _propertyNames.Count == 0 )
			{
				GUI.color						= Color.red;
				EditorGUILayout.LabelField		( componentProp.objectReferenceValue != null ? "Target Has No Bindable Properties" : "Choose Target First!!!" );
				GUI.color						= Color.white;
			}
			else
			{
				var index		= _propertyNames.IndexOf( propertyProp.stringValue );

				if( index == -1 )
				{
					index								= 0;
					propertyProp.stringValue			= _propertyNames[index];
					componentProp.objectReferenceValue	= _properties[index];
				}
				else if( targetChanged )
				{
					componentProp.objectReferenceValue	= _properties[index];
				}

				EditorGUI.BeginChangeCheck		( );
				index							= EditorGUILayout.Popup	( "Property", index, _propertyNamesNice );

				if( EditorGUI.EndChangeCheck ( ) )
				{
					Undo.RecordObjects						( targets, "Target Property Chaged" );
					propertyProp.stringValue				= _propertyNames[index];
					componentProp.objectReferenceValue		= _properties[index];
					paramsProp.stringValue					= "";
				}

				var bindTargetType = componentProp.objectReferenceValue.GetType ( );
				var memberName = propertyProp.stringValue;

				while( bindTargetType != typeof(Object) )
				{
					var methodInfo = bindTargetType.GetMethod ( memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
					if( methodInfo != null )
					{
						var @params = methodInfo.GetParameters ( );
						if( @params.Length == 1 )
						{
							var desc = methodInfo.GetCustomAttribute<BindableAttribute>( true );
							if( desc != null && !String.IsNullOrWhiteSpace( desc.Description ) )
								EditorGUILayout.HelpBox( desc.Description, desc.IsWarning ? MessageType.Warning : MessageType.Info );

							if( @params[0].ParameterType == typeof(String) )
								paramsProp.stringValue		= EditorGUILayout.TextField ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue );

							else if( @params[0].ParameterType == typeof(Int32) )
								paramsProp.stringValue		= EditorGUILayout.IntField  ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0 : Int32.Parse( paramsProp.stringValue ) ).ToString( );
		
							else if( @params[0].ParameterType == typeof(Single) )
								paramsProp.stringValue		= EditorGUILayout.FloatField( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0.0f : Single.Parse( paramsProp.stringValue ) ).ToString( );
						
							else if( @params[0].ParameterType == typeof(Boolean) )
								paramsProp.stringValue		= EditorGUILayout.Toggle( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue != "" && Boolean.Parse( paramsProp.stringValue ) ).ToString( );

							else if (@params[0].ParameterType.IsEnum)
							{
								var type = @params[0].ParameterType;
								paramsProp.stringValue = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse(type, paramsProp.stringValue == "" ? Enum.GetNames(type)[0] : paramsProp.stringValue)).ToString( );
							}
						}
						else
						{
							paramsProp.stringValue = "";
						}
						
						_memberType = methodInfo.ReturnType;
						
						if( _memberType == typeof(Enum) ) //Try to get real enum type
						{
							try
							{
								var val = methodInfo.Invoke( componentProp.objectReferenceValue, null );
								_memberType = val.GetType( );
							}
							catch
							{ }
						}

						break;
					}
					
					var propertyInfo = bindTargetType.GetProperty ( memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
					if( propertyInfo != null )
					{
						paramsProp.stringValue = "";

						_memberType = propertyInfo.PropertyType;

						if( _memberType == typeof(Enum) ) //Try to get real enum type
						{
							try
							{
								var val = propertyInfo.GetValue( componentProp.objectReferenceValue, null );
								_memberType = val.GetType( );
							}
							catch
							{ }
						}
						
						break;
					}

					bindTargetType = bindTargetType.BaseType;
				}
                
                serializedObject.ApplyModifiedProperties();

				DrawBinderProperties();
				DrawCustomProperties();
			}

			serializedObject.ApplyModifiedProperties( );
		}
		
		protected virtual	void				OnEnable			( )
		{
			UpdateMethods	( );
		}

		private				void				UpdateMethods	( )					
		{
			var attrs			= target.GetType( ).GetCustomAttributes ( typeof(BindToAttribute), true );
			var bindType		= ((BindToAttribute)attrs[0]).BindToType;

			var componentProp	= serializedObject.FindProperty( "_target" );

			BindSourcePropertyDrawer.UpdateMethods( componentProp, bindType, out _properties, out _propertyNames, out _propertyNamesNice );
		}

		protected virtual void DrawBinderProperties( )
		{
			DrawPropertiesExcluding			( serializedObject, GetExcludingProperties());
		}

		protected virtual void DrawCustomProperties()
		{
		}
		
		protected virtual String[] GetExcludingProperties()
		{
			return _excludingProperties;
		}
	}
}