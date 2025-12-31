using System.Globalization;
using System.Reflection;

namespace Flexy.Core.Binding
{
	[HelpURL("https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/ScriptingApi/Binder.md")]

	public abstract class Binder : MonoBehaviour
	{
		[BindTo(typeof(void))]
		[SerializeField]	BindSource	_source;

		private Boolean		_isInitialized;

		internal	Component	Component	=> _source.Component;
		internal	String		MemberName	=> _source.MemberName;

		[ContextMenu ("Rebind")]
		public                  void			Rebind			( )					
        {
            SafeBind();
        }

		public override			String			ToString		( )					
		{
			if( Component == null )
				return name + "->" + GetType( ).Name;

			return name + "->" + GetType( ).Name + " On " + Component.name + "->" + Component.GetType( ).Name + "." + _source.MemberName;
		}

		protected abstract		void			Bind			( Boolean init );

		protected	virtual		void			OnEnable		( )					
		{
			if (_source.Component is IBindersNotifier notifier)
			{
				notifier.AttachBinder(this);

				if (notifier.ReadyForBind)
					SafeBind();

				return;
			}

			SafeBind();
		}
		protected	virtual		void			OnDisable		( )					
		{
			if (_source.Component is IBindersNotifier target)
				target.DetachBinder(this);
		}
		protected	virtual		void			OnDestroy		( )					
		{
			if (_source.Component is IBindersNotifier target)
				target.DetachBinder(this);
		}

		protected				void			Init<TArg>		( ref Action<TArg> action,	Boolean requereSetter = true )	
		{
			Init	( ref action, ref _source, requereSetter );
		}
		protected				void			Init<TResult>	( ref Func<TResult> func,	Boolean requireGetter = true )	
		{
			Init	( ref func, ref _source, requireGetter );
		}
	
		protected				void			Init<TArg>		( ref Action<TArg> action,	ref BindSource bindSource, Boolean requireSetter = true )		
		{
			if (!bindSource.Component)
			{
				if (Application.isEditor || Debug.isDebugBuild)
					Debug.LogError	($"Binder {GetType( ).Name} on game object {transform.name} has not Source set", this);
					
				return;
			}
		
			try
			{
				Object? objToBindTo	= bindSource.Component;
				var type			= objToBindTo.GetType();
				var memberName		= bindSource.MemberName; 
                
				if (memberName.Contains('.'))
				{
					var names	= memberName.Split('.', StringSplitOptions.RemoveEmptyEntries);
					memberName	= names[0];
	                
					for (var i = 0; i < names.Length-1; i++)
					{
						var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				        
						objToBindTo = prop.GetValue(objToBindTo);
						type		= objToBindTo.GetType();
						memberName	= names[i+1];
					}
				}

				do
				{
					var prop = type.GetProperty( memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (prop != null)
					{
						var propSetter = prop.GetSetMethod(true);

						if (propSetter != null)
						{
							action = (Action<TArg>)Delegate.CreateDelegate(typeof(Action<TArg>), objToBindTo, propSetter);
							return;
						}
					}

					foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (method.Name != memberName || method.GetParameters().Length < 2)
							continue;

						action = BindSetterMethod<TArg>(objToBindTo, method, bindSource.Params);
					}

					type = type.BaseType;
				}
				while (type != typeof(Object));
			}
			catch (Exception ex)
			{
				if( Application.isEditor || Debug.isDebugBuild )
					Debug.LogException(ex, this);
			}

			if (requireSetter)
				Debug.LogError("[ABinder] - Init Fail: " + bindSource.Component.name + "->" + bindSource.Component.GetType().Name + "." + bindSource.MemberName + " has no setter", this);

			//else
			//    Debug.Log("[ABinder] - Property " + bindSource.Target.name + "->" + bindSource.Target.GetType().Name + "." + bindSource.MemberName + " has no setter. Binder set logic will not work.", this);
		}
		protected				void			Init<TResult>	( ref Func<TResult> func,	ref BindSource bindSource, Boolean requireGetter = true )		
        {
	        if (!bindSource.Component)
	        {
		        if (Application.isEditor || Debug.isDebugBuild)
			        Debug.LogError	($"Binder {GetType().Name} on game object {transform.name} has not Source set", this);
					
		        return;
	        }
        
            try
            {
	            Object? objToBindTo	= bindSource.Component;
                var type			= objToBindTo.GetType();
                var memberName		= bindSource.MemberName; 
                
                if (memberName.Contains('.'))
                {
	                var names	= memberName.Split('.', StringSplitOptions.RemoveEmptyEntries);
	                memberName	= names[0];
	                
	                for (var i = 0; i < names.Length-1; i++)
	                {
	                    var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				        
				        objToBindTo = prop.GetValue(objToBindTo);
				        type		= objToBindTo.GetType();
				        memberName	= names[i+1];
		            }
                }
                
                do
                {
                    var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (prop != null)
                    {
                        var propGetter = prop.GetGetMethod(true);

						func = BindGetterMethod<TResult>(objToBindTo, propGetter, bindSource.Params);
                        return;
                    }

                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.Name != memberName)
                            continue;

                        func = BindGetterMethod<TResult>(objToBindTo, method, bindSource.Params);

                        if (func != null)
                            return;
                    }

                    type = type.BaseType;
                }
                while (type != typeof(Object));
            }
            catch (Exception ex)
            {
				if (Application.isEditor || Debug.isDebugBuild) 
					Debug.LogException(ex, this);
            }

            if (requireGetter)
			{
	            try					{ Debug.LogError("[ABinder] - Init Fail: " + bindSource.Component.name + "->" + bindSource.Component.GetType().Name + "." + bindSource.MemberName + " has no getter", this); }
	            catch (Exception e) { if (Application.isEditor || Debug.isDebugBuild) Debug.LogException(e); }
			}
            else
            {
                Debug.Log("[ABinder] - Property " + bindSource.Component.name + "->" + bindSource.Component.GetType().Name + "." + bindSource.MemberName + " has no getter. Binder get logic will not work.", this);
			}
        }

		protected				void			ReportMissedTargetError		( Type targetType )	
		{
			Debug.LogWarning( $"[{GetType().Name}] There is no target {targetType.Name}, binder path { GetHierarchyName(transform) }, binder is disabled", this );
			enabled = false;
		}

		private					void			SafeBind					( )		
		{
			if (!enabled)
				return;

			try						{ Bind(!_isInitialized); }
			catch( Exception ex )
			{
				if( Application.isEditor || Debug.isDebugBuild )
					Debug.LogError	( $"[ABinder]-[SafeBind] Exception <b>{ex.GetType().Name}</b> at GameObject <b>{GetHierarchyName(gameObject.transform)}</b>:\r\n{ex}", this );
			}
			_isInitialized = true;
		}
		private					void			RebindOnPropertyChanged		( )		
		{
			SafeBind	();
		}

		private					Func<TResult>	BindGetterMethod<TResult>			( Object target, MethodInfo method, String parameters )	
		{
			var @params = method.GetParameters();
			switch( @params.Length )
			{
				case 0 when !method.ReturnType.IsEnum: return  (Func<TResult>)Delegate.CreateDelegate( typeof(Func<TResult>), target, method );

				case 0:
				{
					var enumType  = method.ReturnType;
					var intType   = Enum.GetUnderlyingType(enumType);
					var component = target;

					#if !BUG_FIXED
					{
						//temp workaround
						var dType		= typeof(Func<>).MakeGenericType(enumType);
						var d			= Delegate.CreateDelegate( dType, component, method );
					
						Int32 InternalInvoke ()
						{ 
							var obj			= d.DynamicInvoke();
							var result		= Convert.ToInt32(obj);

							return result;
						}
					
						return  (Func<TResult>) (Delegate) (Func<Int32>) InternalInvoke;
					}
					#else
					{
						if( intType == typeof(Byte) )
							return (Func<TResult>) (Delegate) (Func<Int32>) new ZeroParamBinderWithConvert<Byte, Int32>{ Function = (Func<Byte>)Delegate.CreateDelegate(typeof(Func<Byte>), component, method ), Converter = b => b }.GetValue;

						if( intType == typeof(Int16) )
							return (Func<TResult>) (Delegate) (Func<Int32>) new ZeroParamBinderWithConvert<Int16, Int32>{ Function = (Func<Int16>)Delegate.CreateDelegate(typeof(Func<Int16>), component, method ), Converter = b => b }.GetValue;
									
						if( intType == typeof(Int32) )
							return (Func<TResult>) (Delegate) (Func<Int32>) Delegate.CreateDelegate(typeof(Func<Int32>), component, method );

						return  (Func<TResult>)Delegate.CreateDelegate( typeof(Func<TResult>), target, method );
					}
					#endif
				}

				case 1:
				{
					var paramType = @params[0].ParameterType;
				
					if (paramType == typeof(String))
					{
						return CreateGetDelegate<String, TResult>(target, method, parameters);
					}
					
					if (paramType == typeof(Int32))
					{
						var result = 0;
						if (String.IsNullOrWhiteSpace(parameters) || Int32.TryParse(parameters, out result))
							return CreateGetDelegate<Int32, TResult>(target, method, result);
					}
					
					if (paramType == typeof(Single))
					{
						var result = 0.0f;
						if (String.IsNullOrWhiteSpace(parameters) || Single.TryParse(parameters,  NumberStyles.Any, CultureInfo.InvariantCulture, out result))
							return CreateGetDelegate<Single, TResult>(target, method, result);
					}
					
					if (paramType == typeof(Boolean))
					{
						var result = false;
						if (String.IsNullOrWhiteSpace(parameters) || Boolean.TryParse(parameters, out result))
							return CreateGetDelegate<Boolean, TResult>(target, method, result);
					}
				
					if (paramType == typeof(GameObject))
					{
						return new OneParamBind<GameObject,TResult>( gameObject, (Func<GameObject, TResult>)Delegate.CreateDelegate( typeof(Func<GameObject, TResult>), target, method ) ).GetValue;
					}
					
					if (paramType.IsEnum)
					{
						var enumType	= paramType;
						var intType		= Enum.GetUnderlyingType( enumType );
					
						Int32.TryParse(parameters, out var result);
					
						if (intType == typeof(Byte))
							return CreateGetDelegate<Byte, TResult>( target, method, (Byte)result );

						if (intType == typeof(Int16))
							return CreateGetDelegate<Int16, TResult>( target, method, (Int16)result );

						return CreateGetDelegate<Int32, TResult>( target, method, result );
					}

					break;
				}
			}

			return null!;
		}
		private					Func<TResult>	CreateGetDelegate<TParam,TResult>	( Object target, MethodInfo method, TParam param )		
		{
			if ( !method.ReturnType.IsEnum )
				return new OneParamBind<TParam,TResult>( param, (Func<TParam, TResult>)Delegate.CreateDelegate( typeof(Func<TParam, TResult>), target, method ) ).GetValue;
		
			var enumType  = method.ReturnType;
			var intType   = Enum.GetUnderlyingType( enumType );
			var component = target;
		
			#if !BUG_FIXED
			{
				//temp workaround
				{
					var dType		= typeof(Func<,>).MakeGenericType( typeof(TParam), enumType );
					var d			= Delegate.CreateDelegate( dType, component, method );
				
					Int32 InternalInvoke ()
					{ 
						var @params		= new object?[]{param};
						var obj			= d.DynamicInvoke( @params );
						var result		= Convert.ToInt32( obj );

						return result;
					}
				
					return  (Func<TResult>) (Delegate) (Func<Int32>) InternalInvoke;
				}
			}
			#else
			{
				if( intType == typeof(Byte) )
				{
					var d = (Func<TParam,Byte>)Delegate.CreateDelegate(typeof(Func<TParam,Byte>), component, method );
					
					return (Func<TResult>) (Delegate) (Func<Int32>) new OneParamBinderWithConvert<TParam,Byte,Int32>{ Param = param, Function = d, Converter = b => b }.GetValue;
				}

				if( intType == typeof(Int16) )
				{
					var d = (Func<TParam,Int16>)Delegate.CreateDelegate(typeof(Func<TParam,Int16>), component, method );
					
					return (Func<TResult>) (Delegate) (Func<Int32>) new OneParamBinderWithConvert<TParam,Int16,Int32>{ Param = param, Function = d, Converter = b => b }.GetValue;
				}
								
				if( intType == typeof(Int32) )
				{
					var d = (Func<TParam,Int32>)Delegate.CreateDelegate(typeof(Func<TParam,Int32>), component, method );
					
					return (Func<TResult>) (Delegate) (Func<Int32>) new OneParamBinder<TParam,Int32>{ Param = param, Function = d }.GetValue;
				}

				return  (Func<TResult>)Delegate.CreateDelegate( typeof(Func<TResult>), target, method );
			}
			#endif
		}
		private					Action<TArg>	BindSetterMethod<TArg>				( Object target, MethodInfo method, String parameters )	
		{
			var @params = method.GetParameters();

			if (@params.Length == 2)
			{
				var paramType = @params[0].ParameterType;
				
				if (paramType == typeof(String))
				{
					return new OneParamSetterBind<String, TArg>( parameters, (Action<String, TArg>)Delegate.CreateDelegate( typeof(Action<String, TArg>), target, method ) ).SetValue;
				}
				if (paramType == typeof(Int32))
				{
					var result = 0;
					if( Int32.TryParse ( parameters, out result ) )
						return new OneParamSetterBind<Int32,TArg>( result, (Action<Int32, TArg>)Delegate.CreateDelegate( typeof(Action<Int32, TArg>), target, method ) ).SetValue;
				}
				if (paramType == typeof(Single))
				{
					var result = 0.0f;
					if( Single.TryParse ( parameters, out result ) )
						return new OneParamSetterBind<Single,TArg>( result, (Action<Single, TArg>)Delegate.CreateDelegate( typeof(Action<Single, TArg>), target, method ) ).SetValue;
				}
				if (paramType == typeof(Boolean))
				{
					var result = false;
					if( Boolean.TryParse ( parameters, out result ) )
						return new OneParamSetterBind<Boolean,TArg>( result, (Action<Boolean, TArg>)Delegate.CreateDelegate( typeof(Action<Boolean, TArg>), target, method ) ).SetValue;
				}
				
				if (paramType == typeof(GameObject))
				{
					return new OneParamSetterBind<GameObject,TArg>( gameObject, (Action<GameObject, TArg>)Delegate.CreateDelegate( typeof(Action<GameObject, TArg>), target, method ) ).SetValue;
				}
			}

			return null!;
		}		

		private static			String			GetHierarchyName	( Transform t, Int32 steps = 99 )	
		{
			if ( t == null ) 
				throw new ArgumentNullException( nameof( t ) );
			
			var result = GetTransformHierarchyNameRecursive( t.parent, $"{t.name}", ref steps );
			return result;

			String GetTransformHierarchyNameRecursive( Transform tr, String res, ref Int32 steps2 )
			{
				if ( tr == null || --steps2 == 0)
					return res;

				res = GetTransformHierarchyNameRecursive( tr.parent, $"{tr.name}/{res}", ref steps2 );
				return res;
			}
		}
		
		[Serializable]
		public struct BindSource
		{
			public	Component	Component;
			public	String		MemberName;
			public	String		Params;
		}
		
		internal record		ZeroParamBindWithConvert<TResult, TResult2>( Func<TResult> Func, Func<TResult, TResult2> Convert )		
		{
			public		TResult2	GetValue	( ) => Convert(Func());
		}
		
		internal record		OneParamBind<TArg,TResult>( TArg Param, Func<TArg, TResult> Func )		
		{
			public		TResult		GetValue	( ) => Func( Param );
		}
		
		internal record		OneParamBindWithConvert<TArg,TResult, TResult2>( TArg Param, Func<TArg, TResult> Func, Func<TResult, TResult2> Convert )		
		{
			public		TResult2	GetValue	( ) => Convert(Func( Param ));
		}
		
		internal record		OneParamSetterBind<TArg, TInputArg>( TArg Param, Action<TArg, TInputArg>? Action )		
		{
			public		void		SetValue	( TInputArg param ) => Action?.Invoke( Param, param );
		}		
		
		public static class Internal
		{
			public static void RebindOn ( Binder binder )
			{
				binder.RebindOnPropertyChanged( );
			}
		}
	}
}