using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Flexy.Core.Binding;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Flexy.Core.Editor.Binding
{
	[CustomEditor(typeof(CallBinder), true), CanEditMultipleObjects]
	public class CallBinderEditor : UnityEditor.Editor 
	{
		private				List<Component>		_methodsComponents;
		private				List<MethodInfo>	_methods;
		private				List<String>		_methodNames;
		private				String[]			_methodNamesNice;

		public override VisualElement CreateInspectorGUI()
		{
			var root = new VisualElement();
			var iterator = serializedObject.GetIterator();
			var enterChildren = true;

			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;

				if (iterator.propertyPath == "m_Script" || 
				    iterator.propertyPath == "_target" || 
				    iterator.propertyPath == "_methodName" || 
				    iterator.propertyPath == "_context")
				{
					continue;
				}

				var propertyField = new PropertyField(iterator);
				propertyField.BindProperty(iterator);
				root.Add(propertyField);
			}

			root.Add(new IMGUIContainer(OnInspectorGUI));

			return root;
		}

		public override		void				OnInspectorGUI		( )				
		{
			serializedObject.Update			( );

			//DrawPropertiesExcluding			( serializedObject, "m_Script", "_target", "_methodName", "_context" );

			var componentProp				= serializedObject.FindProperty( "_target" );
			var methodProp					= serializedObject.FindProperty( "_methodName" );
			var context						= serializedObject.FindProperty( "_context" );

			EditorGUI.BeginChangeCheck		( );
			EditorGUILayout.PropertyField	( componentProp );

			var targetChanged				= false;

			if( EditorGUI.EndChangeCheck ( ) )
			{
				UpdateMethods	( );
				targetChanged	= true;
			}

			if( _methodNames.Count == 0 )
			{
				GUI.color						= Color.red;
				EditorGUILayout.LabelField		( componentProp.objectReferenceValue != null && _methodNames.Count == 0 ? "Target Has No Callable Methods" : "Choose Target First!!!" );
				GUI.color						= Color.white;
			}
			else
			{
				var index		= _methodNames.IndexOf( methodProp.stringValue );

				if( index == -1 )
				{
					index									= 0;
					methodProp.stringValue					= _methodNames[index];
					componentProp.objectReferenceValue		= _methodsComponents[index];
				}
				else if( targetChanged )
				{
					componentProp.objectReferenceValue	= _methodsComponents[index];
				}

				EditorGUI.BeginChangeCheck		( );
				index							= EditorGUILayout.Popup	( "Method", index, _methodNamesNice );
				var methodChanged				= EditorGUI.EndChangeCheck ( );

				if( methodChanged )
				{
					Undo.RecordObjects						( targets, "Target Method Chaged" );
					methodProp.stringValue					= _methodNames[index];
					componentProp.objectReferenceValue		= _methodsComponents[index];
					context.stringValue = String.Empty;
				}

				var @params = _methods[index].GetParameters ( );
				if( @params.Length != 0 )
				{
					if( @params.Length == 1 )
					{
						if( @params[0].ParameterType == typeof(String) )
						{
							context.stringValue		= EditorGUILayout.TextField ( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue );
						}
						else if( @params[0].ParameterType == typeof(Int32) )
						{
							context.stringValue		= EditorGUILayout.IntField  ( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue == "" ? 0 : Int32.Parse( context.stringValue ) ).ToString( );
						}
						else if( @params[0].ParameterType == typeof(Single) )
						{
							context.stringValue		= EditorGUILayout.FloatField( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue == "" ? 0.0f : Single.Parse( context.stringValue ) ).ToString( CultureInfo.CurrentCulture );
						}
						else if( @params[0].ParameterType == typeof(Boolean) )
						{
							context.stringValue		= EditorGUILayout.Toggle( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue != "" && Boolean.Parse( context.stringValue ) ).ToString( );
						}   
						else if (@params[0].ParameterType.IsEnum)
						{
							var type = @params[0].ParameterType;
							context.stringValue		= Convert.ToInt32( EditorGUILayout.EnumPopup( ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse( type, String.IsNullOrEmpty( context.stringValue ) ? Enum.GetNames(type)[0] : context.stringValue ) ) ).ToString( );
						}
						else if (@params[0].ParameterType == typeof(GameObject))
						{
							EditorGUILayout.HelpBox( "This GameObject will be passed to method", MessageType.Info );
						}
						else
						{
							EditorGUILayout.HelpBox( "Method arameter type '"+ @params[0].ParameterType +"' is unsupported", MessageType.Error );
							context.stringValue = "";	
						}
					}
					else
					{
						EditorGUILayout.HelpBox( "Methods with more than 'ONE' parameter is unsupported", MessageType.Error );
						context.stringValue = "";
					}
				}
			}

			serializedObject.ApplyModifiedProperties( );
		}
		private				void				OnEnable			( )				
		{
			if( target == null )
			{
				DestroyImmediate( this );
				return;
			}
			UpdateMethods	( );
		}

		private				void				UpdateMethods		( )				
		{
			var componentProp	= serializedObject.FindProperty( "_target" );

			if( componentProp.objectReferenceValue == null )
			{
				_methodsComponents	= new List<Component>	( );
				_methodNames		= new List<String>		( );
				_methods			= new List<MethodInfo>	( );
				_methodNamesNice	= new String[0];
			}
			else
			{
				var obj				= ((Component)componentProp.objectReferenceValue).gameObject;
				_methodsComponents	= new List<Component>( );
				_methodNames		= new List<String>( );
				_methods			= new List<MethodInfo>	( );
				var nicedNames		= new List<String>( );

				foreach ( var component in obj.GetComponents<MonoBehaviour>( ) )
				{
					if( component == null )
						continue;
					
					var type = component.GetType( );
					do
					{
						foreach( var method in component.GetType( ).GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).Where( m => m.GetCustomAttributes( typeof(CallableAttribute), true ).Length > 0 ) )
						{
							_methodsComponents.Add	( component );
							_methodNames.Add		( method.Name );
							_methods.Add			( method );
							nicedNames.Add			( ObjectNames.NicifyVariableName( component.GetType( ).Name + " - " + method.Name ) );
						}
						
						type = type.BaseType;
					}
					while( type != typeof(Object) );
				}

				_methodNamesNice = nicedNames.ToArray( );
			}
		}
	}
}