namespace Flexy.Core.Actions
{
	[Serializable]
	public struct FlexyEvent
	{
		[SerializeReference] FlexyAction	_action;
		
		public UniTask Raise( Component ctxObj ) => _action.Raise( ctxObj );
		
		public event Action<ActionCtx> Raised
		{
			add
			{
				if( _action is not FlexyActionCodeCallbacks c )
				{
					c = new( );
					c.SetNext( _action );
					_action = c;
				}
				
				c.Raised += value;
			}
			remove
			{
				if( _action is FlexyActionCodeCallbacks c )
					c.Raised -= value;
			}
		}
	}
	
	[Serializable]
	public class FlexyActionCodeCallbacks : FlexyActionSync
	{
		[SerializeReference] FlexyAction?	_next;
		
		private List<Action<ActionCtx>>?	_callbacks;
		
		public event Action<ActionCtx> Raised
		{
			add		=> ( _callbacks ??= new( ) ).Add( value );
			remove	=> _callbacks?.Remove( value );
		}
		
		public void SetNext( FlexyAction next ) => _next = next;
		
		public override void	Do		( ActionCtx ctx )
		{
			if( _callbacks is { Count: > 0 } )
			{
				using var tmpList = TempList<Action<ActionCtx>>.Rent( _callbacks );
				
				foreach ( var action in tmpList )
				{
					try						{ action.Invoke( ctx );		}
					catch ( Exception ex )	{ Debug.LogException( ex );	}
				}
			}
			
			_next?.Do( ctx );
		}
	}
	
	public static class FlexyActionExtensions
	{
		public static		UniTask	Raise			( this FlexyAction action, Component ctxObj )	
		{
			if( action == null )
				return UniTask.CompletedTask;
			
			return action.GuardedDoAsync( ActionCtx.Rent( ctxObj ) );
		}
		public static async	UniTask	GuardedDoAsync	( this FlexyAction action, ActionCtx ctx )		
		{
			try
			{
				ctx.IncreaseRef( );
				await action.DoAsync( ctx );
			}
			finally
			{
				ctx.DecreaseRef( );
			}
		}
	}
	
	[Serializable]
	public abstract class FlexyAction
	{
		public abstract void Do(ActionCtx ctx);
		public abstract UniTask	DoAsync	( ActionCtx ctx );
		public abstract void Sample(ActionCtx ctx, Single time01);
	}
	
	[Serializable]
	public abstract class FlexyActionSync : FlexyAction
	{
		public sealed override void Sample(ActionCtx ctx, Single time01)		{ if( time01 == 0 ) Do( ctx ); if( time01 == 1 ) DoBack( ctx ); }
		public abstract override void Do(ActionCtx ctx);
		public virtual void DoBack(ActionCtx ctx)		{ }
		public sealed override		UniTask		DoAsync	( ActionCtx ctx )			{ Do( ctx ); return default; }
	}
	
	[Serializable]
	public abstract class FlexyActionAsync : FlexyAction
	{
		public sealed override void Sample(ActionCtx ctx, Single time01)		{ if( time01 == 0 ) Do( ctx ); }
		public abstract override	UniTask		DoAsync	( ActionCtx ctx );
		public sealed override void Do(ActionCtx ctx) => this.GuardedDoAsync( ctx ).Forget( Debug.LogException );
	}
	
	[Serializable]
	public abstract class FlexyActionSample : FlexyAction
	{
		public LerpAndTimeData LerpAndTime;	
		
		public abstract override void Sample(ActionCtx ctx, Single time01);
		public sealed override void Do(ActionCtx ctx) => this.GuardedDoAsync( ctx ).Forget( Debug.LogException );
		
		public sealed async override UniTask	DoAsync	( ActionCtx ctx )		
		{
			var timeEnd = Time.time + LerpAndTime.Length;

			Sample( ctx, 0 );
			while( Time.time < timeEnd )
			{
				await UniTask.DelayFrame( 1 );
				Sample( ctx, 1 - (Time.time - timeEnd) / LerpAndTime.Length );
			}
			Sample( ctx, 1 );
		}
		
		[Serializable]
		public struct LerpAndTimeData
		{
			public	ELerp	Begin;
			public	Single	Length;
			public	ELerp	End;
		}
		public enum ELerp
		{
			Clamped,
			Unclamped,
			Repeat,
			PingPong
		}
	}
		
	public interface IActionEntrance		{ public void Do	( );		}
	public interface IActionEntrance<T>		{ public void Do	( T data );	}
	
	public interface IActionExit			{ public IActionEntrance	Next{get;}	}
	public interface IActionExit<T>			{ public IActionEntrance<T>	Next{get;}	}
	
	[Serializable]
	public class FlexyAction_IntToFloat: IActionEntrance<Int64>, IActionExit<Single>
	{
		public void Do	( Int64 data )
		{
			_next.Do( data );
		}

		[SerializeReference] IActionEntrance<Single> _next;
		
		public IActionEntrance<Single> Next => _next;
	}
	
	[Serializable]
	public class FlexyAction_GenericConverter<TFrom, TTo>: IActionEntrance<TFrom>, IActionExit<TTo>
	{
		[SerializeField] String _converterPath; // Flexy.Scripting.Converters.Int16_Single
		
		private Func<TFrom, TTo> _converter;
		public void Do	( TFrom data )
		{
			_next.Do( _converter(data) );
		}

		[SerializeReference] IActionEntrance<TTo> _next;
		
		public IActionEntrance<TTo> Next => _next;
	}
	
	public static class Converters
	{
		public static	Single		SByte_Single	( SByte		data )		=> data;
		public static	Single		Byte_Single		( Byte		data )		=> data;
		public static	Single		Int16_Single	( Int16		data )		=> data;
		public static	Single		UInt16_Single	( UInt16	data )		=> data;
		public static	Single		Int32_Single	( Int32		data )		=> data;
		public static	Single		UInt32_Single	( UInt32	data )		=> data;
		public static	Single		Int64_Single	( Int64		data )		=> data;
		public static	Single		UInt64_Single	( UInt64	data )		=> data;
	}
	
	public class ActionCtx
	{
		public	Component			CtxObj;
		public	Object				RefValue;
		public	Int32				IntValue;
		public	Single				FloatValue;

		private Int32				_refCounter;
		
		public void IncreaseRef()
		{
			_refCounter++;
		}
		public void DecreaseRef()
		{
			_refCounter--;
			
			if( _refCounter <= 0 )
				Release( this );
		}
		
		private ActionCtx( ) { }
		
		private static List<ActionCtx> _ctxPool = new( );
		
		public static ActionCtx Rent( Component ctxObj )
		{
			var ctx = default(ActionCtx);
			
			if( _ctxPool.Count == 0 )
			{
				ctx = new( );
			}
			else
			{
				ctx = _ctxPool[^1];
				_ctxPool.RemoveAt( _ctxPool.Count-1 );
			}
			
			ctx.CtxObj = ctxObj;
			return ctx;
		}
		
		private static void Release( ActionCtx ctx )
		{
			ctx.Clear( );
			_ctxPool.Add( ctx );
		}
		
		private void Clear( )
		{
			CtxObj		= null;
			RefValue	= null;
			IntValue	= 0;
			FloatValue	= 0;
		}
	}
	
	// public class FCtxBig 
	// {
	// 	// increate every enter async action way 
	// 	// decrease every exit async action way
	// 	public Int32 AsyncTasksRunningCount = 0; 
 //        
	// 	[SerializeField] private String[]	_keys;
	// 	[SerializeField] private GameObject	_exposedObject;
	//
	// 	private readonly Dictionary<String, Object> _objectsDict	= new Dictionary<String, Object>( );
	// 	private readonly Dictionary<String, Int64>	_dataDict		= new Dictionary<String, Int64>( );
	// 	
	// 	public MonoBehaviour	Source			{ get; set; }
	// 	public Object			MainObject		{ get; set; }
	// 	public GameObject		ExposedObject	=> _exposedObject;
	//
	// 	public void SetValue    ( String key, Sprite value )	=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Enum value )		=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Color value )		=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, String value )	=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Int32 value )		=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Int64 value )		=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Boolean value )	=> _objectsDict[key] = value;
	// 	public void SetValue    ( String key, Single value )	
	// 	{
	// 		unsafe
	// 		{
	// 			var sp = &value;
	// 			var ip = (Int32*)sp;
	// 		
	// 			_dataDict[key] = *ip;
	// 		}
	// 	}
	// 	public void SetValue	( String key, Func<FCtx, Boolean> getter )		=> _objectsDict[key] = getter;
	// 	public void SetValue	( String key, Func<FCtx, Int64> getter )		=> _objectsDict[key] = getter;
	// 	public void SetValue	( String key, Func<FCtx, Int32> getter )		=> _objectsDict[key] = getter;
	// 	public void SetValue	( String key, Func<FCtx, Single> getter)		=> _objectsDict[key] = getter;
	//
	// 	// [Bindable]	public Int64	GetInt64	( String key )
	// 	// {
	// 	// 	return GetDataVal64( key );
	// 	// }
	// 	// [Bindable]	public Int32	GetInt      ( String key )
	// 	// {
	// 	// 	return GetDataVal( key );
	// 	// }
	// 	// [Bindable]	public Single	GetSingle   ( String key )
	// 	// {
	// 	// 	var val = GetDataVal( key );
	// 	// 	
	// 	// 	unsafe
	// 	// 	{
	// 	// 		var ip = &val;
	// 	// 		var sp = (Single*)ip;
	// 	// 	
	// 	// 		return *sp;
	// 	// 	}
	// 	// }
	// 	// [Bindable]	public Sprite	GetSprite   ( String key )
	// 	// {
	// 	// 	return (Sprite)GetObjectVal( key );
	// 	// }
	// 	// [Bindable]	public Color	GetColor	( String key )
	// 	// {
	// 	// 	return (Color)GetObjectVal( key );
	// 	// }
	// 	// [Bindable]	public String	GetString   ( String key )
	// 	// {
	// 	// 	return (String)GetObjectVal( key );
	// 	// }
	// 	// [Bindable]	public Boolean	GetBoolean  ( String key )
	// 	// {
	// 	// 	return GetDataVal( key ) != 0;
	// 	// }
	//
	// 	private	Object	GetObjectVal	( String key )
	// 	{
	// 		Object val;
	// 		return _objectsDict.TryGetValue( key, out val ) ? val : null;
	// 	}                           	
	// 	private	Int32	GetDataVal		( String key )
	// 	{
	// 		//try get data val
	// 		{
	// 			Int64 val;
	// 			if( _dataDict.TryGetValue( key, out val ) )
	// 				return (Int32)val;
	// 		}
	//
	// 		//try get dynamic val
	// 		{
	// 			Object val;
	// 			if( !_objectsDict.TryGetValue( key, out val ) )
	// 				return 0;
	//
	// 			switch( val )
	// 			{
	// 				case Func<FCtx, Int32> a:		return a(this);
	// 				case Func<FCtx, Boolean> a:	return a(this)? 1: 0;
	// 				case Func<FCtx, Single> a:		
	// 				{
	// 					var value = a(this);
	//
	// 					unsafe
	// 					{
	// 						var sp = &value;
	// 						var ip = (Int32*)sp;
	// 	
	// 						return *ip;
	// 					}
	// 				}
	// 			}
	// 		}
	//
	// 		return 0;
	// 	}
	// 	private	Int64	GetDataVal64	( String key )
	// 	{
	// 		//try get data val
	// 		{
	// 			Int64 val;
	// 			if( _dataDict.TryGetValue( key, out val ) )
	// 				return val;
	// 		}
	//
	// 		//try get dynamic val
	// 		{
	// 			Object val;
	// 			if( !_objectsDict.TryGetValue( key, out val ) )
	// 				return 0;
	// 			
	// 			switch( val )
	// 			{
	// 				case Func<FCtx, Int64> a: 		return a(this);
	// 				case Func<FCtx, Int32> a: 		return a(this);
	// 				case Func<FCtx, Boolean> a:	return a(this)? 1: 0;
	// 				case Func<FCtx, Single> a:		
	// 				{
	// 					var value = a(this);
	//
	// 					unsafe
	// 					{
	// 						var sp = &value;
	// 						var ip = (Int32*)sp;
	// 	
	// 						return *ip;
	// 					}
	// 				}
	// 			}
	// 		}
	//
	// 		return 0;
	// 	}
	// }
}