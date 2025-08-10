using System.Reflection;

namespace Flexy.Core.Binding
{
	public abstract class Binder : MonoBehaviour
	{
		[BindTo(typeof(void))]
		[SerializeField]	private	BindSource _source;

		private Boolean _isInitialized;

		public					Component		Component		=> _source.Component;
		public					String			MemberName		=> _source.MemberName;

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
			var target2 = _source.Component as IBindersNotifier;
			if( target2 != null )
			{
				target2.AttachBinder( this );

				if( target2.ReadyForBind )
					SafeBind(  );

				return;
			}

			SafeBind (  );
		}
		protected	virtual		void			OnDisable		( )					
		{
			var target2 = _source.Component as IBindersNotifier;
			if( target2 != null )
				target2.DetachBinder( this );
		}
		protected	virtual		void			OnDestroy		( )					
		{
			var target2 = _source.Component as IBindersNotifier;
			if( target2 != null )
				target2.DetachBinder( this );
		}

		protected				void			Init<TArg>		( ref Action<TArg> action,	Boolean requereSetter = true )	
		{
			if( !_source.Component && (Application.isEditor || Debug.isDebugBuild) )
				Debug.LogError		( $"Binder {GetType( ).Name} on game object {transform.name} has not Source set", this  );

			Init( ref action, ref _source, requereSetter );
		}
		protected				void			Init<TResult>	( ref Func<TResult> func,	Boolean requireGetter = true )	
		{
			if( !_source.Component && (Application.isEditor || Debug.isDebugBuild) )
				Debug.LogError		( $"Binder {GetType( ).Name} on game object {transform.name} has not Source set", this  );

			Init	( ref func, ref _source, requireGetter );
		}
	
		protected				void			Init<TArg>		( ref Action<TArg> action,	ref BindSource bindSource, Boolean requireSetter = true )		
		{
			try
			{
				var type = bindSource.Component.GetType();

				do
				{
					var prop = type.GetProperty( bindSource.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (prop != null)
					{
						var propSetter = prop.GetSetMethod(true);

						if (propSetter != null)
						{
							action = (Action<TArg>)Delegate.CreateDelegate(typeof(Action<TArg>), bindSource.Component, propSetter);
							return;
						}
					}

					foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (method.Name != bindSource.MemberName || method.GetParameters(  ).Length < 2 || method.GetCustomAttributes(typeof(BindableAttribute), true).Length == 0)
							continue;

						action = BindSetterMethod<TArg>(bindSource.Component, method, bindSource.Params);

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
		protected				void			Init<TResult>	( ref Func<TResult> action,	ref BindSource bindSource, Boolean requireGetter = true )		
        {
            try
            {
                var type = bindSource.Component.GetType();

                do
                {
                    var prop = type.GetProperty( bindSource.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if ( prop != null )
                    {
                        if ( prop.GetCustomAttributes(typeof(BindableAttribute), true).Length > 0 )
                        {
                            var propGetter = prop.GetGetMethod(true);

							action = BindMethod<TResult>(bindSource.Component, propGetter, bindSource.Params);
                            return;
                        }
                    }

                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.Name != bindSource.MemberName || method.GetCustomAttributes(typeof(BindableAttribute), true).Length == 0)
                            continue;

                        action = BindMethod<TResult>(bindSource.Component, method, bindSource.Params);

                        if (action != null)
                            return;
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

            if ( requireGetter )
			{
	            try					{ Debug.LogError("[ABinder] - Init Fail: " + bindSource.Component.name + "->" + bindSource.Component.GetType().Name + "." + bindSource.MemberName + " has no getter", this); }
	            catch (Exception e) { if( Application.isEditor || Debug.isDebugBuild ) Debug.LogException( e ); }
			}
            else
            {
                Debug.Log("[ABinder] - Property " + bindSource.Component.name + "->" + bindSource.Component.GetType().Name + "." + bindSource.MemberName + " has no getter. Binder get logic will not work.", this);
			}
        }

		protected				void			ReportMissedTargetError		( Type targetType )	
		{
			Debug.Log( $"[{GetType().Name}] There is no target {targetType.Name}, binder path { GetHierarchyName( transform ) }, binder is disabled", this );
			enabled = false;
		}

		private					void			SafeBind					( )		
		{
			if( !enabled )
				return;

			try						{ Bind(  !_isInitialized ); }
			catch( Exception ex )
			{
				if( Application.isEditor || Debug.isDebugBuild )
					Debug.LogError	( $"[ABinder]-[SafeBind] Exception <b>{ex.GetType(  ).Name}</b> at GameObject <b>{GetHierarchyName( gameObject.transform )}</b>:\r\n{ex}", this );
			}
			_isInitialized = true;
		}
		private					void			RebindOnPropertyChanged		( )		
		{
			SafeBind	(  );
		}

		private					Func<TResult>	BindMethod<TResult>					( Object target, MethodInfo method, String parameters )	
		{
			var @params = method.GetParameters ( );
			switch( @params.Length )
			{
				case 0 when !method.ReturnType.IsEnum: return  (Func<TResult>)Delegate.CreateDelegate( typeof(Func<TResult>), target, method );

				case 0:
				{
					var enumType  = method.ReturnType;
					var intType   = Enum.GetUnderlyingType( enumType );
					var component = target;

					#if !BUG_FIXED
					{
						//temp workaround
						var dType		= typeof(Func<>).MakeGenericType( enumType );
						var d			= Delegate.CreateDelegate( dType, component, method );
					
						Int32 InternalInvoke ()
						{ 
							var obj			= d.DynamicInvoke( );
							var result		= Convert.ToInt32( obj );

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
				
					if( paramType == typeof(String) )
					{
						return CreateGetDelegate<String, TResult>( target, method, parameters );
					}
					if( paramType == typeof(Int32) )
					{
						var result = 0;
						if( Int32.TryParse ( parameters, out result ) )
							return CreateGetDelegate<Int32, TResult>( target, method, result );
					}
					if( paramType == typeof(Single) )
					{
						var result = 0.0f;
						if( Single.TryParse ( parameters, out result ) )
							return CreateGetDelegate<Single, TResult>( target, method, result );
					}
					if( paramType == typeof(Boolean) )
					{
						var result = false;
						if( Boolean.TryParse ( parameters, out result ) )
							return CreateGetDelegate<Boolean, TResult>( target, method, result );
					}
				
					if( paramType == typeof(BindableDataStore) )
					{
						return ( new OneParamBinder<BindableDataStore,TResult>{ Param = gameObject.GetComponent<BindableDataStore>(), Function = (Func<BindableDataStore, TResult>)Delegate.CreateDelegate( typeof(Func<BindableDataStore, TResult>), target, method ) } ).GetValue;
					}
					if( paramType == typeof(GameObject) )
					{
						return ( new OneParamBinder<GameObject,TResult>{ Param = gameObject, Function = (Func<GameObject, TResult>)Delegate.CreateDelegate( typeof(Func<GameObject, TResult>), target, method ) } ).GetValue;
					}
					
					if ( paramType.IsEnum )
					{
						var enumType	= paramType;
						var intType		= Enum.GetUnderlyingType( enumType );
					
						if( intType == typeof(Byte) )
							return CreateGetDelegate<Byte, TResult>( target, method, Byte.Parse( parameters ) );

						if( intType == typeof(Int16) )
							return CreateGetDelegate<Int16, TResult>( target, method, Int16.Parse( parameters ) );

						return CreateGetDelegate<Int32, TResult>( target, method, Int32.Parse( parameters ) );
					}

					break;
				}
			}

			return null;
		}
		private					Func<TResult>	CreateGetDelegate<TParam,TResult>	( Object target, MethodInfo method, TParam param )		
		{
			if ( !method.ReturnType.IsEnum )
				return ( new OneParamBinder<TParam,TResult>{ Param = param, Function = (Func<TParam, TResult>)Delegate.CreateDelegate( typeof(Func<TParam, TResult>), target, method ) } ).GetValue;
		
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
						var @params		= new object[]{param};
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
			var @params = method.GetParameters ( );

			if( @params.Length == 2 )
			{
				var paramType = @params[0].ParameterType;
				
				if( paramType == typeof(String) )
				{
					return ( new OneParamSetterBinder<String, TArg>{ Action = (Action<String, TArg>)Delegate.CreateDelegate( typeof(Action<String, TArg>), target, method ) } ).SetValue;
				}
				if( paramType == typeof(Int32) )
				{
					var result = 0;
					if( Int32.TryParse ( parameters, out result ) )
						return ( new OneParamSetterBinder<Int32,TArg>{ Param = result, Action = (Action<Int32, TArg>)Delegate.CreateDelegate( typeof(Action<Int32, TArg>), target, method ) } ).SetValue;
				}
				if( paramType == typeof(Single) )
				{
					var result = 0.0f;
					if( Single.TryParse ( parameters, out result ) )
						return ( new OneParamSetterBinder<Single,TArg>{ Param = result, Action = (Action<Single, TArg>)Delegate.CreateDelegate( typeof(Action<Single, TArg>), target, method ) } ).SetValue;
				}
				if( paramType == typeof(Boolean) )
				{
					var result = false;
					if( Boolean.TryParse ( parameters, out result ) )
						return ( new OneParamSetterBinder<Boolean,TArg>{ Param = result, Action = (Action<Boolean, TArg>)Delegate.CreateDelegate( typeof(Action<Boolean, TArg>), target, method ) } ).SetValue;
				}
				if( paramType == typeof(BindableDataStore) )
				{
					return ( new OneParamSetterBinder<BindableDataStore,TArg>{ Param = gameObject.GetComponent<BindableDataStore>(), Action = (Action<BindableDataStore, TArg>)Delegate.CreateDelegate( typeof(Action<BindableDataStore, TArg>), target, method ) } ).SetValue;
				}
				if( paramType == typeof(GameObject) )
				{
					return ( new OneParamSetterBinder<GameObject,TArg>{ Param = gameObject, Action = (Action<GameObject, TArg>)Delegate.CreateDelegate( typeof(Action<GameObject, TArg>), target, method ) } ).SetValue;
				}
			}

			return null;
		}		

		private static			String			GetHierarchyName	( Transform t, Int32 steps = 99 )	
		{
			if ( t == null ) throw new ArgumentNullException( nameof( t ) );
			
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
		
		internal class		ZeroParamBinderWithConvert<TResult, TResult2>		
		{
			public		Func<TResult>			Function;
			public		Func<TResult, TResult2>	Converter;
			
			public		TResult2	GetValue	( )
			{
				return Converter(Function( ));
			}
		}
		
		public class		OneParamBinder<TArg,TResult>		
		{
			public		TArg					Param;
			public		Func<TArg, TResult>		Function;
			public		TResult		GetValue	( )
			{
				return Function( Param );
			}
		}
		
		internal class		OneParamBinderWithConvert<TArg,TResult, TResult2>		
		{
			public		TArg					Param;
			public		Func<TArg, TResult>		Function;
			public		Func<TResult, TResult2>	Converter;
			
			public		TResult2	GetValue	( )
			{
				return Converter(Function( Param ));
			}
		}
		
		private class		OneParamSetterBinder<TArg, TInputArg>		
		{
			public		TArg							Param;
			public		Action<TArg, TInputArg>			Action;

			public		void		SetValue	( TInputArg param )
			{
				Action( Param, param );
			}
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