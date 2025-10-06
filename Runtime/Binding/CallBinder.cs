using System.Reflection;

namespace Flexy.Core.Binding
{
	public abstract class CallBinder : MonoBehaviour
	{
		[SerializeField]	private	Component	_target		= null!;
		[SerializeField]	private	String		_methodName	= null!;
		[SerializeField]	private	String		_context	= null!;
		
		protected void Init( ref Action? action )
		{
			GetMethodDelegate(this, _target, _methodName, ref action, _context );

			if( action == null )
				Debug.LogWarning		( $"[CallBinder] - Bind Init Fail: source object {name}, target {(_target ? _target.name : "null")} -> {_methodName}", this );
		}
		
		private static	void	GetMethodDelegate	( MonoBehaviour @this, Component target, String methodName, ref Action? action, String? props = null )
		{
			var methods	= target.GetType( ).GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

			foreach( var method in methods )
			{
				if( method.Name != methodName )
					continue;

				if( method.GetCustomAttributes ( typeof(CallableAttribute), true ).Length == 0 )
					continue;

				action = BindMethod( @this, target, method, props );
				return;
			}
		}
		private static	Action?	BindMethod			( MonoBehaviour @this, Object target, MethodInfo method, String? parameters )	
		{
			var @params			= method.GetParameters ( );
			var isUniTaskVoid	= method.ReturnParameter?.ParameterType  == typeof(UniTaskVoid);

			if ( @params.Length == 0 )
			{
				return isUniTaskVoid 
					? new ZeroParamBinderVoid( (Func<UniTaskVoid>)Delegate.CreateDelegate( typeof(Func<UniTaskVoid>), target, method ) ).SetValue 
					: (Action)Delegate.CreateDelegate( typeof(Action), target, method );
			}

			if( @params.Length == 1 )
			{
				if( @params[0].ParameterType == typeof(String) )
					return FinalBind( isUniTaskVoid, parameters, target, method );

				if( @params[0].ParameterType == typeof(Int32) )
				{
					var result = 0;
					if( Int32.TryParse ( parameters, out result ) )
						return FinalBind( isUniTaskVoid, result, target, method );
				}
				
				if( @params[0].ParameterType == typeof(Single) )
				{
					var result = 0.0f;
					if( Single.TryParse ( parameters, out result ) )
						return FinalBind( isUniTaskVoid, result, target, method );
				}
				
				if( @params[0].ParameterType == typeof(Boolean) )
				{
					var result = false;
					if( Boolean.TryParse ( parameters, out result ) )
						return FinalBind( isUniTaskVoid, result, target, method );
				}
				
				if ( @params[0].ParameterType.IsEnum )
				{
					var enumType	= @params[0].ParameterType;
					var intType		= Enum.GetUnderlyingType( enumType );

					if( intType == typeof(Byte) )
						return FinalBind( isUniTaskVoid, Byte.Parse( parameters ), target, method );

					if( intType == typeof(Int16) )
						return FinalBind( isUniTaskVoid, Int16.Parse( parameters ), target, method );

					if( intType == typeof(Int32) )
						return FinalBind( isUniTaskVoid, Int32.Parse( parameters ), target, method );
				}
				
				if( @params[0].ParameterType == typeof(GameObject) )
					return FinalBind( isUniTaskVoid, @this.gameObject, target, method );
			}

			static Action FinalBind<T>( Boolean isUniTaskVoid, T val, Object target, MethodInfo method )
			{
				return isUniTaskVoid 
					? new OneParamBinderVoid<T>		( val, (Func<T, UniTaskVoid>)	Delegate.CreateDelegate( typeof(Func<T, UniTaskVoid>),	target, method ) ).SetValue
					: (Action) new OneParamBinder<T>( val,  (Action<T>)				Delegate.CreateDelegate( typeof(Action<T>),				target, method ) ).SetValue;
			}

			return null;
		}
	
		private record		OneParamBinder<TParam>( TParam Param, Action<TParam> Function )		
		{
			public	void		SetValue	( ) => Function( Param );
		}

		private record		ZeroParamBinderVoid( Func<UniTaskVoid> Function )
		{
			public	void		SetValue	( ) => Function( );
		}

		private record		OneParamBinderVoid<TParam>( TParam Param, Func<TParam, UniTaskVoid> Function )		
		{
			public	void		SetValue	( ) => Function( Param ).Forget( );
		}
	}
}