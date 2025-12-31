using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Flexy.Core.Binding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Binder = Flexy.Core.Binding.Binder;
using Label = UnityEngine.UIElements.Label;
using Object = System.Object;

namespace Flexy.Core.Editor.Binding;

[CustomPropertyDrawer(typeof(Binder.BindSource), true)]
public class BindSourceDrawer : PropertyDrawer
{
	internal const	String	AllowBindToAnyMemberKey	= $"Flexy/Core/Binders/AllowBindToAnyMember";
	private			Type?	_bindType 			= null;
		
	public override void	OnGUI				( Rect position, SerializedProperty property, GUIContent label )	
	{
		if (_bindType == null)
		{
			var attr	= fieldInfo.GetCustomAttribute<BindToAttribute>(true);
				
			if (attr != null)
			{
				if( attr.BindToType == typeof(void) )
					attr = property.serializedObject.targetObject.GetType( ).GetCustomAttribute<BindToAttribute>(true);
			
				if (attr != null)
					_bindType		= attr.BindToType;
			}
		}
			
		if (_bindType == null)
		{
			GUI.color = Color.red;
			GUILayout.Label( "Property " + property.displayName + " has no BindToAttribute! Add one to go on." );
			GUI.color = Color.white;
			return;
		}

		DrawProp	( property, _bindType, out _ );
		GUILayout.Space(10);
	}
	public override Single	GetPropertyHeight	( SerializedProperty property, GUIContent label )					
	{
		return 0;
	}
		
	public static	void	UpdateMethods		( SerializedProperty componentProp, Type bindType, out List<Component> properties, out List<String> propertyNames, out String[] propertyNamesNice )					
	{
		if (componentProp.propertyType != SerializedPropertyType.ObjectReference)
		{ 
			Debug.Log		( $"[BindSourcePropertyDrawer] - UpdateMethods: Unsupported Type: {componentProp.propertyType}" );

			properties			= new List<Component>();
			propertyNames		= new List<String>();
			propertyNamesNice	= Array.Empty<String>();

			return;
		}

		if (componentProp.objectReferenceValue == null)
		{
			properties			= new List<Component>();
			propertyNames		= new List<String>();
			propertyNamesNice	= Array.Empty<String>();
		}
		else
		{
			var obj				= ((Component)componentProp.objectReferenceValue).gameObject;
			properties			= new List<Component>( );
			propertyNames		= new List<String>( );
			propertyNamesNice	= Array.Empty<String>();
			var propertyNamesNiceList	= new List<String>();

			foreach (var component in obj.GetComponents<MonoBehaviour>())
			{
				if (!component)
					continue;
					
				var seenTypes = new HashSet<Type>();
				var allowBindToAnyMemberKey = EditorPrefs.GetBool(AllowBindToAnyMemberKey, false);
				
				GetMethodsFromObj(seenTypes, 1, component, component.GetType(), "", bindType, !allowBindToAnyMemberKey, properties, propertyNames, propertyNamesNiceList);
				propertyNamesNice = propertyNamesNiceList.ToArray();
			}
		}
	}
	public static	void	GetMethodsFromObj	( HashSet<Type> seenTypes, int level, Component component, Type? objType, String propPrefix, Type bindType, Boolean onlyBindable, List<Component> properties, List<String> propertyNames, List<String> niceNames )												
	{
		if (level >= 3 || objType == null || objType == typeof(Object))
			return;
		
		var type = objType;
		do
		{
			if (!seenTypes.Add(type))
				break;

			foreach (var propertyInfo in type!.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ))
			{
				var typeProviderMemberName = default(String);
			
				var attr = propertyInfo.GetCustomAttribute<BindableAttribute>(true);

				if (attr != null)
					typeProviderMemberName	= attr.TypeProvider;

				else if (onlyBindable)
					continue;

				if (bindType.IsAssignableFrom( propertyInfo.PropertyType ))
				{
					properties.Add		( component );
					propertyNames.Add	( propPrefix + propertyInfo.Name );
					niceNames.Add		( ObjectNames.NicifyVariableName( component.GetType().Name + ":  " + propPrefix + propertyInfo.Name ) );
				}
				else if (propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.IsByRef)
				{
					var memberName	= typeProviderMemberName;
					var propType	= propertyInfo.PropertyType;
						
					if  (memberName != null)
					{
						var fieldInfo = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (fieldInfo != null)
						{
							var o = fieldInfo.GetValue(component);
							if (o is String typeStr) propType = Type.GetType(typeStr);
							else if (o is Type typeObj) propType = typeObj;
						}
							
						var propInfo = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (propInfo != null)
						{
							var o = propInfo.GetValue(component);
							if (o is String typeStr) propType = Type.GetType(typeStr);
							else if (o is Type typeObj) propType = typeObj;
						}
					}
						
					GetMethodsFromObj(seenTypes, level + 1, component, propType, propPrefix + propertyInfo.Name + ".", bindType, onlyBindable, properties, propertyNames, niceNames);
				}
			}
			type = type.BaseType;
		}
		while (type != null && type != typeof(Object));

		type = objType;
		do
		{
			if (!seenTypes.Add(type))
				break;
		
			foreach( var methodInfo in type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).Where( methodInfo => bindType.IsAssignableFrom( methodInfo.ReturnType ) && methodInfo.GetCustomAttributes( typeof(BindableAttribute), true ).Length > 0 ) )
			{
				properties.Add		( component );
				propertyNames.Add	( propPrefix + methodInfo.Name );

				var @params = methodInfo.GetParameters ();
				var optionString = $"{objType.Name} . {methodInfo.Name} ({(@params.Length == 0 ? "" : @params[0].Name)})";
				niceNames.Add( ObjectNames.NicifyVariableName( optionString ) );
			}
						
			type = type.BaseType;
		}
		while (type != null && type != typeof(Object));
	}
	public static	void	DrawProp			( SerializedProperty property, Type bindType, out Type memberType )	
	{
		var componentProp				= property.FindPropertyRelative( "Component" );
		var memberNameProp				= property.FindPropertyRelative( "MemberName" );
		var paramsProp					= property.FindPropertyRelative( "Params" );
		
		var errorString = String.Empty;
		
		GUILayout.BeginHorizontal();
		{
			memberType = typeof(void);
				
			GUILayout.Label( property.displayName );
			EditorGUILayout.PropertyField	( componentProp, GUIContent.none );

			if (componentProp.objectReferenceValue == null)
			{
				GUI.color						= Color.red;
				EditorGUILayout.LabelField		( "Choose Source First!!!" );
				GUI.color						= Color.white;
					
				GUILayout.EndHorizontal();
				return;
			}

			if (String.IsNullOrWhiteSpace(memberNameProp.stringValue))
			{
				GUI.color = Color.red;
				errorString = $"Property not selected !!!";
			}
			else 
			{
				var testName = memberNameProp.stringValue;
			
				if (testName.Contains("."))
					testName = testName.Split('.')[0];
			
				if (componentProp.objectReferenceValue.GetType().GetMember(testName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0)
				{
					GUI.color = Color.red;
					errorString = $"Source Has No Property named {memberNameProp.stringValue}";
				}
			}
			
			if (GUILayout.Button("."+memberNameProp.stringValue, new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft }, GUILayout.MinWidth(150)))
			{
				UpdateMethods( property.FindPropertyRelative( "Component" ), bindType, out var properties, out var propertyNames, out var propertyNamesNice );
							
				var menu = new GenericMenu();
				for (var i = 0; i < propertyNamesNice.Length; i++)
				{
					var idx = i;
					menu.AddItem(new GUIContent(propertyNamesNice[i]), /*idx == index*/false, () =>
					{
						Undo.RecordObjects(property.serializedObject.targetObjects, "Target Property Changed");
						memberNameProp.stringValue = propertyNames[idx];
						componentProp.objectReferenceValue = properties[idx];
						paramsProp.stringValue = "";
						property.serializedObject.ApplyModifiedProperties();
					});
				}
				menu.ShowAsContext();
			}
			GUI.color = Color.white;
		}
		GUILayout.EndHorizontal();

		var source		= componentProp.objectReferenceValue;
		var memberName	= memberNameProp.stringValue;

		if (source == null || String.IsNullOrWhiteSpace(memberName))
			return;

		var bindTargetType = source.GetType();
			
		while (bindTargetType != typeof(Object))
		{
			var method = bindTargetType.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault( methodInfo => methodInfo.Name == memberName && bindType.IsAssignableFrom( methodInfo.ReturnType ) );
			if (method != null)
			{
				var @params = method.GetParameters();
				if (@params.Length == 1)
				{
					var attr = method.GetCustomAttribute<BindableAttribute>( true );
					if (!String.IsNullOrWhiteSpace(attr.Description))
						EditorGUILayout.HelpBox( attr.Description, attr.IsWarning ? MessageType.Warning : MessageType.Info );

					var paramType = @params[0].ParameterType;
						
					try
					{
						
						if (paramType == typeof(String))
						{
							EditorGUI.BeginChangeCheck();
							var val  	= EditorGUILayout.TextField ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue );
								
							if (EditorGUI.EndChangeCheck())
								paramsProp.stringValue = val;
						}
							
						else if (paramType == typeof(Single))
						{
							EditorGUI.BeginChangeCheck();
							var val  	= EditorGUILayout.FloatField( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0.0f : Single.Parse( paramsProp.stringValue ) ).ToString(CultureInfo.InvariantCulture );
								
							if (EditorGUI.EndChangeCheck())
								paramsProp.stringValue = val;
						}
					
						else if (paramType == typeof(Boolean))
						{
							EditorGUI.BeginChangeCheck();
							var val  	= EditorGUILayout.Toggle( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue != "" && Boolean.Parse( paramsProp.stringValue ) ).ToString( );
								
							if (EditorGUI.EndChangeCheck())
								paramsProp.stringValue = val;
						}

						else if (paramType.IsEnum)
						{
							EditorGUI.BeginChangeCheck();
							var type	= paramType;
							var val  	= Convert.ToInt32( EditorGUILayout.EnumPopup( ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse( type, String.IsNullOrEmpty( paramsProp.stringValue ) ? Enum.GetNames(type)[0] : paramsProp.stringValue ) ) ).ToString( );
								
							if (EditorGUI.EndChangeCheck())
								paramsProp.stringValue = val;
						}

						else if (paramType == typeof(Int32))
						{
							EditorGUI.BeginChangeCheck();
							var val  	= EditorGUILayout.IntField  ( ObjectNames.NicifyVariableName( @params[0].Name ), paramsProp.stringValue == "" ? 0 : Int32.Parse( paramsProp.stringValue ) ).ToString( );
								
							if (EditorGUI.EndChangeCheck())
								paramsProp.stringValue = val;
						}
							
						else if (paramType == typeof(GameObject))
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
					catch (FormatException fe)
					{
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
			if (propertyInfo != null)
			{
				paramsProp.stringValue = "";
				memberType = propertyInfo.PropertyType;
				break;
			}

			bindTargetType = bindTargetType.BaseType;
		}
		
		if (!String.IsNullOrWhiteSpace(errorString))
		{
			EditorGUILayout.HelpBox(errorString, MessageType.Error);
		}
	}
}