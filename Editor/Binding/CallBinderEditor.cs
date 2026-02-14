namespace Flexy.Core.Editor.Binding
{
	[CustomEditor(typeof(CallBinder), true), CanEditMultipleObjects]
	public class CallBinderEditor : UnityEditor.Editor 
	{
		private				List<Object>		_methodsComponents	= null!;
		private				List<MethodInfo>	_methods			= null!;
		private				List<String>		_methodNames		= null!;
		private				String[]			_methodNamesNice	= null!;

		public override		VisualElement		CreateInspectorGUI	( )		
		{
			var root = new VisualElement();
			var iterator = serializedObject.GetIterator();
			var enterChildren = true;

			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;

				if (iterator.propertyPath is "m_Script" or "_target" or "_methodName" or "_context")
					continue;

				var propertyField = new PropertyField(iterator);
				propertyField.BindProperty(iterator);
				root.Add(propertyField);
			}

			root.Add(new IMGUIContainer(OnInspectorGUI));

			return root;
		}

		public override		void				OnInspectorGUI		( )		
		{
			serializedObject.Update			();

			var componentProp				= serializedObject.FindProperty( "_target" );
			var methodProp					= serializedObject.FindProperty( "_methodName" );
			var context						= serializedObject.FindProperty( "_context" );

			EditorGUI.BeginChangeCheck		();
			EditorGUILayout.PropertyField	(componentProp);

			var targetChanged				= false;

			if (EditorGUI.EndChangeCheck())
			{
				UpdateMethods	();
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
				var index		= _methodNames.IndexOf(methodProp.stringValue);

				if (index == -1)
				{
					index								= 0;
					methodProp.stringValue				= _methodNames[index];
					componentProp.objectReferenceValue	= _methodsComponents[index];
				}
				else if (targetChanged)
				{
					componentProp.objectReferenceValue	= _methodsComponents[index];
				}

				EditorGUI.BeginChangeCheck		();
				index							= EditorGUILayout.Popup("Method", index, _methodNamesNice);
				var methodChanged				= EditorGUI.EndChangeCheck();

				if( methodChanged )
				{
					Undo.RecordObjects						( targets, "Target Method Chaged" );
					methodProp.stringValue					= _methodNames[index];
					componentProp.objectReferenceValue		= _methodsComponents[index];
					context.stringValue = String.Empty;
				}

				var @params = _methods[index].GetParameters();
				if (@params.Length != 0)
				{
					if (@params.Length == 1)
					{
						if (@params[0].ParameterType == typeof(String))
						{
							context.stringValue		= EditorGUILayout.TextField ( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue );
						}
						else if (@params[0].ParameterType == typeof(Int32))
						{
							context.stringValue		= EditorGUILayout.IntField  ( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue == "" ? 0 : Int32.Parse( context.stringValue ) ).ToString( );
						}
						else if (@params[0].ParameterType == typeof(Single))
						{
							context.stringValue		= EditorGUILayout.FloatField( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue == "" ? 0.0f : Single.Parse( context.stringValue ) ).ToString( CultureInfo.CurrentCulture );
						}
						else if (@params[0].ParameterType == typeof(Boolean))
						{
							context.stringValue		= EditorGUILayout.Toggle( ObjectNames.NicifyVariableName( @params[0].Name ), context.stringValue != "" && Boolean.Parse( context.stringValue ) ).ToString( );
						}   
						else if (@params[0].ParameterType.IsEnum)
						{
							var type = @params[0].ParameterType;
							if (type.IsDefined(typeof(FlagsAttribute)))
							{
								context.stringValue		= Convert.ToInt32( EditorGUILayout.EnumFlagsField( ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse( type, String.IsNullOrEmpty( context.stringValue ) ? Enum.GetNames(type)[0] : context.stringValue ) ) ).ToString( );
							}
							else
							{
								context.stringValue		= Convert.ToInt32( EditorGUILayout.EnumPopup( ObjectNames.NicifyVariableName( type.Name ), (Enum)Enum.Parse( type, String.IsNullOrEmpty( context.stringValue ) ? Enum.GetNames(type)[0] : context.stringValue ) ) ).ToString( );
							}
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

			serializedObject.ApplyModifiedProperties();
		}
		private				void				OnEnable			( )		
		{
			if (target == null)
			{
				DestroyImmediate(this);
				return;
			}
			UpdateMethods	();
		}

		private				void				UpdateMethods		( )		
		{
			var componentProp			= serializedObject.FindProperty("_target");
			var onlyCallable			= !EditorPrefs.GetBool(BindSourceDrawer.AllowBindToAnyMemberKey, false);
			var onlyPublic				= !EditorPrefs.GetBool(BindSourceDrawer.AllowBindToNonPublicKey, false);

			if (componentProp.objectReferenceValue == null)
			{
				_methodsComponents	= new List<Object>	();
				_methodNames		= new List<String>		();
				_methods			= new List<MethodInfo>	();
				_methodNamesNice	= new String[0];
			}
			else
			{
				var obj				= ((Component)componentProp.objectReferenceValue).gameObject;
				_methodsComponents	= new List<Object>();
				_methodNames		= new List<String>();
				_methods			= new List<MethodInfo>();
				
				var allTargets = obj.GetComponents<Component>().Cast<Object>().Prepend(obj).ToList();

				foreach (var component in allTargets)
				{
					if (component == null)
						continue;
					
					var type = component.GetType();
					do
					{
						foreach (var method in type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ))
						{
							var isObsolete = method.GetCustomAttribute<ObsoleteAttribute>(true) != null;
						
							if (isObsolete)
								continue;
							
							var hasAttr = method.GetCustomAttribute<CallableAttribute>(true) != null;
							
							if (onlyCallable && !hasAttr)
								continue;  
								
							if (onlyPublic && !method.IsPublic && !hasAttr)
								continue;
						
							if (method.ReturnType != typeof(void) && method.ReturnType != typeof(UniTaskVoid))
								continue;
								
							var parameters = method.GetParameters();
							
							if (parameters.Length > 1)
								continue;
								
							if (parameters.Length == 1)
							{
								var ptype = parameters[0].ParameterType;
								if (ptype != typeof(String) && ptype != typeof(Int32) && ptype != typeof(Boolean) && ptype != typeof(Single) && !ptype.IsEnum && ptype != typeof(GameObject))
									continue;
							}
							
							_methodsComponents	.Add( component );
							_methodNames		.Add( method.Name );
							_methods			.Add( method );
							
						}
						
						type = type.BaseType;
					}
					while (type != typeof(Object));
				}

				var componentsCount = _methodsComponents.Distinct().Count();
				_methodNamesNice = new String[_methodsComponents.Count];
				for (var i = 0; i < _methodsComponents.Count; i++)
				{
					if (componentsCount > 1)
						_methodNamesNice[i] = ObjectNames.NicifyVariableName( _methodsComponents[i].GetType().Name.Replace("_", "  ") + " / " + _methods[i].Name.Replace('_', ' ') );
					else
						_methodNamesNice[i] = ObjectNames.NicifyVariableName( _methodsComponents[i].GetType().Name.Replace("_", "  ") + " - " + _methods[i].Name.Replace('_', ' ') );	
				}
			}
		}
	}
}