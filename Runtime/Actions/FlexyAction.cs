namespace Flexy.Core.Actions
{
	[Serializable]
	public abstract class FlexyAction
	{
		public abstract				void		Do		( ActionCtx ctx );
		public abstract				UniTask		DoAsync	( ActionCtx ctx );
	}
	
	[Serializable]
	public abstract class FlexyActionSync : FlexyAction
	{
		public abstract override	void		Do		( ActionCtx ctx );
		public sealed override		UniTask		DoAsync	( ActionCtx ctx )	{ Do( ctx ); return UniTask.CompletedTask; }
	}
	
	[Serializable]
	public abstract class FlexyActionAsync : FlexyAction
	{
		public sealed override		void		Do		( ActionCtx ctx )	=> this.GuardedDoAsync( ctx ).Forget( Debug.LogException );
		public abstract override	UniTask		DoAsync	( ActionCtx ctx );
	}

	[Serializable]
	public struct FlexyEvent
	{
		[SerializeReference] FlexyAction?	_action;
		
		public UniTask Raise( Component ctxObj ) => _action.Raise( ctxObj );
		
		public event Action<ActionCtx> Raised	
		{
			add
			{
				if( _action is not FlexyActionCodeCallbacks cc )
				{
					cc = new( );
					cc.SetNext( _action );
					_action = cc;
				}
				
				cc.Raised += value;
			}
			remove
			{
				if( _action is FlexyActionCodeCallbacks cc )
					cc.Raised -= value;
			}
		}
	}
	
	[Serializable]
	public class FlexyActionCodeCallbacks : FlexyActionSync
	{
		[SerializeReference] FlexyAction?	_next;
		private List<Action<ActionCtx>>?	_callbacks;
		
		public event Action<ActionCtx>		Raised	
		{
			add		=> ( _callbacks ??= new( ) ).Add( value );
			remove	=> _callbacks?.Remove( value );
		}
		
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
		public			void	SetNext	( FlexyAction? next ) => _next = next;
	}
	
	public static class FlexyActionExtensions
	{
		public static			UniTask	Raise			( this FlexyAction? action, Component ctxObj )	
		{
			if( action == null )
				return UniTask.CompletedTask;
			
			return action.GuardedDoAsync( ActionCtx.Rent( ctxObj ) );
		}
		public static async		UniTask	GuardedDoAsync	( this FlexyAction action, ActionCtx ctx )		
		{
			try
			{
				ctx.IncreaseRef();
				await action.DoAsync( ctx );
			}
			finally
			{
				ctx.DecreaseRef();
			}
		}
	}
	
	public class ActionCtx
	{
		[RuntimeStaticClear]
		private void StaticClear() => _ctxPool = new();
	
		private static List<ActionCtx> _ctxPool = new();
		
		public	Component			CtxObj		= null!;
		public	Object?				RefValue;
		public	Int32				IntValue;
		public	Single				FloatValue;

		private Int32				_refCounter;
		
		public	void	IncreaseRef	( )	
		{
			_refCounter++;
		}
		public	void	DecreaseRef	( )	
		{
			_refCounter--;
			
			if( _refCounter <= 0 )
				Release( this );
		}
		
		public static	ActionCtx	Rent	( Component ctxObj )	
		{
			ActionCtx ctx;
			
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
		private static	void		Release	( ActionCtx ctx )		
		{
			ctx.CtxObj		= null!;
			ctx.RefValue	= null;
			ctx.IntValue	= 0;
			ctx.FloatValue	= 0;
			
			_ctxPool.Add( ctx );
		}
	}
}