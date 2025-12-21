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
		public sealed override		UniTask		DoAsync	( ActionCtx ctx )	{ Do(ctx); return UniTask.CompletedTask; }
	}
	
	[Serializable]
	public abstract class FlexyActionAsync : FlexyAction
	{
		public sealed override		void		Do		( ActionCtx ctx )	=> this.GuardedDoAsync(ctx).Forget();
		public abstract override	UniTask		DoAsync	( ActionCtx ctx );
	}

	[Serializable]
	public struct FlexyEvent
	{
		[SerializeReference] FlexyAction?	_action;
		
		public UniTask Raise( Component srcObject ) => _action.Raise(srcObject);
		
		public event Action<ActionCtx> Raised	
		{
			add
			{
				if (_action is not FlexyActionCodeCallbacks cc)
				{
					cc = new();
					cc.SetNext(_action);
					_action = cc;
				}
				
				cc.Raised += value;
			}
			remove
			{
				if (_action is FlexyActionCodeCallbacks cc)
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
			add		=> ( _callbacks ??= new() ).Add(value);
			remove	=> _callbacks?.Remove(value);
		}
		
		public override void	Do		( ActionCtx ctx )	
		{
			if (_callbacks is { Count: > 0 })
			{
				using var tmpList = TempList<Action<ActionCtx>>.Rent(_callbacks);
				
				foreach ( var action in tmpList )
				{
					try						{ action.Invoke(ctx);		}
					catch ( Exception ex )	{ Debug.LogException(ex);	}
				}
			}
			
			_next?.Do(ctx);
		}
		public			void	SetNext	( FlexyAction? next ) => _next = next;
	}
	
	public static class FlexyActionExtensions
	{
		public static			UniTask	Raise			( this FlexyAction? action, Component srcObject )	
		{
			if (action == null)
				return UniTask.CompletedTask;
			
			return action.GuardedDoAsync( new ActionCtx{ SrcObject = srcObject } );
		}
		public static async		UniTask	GuardedDoAsync	( this FlexyAction action, ActionCtx ctx )			
		{
			try
			{
				await action.DoAsync(ctx);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}
	
	public struct ActionCtx
	{
		public	Component	SrcObject;
	}
}