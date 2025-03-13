namespace Flexy.Core.Tweens
{
	public class FlexyTweenService : MonoBehaviour
	{
		public static FlexyTweenService _service;
		private readonly Dictionary<Int64, TweenRunner> _runners = new( );
		
		public	TweenHandle		Run<T, TLerp>	( Builder<T, TLerp> tween ) where T : unmanaged where TLerp : unmanaged, ITweenLerper<T>	
		{
			var id = (Int64)typeof(T).TypeHandle.Value;
			
			if( !_runners.TryGetValue( id, out var runner ) )
				_runners.Add( id, ( runner = new TweenRunner<T, TLerp>( ) ) );
			
			return ((TweenRunner<T, TLerp>)runner).Run( tween );
		}
		public	void			Complete		( TweenHandle handle )		
		{
			if( !_runners.TryGetValue( handle.Id, out var runner ) )
				return;
			
			runner.Complete( handle );
		}
		public	void			Cancel			( TweenHandle handle )		
		{
			if( !_runners.TryGetValue( handle.Id, out var runner ) )
				return;
			
			runner.Cancel( handle );
		}
		public	Boolean			IsValid			( TweenHandle handle )		
		{
			if( !_runners.TryGetValue( handle.Id, out var runner ) )
				return false;
			
			return runner.IsValid( handle );
		}
		public	UniTask.Awaiter	Await			( TweenHandle handle )		
		{
			if( !_runners.TryGetValue( handle.Id, out var runner ) )
				return UniTask.CompletedTask.GetAwaiter( );
			
			return runner.Await( handle );
		}
		
		private	void			Awake	( )		
		{
			_service = this;
		}
		private	void			Update	( )		
		{
			foreach ( var r in _runners )
				r.Value.Update( );
		}
		
		public class Runner : TweenBackend
		{
			public override		TweenHandle		Run<T, TLerp>( Builder<T, TLerp> tween )	=> _service.Run( tween );
			public override		void			Complete	( TweenHandle h )				=> _service.Complete( h );
			public override		void			Cancel		( TweenHandle h )				=> _service.Cancel( h );
			public override		Boolean			IsValid		( TweenHandle h )				=> _service.IsValid( h );
			public override		UniTask.Awaiter	Await		( TweenHandle h )				=> _service.Await( h );
		}
	
		private abstract class TweenRunner
		{
			public abstract	void			Complete	( TweenHandle handle );
			public abstract	void			Cancel		( TweenHandle handle );
			public abstract	Boolean			IsValid		( TweenHandle handle );
			public abstract	UniTask.Awaiter	Await		( TweenHandle handle );
			
			public abstract void Update( );
		}
		
		private class TweenRunner<T, TLerp> : TweenRunner where T : unmanaged where TLerp : unmanaged, ITweenLerper<T>
		{
			private readonly	List<Tween>		_tweens		= new ( 32 );
			
			public				TweenHandle		Run			( Builder<T, TLerp> tween )	
			{
				var id = (Int64)typeof(T).TypeHandle.Value;
				
				for ( var i = 0; i < _tweens.Count; i++ )
				{
					var t = _tweens[i];
					if ( t.Data.LoopsCount == 0 ) //Empty
					{
						_tweens[i] = new() { Data = tween.Data, CurrentValue = tween.Data.From, Version = _tweens[i].Version };
						return new(){ Id = id, SubId = i, Version = _tweens[i].Version };
					}
				}
				
				_tweens.Add( new() { Data = tween.Data, CurrentValue = tween.Data.From, Version = 1 } );
				return new(){ Id = id, SubId = _tweens.Count - 1, Version = 1 };
			}
			public override		void			Update		( )							
			{
				// Advance tweens
				for ( var i = 0; i < _tweens.Count; i++ )
				{
					var tween = _tweens[i];
					if ( tween.Data.LoopsCount == 0 ) //Empty
						continue;
					
					var dt = tween.Data.TweenTime switch
					{
						ETime.DeltaTime			=> Time.deltaTime,
						ETime.UnscaledDeltaTime	=> Time.unscaledDeltaTime,
						//ETweenTime.Realtime	=> Time.realtimeSinceStartup,
						_						=> Time.deltaTime
					};
					
					tween.t += dt / (tween.Data.DurationMs / 1000.0f);
					
					if( tween.t < 0 ) // delayed tween
						continue;
					
					var easedT			= tween.Data.Ease.Evaluate( Mathf.Clamp01( tween.t ) );  
					tween.CurrentValue	= tween.Data.Lerper.Lerp( tween.Data.From, tween.Data.To, easedT );
					
					_tweens[i] = tween;
				}
				
				// Invoke callbacks
				for ( var i = 0; i < _tweens.Count; i++ )
				{
					var tween = _tweens[i];
					if ( tween.Data.LoopsCount == 0 ) //Empty
						continue;
					
					try						{ tween.Data.BindData.InvokeUnsafe( tween.CurrentValue ); }
					catch ( Exception ex )	{ Debug.LogException( ex ); }
					
					if( tween.t >= 1 )
					{
						try						{ tween.Data.Callbacks.Completed?.Invoke( ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
						
						var nextVersion = tween.Version + 1;
						tween = default;
						tween.Version = nextVersion;
						_tweens[i] = tween;
					}
				}
			}
			
			public override		void			Complete	( TweenHandle handle )		
			{
				var index	= handle.SubId;
				var version	= handle.Version;
				
				if( _tweens.Count <= index )
					return;
				
				var tween = _tweens[index]; 
				
				if( tween.Version != version )
					return;
				
				try						{ tween.Data.BindData.InvokeUnsafe( tween.Data.To ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
				
				try						{ tween.Data.Callbacks.Completed( ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
				
				tween = default;
				tween.Version = version + 1;
				_tweens[index] = tween;
			}
			public override		void			Cancel		( TweenHandle handle )		
			{
				var index	= handle.SubId;
				var version	= handle.Version;
				
				if( _tweens.Count <= index )
					return;
				
				var tween = _tweens[index];
				
				if( tween.Version != version )
					return;
				
				try						{ tween.Data.Callbacks.Canceled( ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
				
				tween = default;
				tween.Version = version + 1;
				_tweens[index] = tween;
			}
			public override		Boolean			IsValid		( TweenHandle handle )		
			{
				var index	= handle.SubId;
				var version	= handle.Version;
				
				if( _tweens.Count <= index )
					return false;
				
				var tween = _tweens[index];
				
				if( tween.Version != version )
					return false;
				
				return true;
			}
			public override		UniTask.Awaiter	Await		( TweenHandle handle )		
			{
				if( !IsValid( handle ) )
					return UniTask.CompletedTask.GetAwaiter( );
				
				return Awaiter( _tweens, handle.SubId, handle.Version ).GetAwaiter( );
				
				static async UniTask Awaiter( List<Tween> tweens, Int32 index, Int32 version )
				{
					while( tweens[index].Version == version )
						await UniTask.DelayFrame( 1 );
				}
			}
			
			private struct Tween
			{
				public TweenData<T, TLerp>	Data;
				public T					CurrentValue;
				public Single				t;
				public Int32				Version;
			}
		}
	}
}