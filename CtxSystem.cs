using UnityEngine.Profiling;

namespace Flexy.Core
{
	public abstract class CtxSystem : MonoBehaviour
	{
		protected static T GetOrCreateRef<T> ( GameObject ctx ) where T:CtxSystem, new( )
		{
			var c		= GameContext.GetCtx( ctx );
			var svc		= c.GetService<T>( );
				
			if ( !ReferenceEquals( svc, null ) ) 
				return svc;
				
			svc = c.Systems.AddComponent<T>( ); 
			
			if( !svc )
				svc = new( );
			
			c.SetService( svc );

			return svc;
		}
	}

		
	public class CtxSystem<T> : CtxSystem where T:MonoBehaviour
	{
		private readonly	List<T>			_items	= new( );

		public				RoList<T>		Items	=> _items;

		private				void			Add				( T item ) => _items.Add( item );
		private				void			Remove			( T item ) => _items.Remove( item );
		
		public static 		void			AddItem			 ( T item )	  => GetOrCreateRef<CtxSystem<T>>( item.gameObject )									.Add	( item );
		public static 		void			RemoveItem		 ( T item )	  => GameContext.GetCtx( item.gameObject ).GetService<CtxSystem<T>>()					?.Remove( item );
		public static 		void			AddItemT	<TS> ( T item ) where TS: CtxSystem<T>, new() => GetOrCreateRef<TS>( item.gameObject )					.Add	( item );
		public static 		void			RemoveItemT	<TS> ( T item ) where TS: CtxSystem<T>, new() => GameContext.GetCtx( item.gameObject ).GetService<TS>()	?.Remove( item );
	}

	public abstract class CtxSystemLoop
	{
		protected CtxSystemLoop( ) 
		{
			_sampler		= CustomSampler.Create( $"{GetType().Name}" + (GetType().IsGenericType ? $"<{GetType().GetGenericArguments()[0].Name}>": "" ) ); 
		}
		private readonly	CustomSampler	_sampler;
			
		internal void			Loop		( )
		{
			_sampler.Begin( );
			LoopCore( );
			_sampler.End( );
		}
		protected abstract void	LoopCore	( );
	}

	public class CtxSystemLoop<T>: CtxSystemLoop where T:MonoBehaviour
	{
		public CtxSystemLoop( CtxSystem<T> owner )
		{
			_owner = owner;
		}
			
		private readonly	CtxSystem<T>	_owner;		
			
		private readonly	CustomSampler	_samplerInstance	= CustomSampler.Create( $"U:{typeof(T).Name}" );
			
		private				Int32			_currentIndex		= 0;

		public				Int32			CurrentIndex		=> _currentIndex;
			
		protected override	void	LoopCore		( )		
		{
			var instances = _owner.Items;
			for ( var i = 0; i < instances.Count; i++ )
			{
				var instance = instances[i];
					
	#if DEVELOPMENT_BUILD || UNITY_EDITOR
				_samplerInstance.Begin( instance );
	#endif
					
				_currentIndex = i;
				//try { instance.UpdateCore( ); } catch (Exception ex) { Debug.LogException( ex ); }
					
	#if DEVELOPMENT_BUILD || UNITY_EDITOR
				_samplerInstance.End( );
	#endif
			}
		}
	}
}