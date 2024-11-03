namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+1)]
    public sealed class MainContext : GameContext
    {
		[SerializeField] GlobalContext		_globalContext;
		[SerializeField] Boolean			_isSingleton;
		
		//very first Awake in scene thanks to DefaultExecutionOrder
		private new		void			Awake				( )		
		{
			var isDuplicate = CtxWithMyNameExists( );
			
			if ( isDuplicate )
			{
				DestroyImmediate( gameObject );
				return;
			}

			Debug.Log( $"{Time.frameCount} [MainContext] ----------- ===========   Game Initialisation Start   =========== -----------" );			
			
			if( !_global )
				Instantiate( _globalContext ).name = _globalContext.name;
			
			transform.parent = null;
			
			base.Awake( );
			
			if( _isSingleton )
				DontDestroyOnLoad( gameObject );
			
			Debug.Log( $"{Time.frameCount} [MainContext] ----------- ===========   Game Initialisation Done   =========== -----------" );
		}
		private			Boolean			CtxWithMyNameExists	( ) 	
		{
			if( !_global )
				return false;
			
			var ctx = GetCtx( gameObject );
				
			while ( ctx )
			{
				if ( ctx is MainContext tctx && tctx.Name == Name )
					return true;
					
				ctx = ctx._parent;
			}
				
			return default;
		}
	}
}
