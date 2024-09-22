using System.Linq;
using System.Reflection;
using Flexy.AssetRefs;
using Flexy.Utils;
using UnityEngine.SceneManagement;
using Debug = Flexy.Utils.Logger.Debug;

namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+2)]
	public class GameContext : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void StaticClear( )
		{
			_global = null; 
			_sceneToCtxRegistry.Clear( );
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void StaticBind( ) 
		{
			SceneManager.sceneUnloaded -= ClearSceneRegistration; 
			SceneManager.sceneUnloaded += ClearSceneRegistration; 
			
			SceneManager.activeSceneChanged -= RegisterCreatedScene; 
			SceneManager.activeSceneChanged += RegisterCreatedScene;
			
			SceneManager.sceneLoaded -= RegisterSideLoadedScene; 
			SceneManager.sceneLoaded += RegisterSideLoadedScene;
			
			AssetsLoader.NewSceneCreatedAndLoadingStarted -= RegisterLoadedScene;
			AssetsLoader.NewSceneCreatedAndLoadingStarted += RegisterLoadedScene;
		}
		
		[SerializeField]	String				_name;
		[SerializeField]	GameObject			_services;
		public				ESceneRegistration	SceneRegistration;
		
		protected static	GameContext			_global;
		internal			GameContext			_parent;
		private readonly	List<GameContext>	_children = new(4);
		private				EventBusHub			_localEventBus;
		
		protected 			FlexySystemGroup 		_group_EarlyUpdate;
		protected 			FlexySystemGroup 		_group_FixedUpdateFirst;
		protected 			FlexySystemGroup 		_group_FixedUpdateLast;
		protected 			FlexySystemGroup 		_group_UpdateFirst;
		protected 			FlexySystemGroup 		_group_UpdateLast;
		protected 			FlexySystemGroup 		_group_LateUpdateFirst;
		protected 			FlexySystemGroup 		_group_LateUpdateLast;

		private static readonly		Dictionary<Scene, GameContext>	_sceneToCtxRegistry = new ( );
		private readonly			Dictionary<Type, Object>		_registeredServicesDict	= new ( );
		private readonly			List<Object>					_registeredServicesList	= new ( );
		
		public static	GameContext		Global					=> _global.OrNull( ) ?? (_global = CreateGlobalContext());

		public			GameContext		ParentContext			=> _parent;
			
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );//go.TryGetComponent<GameContext>( out var selfCtx ) ? selfCtx : go.transform.root.TryGetComponent<GameContext>( out var rootCtx ) ? rootCtx : GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			RegisterGameScene		( Scene scene )		
		{
			Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register scene: {scene.name}" );
			_sceneToCtxRegistry[scene] = this;
		}
		
		public	String					Name					=> _name;
		public	EventBusHub				EventBus				=> _localEventBus ?? _global.EventBus;
		
		protected		void			Awake					( )		
		{
			if( _global == null )
			{
				_global = this;
				SceneRegistration = ESceneRegistration.Global;
				DontDestroyOnLoad( gameObject );
			}
			else if ( _parent == null )
			{
				_parent = transform.parent == null ? GetCtx(gameObject.scene) : GetCtx(transform.parent);
			}
			
			if( String.IsNullOrWhiteSpace( _name ) )
				_name = gameObject.name + " Context";
			
			Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Awake" );
			
			switch( SceneRegistration )
			{
				case ESceneRegistration.Global:
				{
					Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register Scenes: Global" );
					RegisterGameScene( _global.gameObject.scene );
					var count = SceneManager.sceneCount;
					for ( var i = 0; i < count; i++ )
						RegisterGameScene( SceneManager.GetSceneAt( i ) );
					
					break;
				}
				case ESceneRegistration.Local:
				{
					Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register Scenes: Local" );
					RegisterGameScene( gameObject.scene );
					break;
				}
				default:
				{
					Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register Scenes: None" );
					break;
				}
			}
			
			RegisterCtxServices( );
			
			if( _registeredServicesList.Count > 0 )
			{
				InitializeServices		( this );
				InitializeAsyncServices	( this ).Forget( Debug.LogException );
				
				static			void			InitializeServices				( GameContext ctx )		
				{
					var svcs = ctx._registeredServicesList.OfType<IService>( ).ToArray( ); 
					Array.Sort( svcs, (l, r) => l.Order - r.Order );
					foreach ( var service in svcs )
					{
						try						{ service.OrderedInit( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
				}
				static async	UniTask			InitializeAsyncServices			( GameContext ctx )		
				{
					var svcs = ctx._registeredServicesList.OfType<IServiceAsync>( ).ToArray( );
					Array.Sort( svcs, (l, r) => l.Order - r.Order );
					foreach ( var service in svcs )
					{
						try						{ await service.OrderedInitAsync( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
				}
			}
		}
		protected		void			OnEnable				( )		
		{
			if(_parent)
				_parent._children.Add( this );
		}
		protected		void			OnDisable				( )		
		{
			if(_parent)
				_parent._children.Remove( this );
		}
		protected		void			OnDestroy				( )		
		{
			if ( !_parent ) 
				return;
			
			foreach ( var pair in _sceneToCtxRegistry.ToArray( ) )
			{
				if( pair.Value == this )
					_sceneToCtxRegistry[pair.Key] = _parent;
			}
		}
		
		public			void			SetName					( String newName )									
		{
			_name = newName;
		}
		public			void			SetParent				( GameContext ctx )									
		{
			_parent = ctx;
		}
		public 			void			RegisterCtxServices		( )													
		{
			foreach ( var svc in gameObject.GetComponents<IService>( ) )
				SetService( svc );
			
			if( _services )
			{
				foreach ( var svc in _services.GetComponents<MonoBehaviour>( ) )
					SetServiceImpl( svc );
				
				foreach ( Transform tr in _services.transform )
					foreach ( var svc in tr.GetComponents<MonoBehaviour>( ) )
						SetServiceImpl( svc );
			}
			 
			void SetServiceImpl( MonoBehaviour service )
			{
				if ( !service )	//Probably script class was defined out on this platform
					return;

				if ( service is ServiceProvider sp )
					sp.ProvideServices( this );
				else
					SetService( service );
			}
		}
		public			T				GetService<T>			( Boolean searchHierarchy = true )						where T : class 			
		{
			if( _registeredServicesDict.TryGetValue( typeof(T), out var svc ) )
				return svc as T;

			if( _parent && searchHierarchy )
				return _parent.GetService<T>( );
			
			return default;
		}
		public			void			SetService<TBindFrom>	( TBindFrom service )	where TBindFrom : class		
		{
			if( service == null )
				return;
			
			var typeActual	= service.GetType();
			
			if( typeActual.GetCustomAttribute<ServiceInterfaceAttribute>( ) is {} si )
			{
				foreach( var serviceType in si.InterfaceType )
				{
					if ( !serviceType.IsAssignableFrom( typeActual ) ) 
						continue;
					
					Debug.Log	( $"[GameCtx] {Name} - SetService: {serviceType} => {serviceType.Name}" );
					_registeredServicesDict.Add( serviceType, service );
					_registeredServicesList.Add( service );
				}
			}
			else
			{
				Debug.Log	( $"[GameCtx] {Name} - SetService: {typeActual.Name}" );
				_registeredServicesDict.Add( typeActual, service );
				_registeredServicesList.Add( service );
			}
		}
		
		public			void			CreateLocalEventBus		( )													
		{
			_localEventBus = new( );
		}
		public			void			AddSystem				( ESystemGroup group, FlexySystem system )				
		{
			GetGroupByEnum( group ).Systems.Add( system );
		}
		public			void			RemoveSystem			( ESystemGroup group, FlexySystem system )				
		{
			var g = GetGroupByEnum( group );
			g.Systems.Remove( system );
		}
		
		protected		void			EarlyUpdate				( )		
		{
			_localEventBus?.ClearOldEvents( );

			_group_EarlyUpdate?.Update( );
			foreach ( var child in _children )
				child.EarlyUpdate( );
		}
		protected		void			FixedUpdateFirst		( )		
		{
			_group_FixedUpdateFirst?.Update( );
			foreach ( var child in _children )
				child.FixedUpdateFirst( );
		}
		protected		void			FixedUpdateLast			( )		
		{
			_group_FixedUpdateLast?.Update( );
			foreach ( var child in _children )
				child.FixedUpdateLast( );
		}
		protected		void			UpdateFirst				( )		
		{
			_group_UpdateFirst?.Update( );
			foreach ( var child in _children )
				child.UpdateFirst( );
		}
		protected		void			UpdateLast				( )		
		{
			_group_UpdateLast?.Update( );
			foreach ( var child in _children )
				child.UpdateLast( );
		}
		protected		void			LateUpdateFirst			( )		
		{
			_group_LateUpdateFirst?.Update( );
			foreach ( var child in _children )
				child.LateUpdateFirst( );
		}
		protected		void			LateUpdateLast			( )		
		{
			_group_LateUpdateLast?.Update( );
			foreach ( var child in _children )
				child.LateUpdateLast( );
		}
		
		private static	GameContext		CreateGlobalContext		( )													
		{
			var go = new GameObject( "Flexy Global Game Context", typeof(GameContext) );
			DontDestroyOnLoad( go );
			
			var ctx = go.GetComponent<GlobalContext>( );
			ctx._name = "Flexy Global Game Context";
			
			return ctx;
		}
		private static 	void			ClearSceneRegistration	( Scene scene )										
		{
			Debug.Log( $"{Time.frameCount} [GameCtx] ClearSceneRegistration {scene.name}" );
			_sceneToCtxRegistry.Remove( scene );
		}
		private static 	void			RegisterCreatedScene	( Scene oldScene, Scene newScene )					
		{
			if( _sceneToCtxRegistry.ContainsKey( oldScene ) && !_sceneToCtxRegistry.ContainsKey( newScene ) )
            {
				var ctx = _sceneToCtxRegistry[oldScene];
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.Name} - Register created scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		private static 	void			RegisterSideLoadedScene	( Scene newScene, LoadSceneMode loadSceneMode )		
		{
			if( !_sceneToCtxRegistry.ContainsKey( newScene ) )
			{
				var scene	= SceneManager.GetActiveScene( );
				var ctx		= _sceneToCtxRegistry[scene]; 
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.Name} - Register Side loaded scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		private static 	void			RegisterLoadedScene		( Scene ctx, Scene newScene )						
		{
			GetCtx( ctx ).RegisterGameScene( newScene );
		}
		
		private			FlexySystemGroup		GetGroupByEnum			( ESystemGroup group )								
		{
			return group switch
			{
				ESystemGroup.EarlyUpdate		=>	_group_EarlyUpdate		??= new(group.ToString()),
				ESystemGroup.FixedUpdate		=>	_group_FixedUpdateFirst	??= new(group.ToString()),
				ESystemGroup.FixedUpdateLast	=>	_group_FixedUpdateLast	??= new(group.ToString()),
				ESystemGroup.Update				=>	_group_UpdateFirst		??= new(group.ToString()),
				ESystemGroup.UpdateLast			=>	_group_UpdateLast		??= new(group.ToString()),
				ESystemGroup.LateUpdate			=>	_group_LateUpdateFirst	??= new(group.ToString()),
				ESystemGroup.LateUpdateLast		=>	_group_LateUpdateLast	??= new(group.ToString()),
				
				_ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
			};
		}
		
		public enum ESceneRegistration: Byte
		{
			Global,
			Local,
			None
		}
		
		public enum ESystemGroup : Byte
		{
			EarlyUpdate,	
			FixedUpdate,		
			FixedUpdateLast,		
			Update,				
			UpdateLast,			
			LateUpdate,			
			LateUpdateLast
		}
		
		#if UNITY_EDITOR
		[RuntimeInspectorUI( Repaint = true )]
		public void RuntimeGUI	( )		
		{
			if( !Application.isPlaying )
				return;
			
			GUILayout.Space( 10 );
			GUILayout.Label( "Registered Services:" );
			
			if( _parent )
			{
				GUILayout.Space( 5 );
				DrawCtxAndParentLine( this );
			}
			else
			{
				var ctxs = FindObjectsByType<GameContext>( FindObjectsInactive.Include, FindObjectsSortMode.None );

				Array.Sort( ctxs, ( l, r ) => IsInParent( l, r ) ? -1 : 1 );
				
				static Boolean IsInParent( GameContext l, GameContext r )
				{
					for ( var ctx = l._parent; ctx != null; ctx = ctx._parent )
						if( ctx == r )
							return true;
					
					return false;
				}
				
				foreach ( var context in ctxs )
				{
					GUILayout.Space( 5 );
					DrawCtx( context );
				}
				
				
				GUILayout.Space( 10 );
				GUILayout.Label("Scene To Ctx");
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginVertical( );

					foreach ( var pair in _sceneToCtxRegistry )
					{
						GUILayout.Label( $"{pair.Key.name} => {pair.Value.Name}" );
					}
					GUILayout.EndVertical( );
				}
				GUILayout.EndHorizontal( );
			}
			
			static void DrawCtxAndParentLine( GameContext ctx )
			{
				DrawCtx( ctx );
				
				if( ctx._parent != null )
				{
					GUILayout.Space( 5 );
					DrawCtxAndParentLine( ctx._parent );
				}
			}
			static void DrawCtx( GameContext ctx )
			{
				//Header
				GUILayout.BeginHorizontal(  );
				GUILayout.Label(ctx.Name);
				GUILayout.FlexibleSpace();
				if( GUILayout.Button( "?" ))
				   UnityEditor.EditorGUIUtility.PingObject( ctx );
				GUILayout.EndHorizontal( );

				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginHorizontal( );
					GUILayout.BeginVertical();
					{
						foreach ( var pair in ctx._registeredServicesDict )
						{
							var key			= pair.Key.Name;
							var name		= pair.Value.GetType().Name;
							
							GUILayout.Label( key != name ? $"{key} => {name}" : $"{name}" );
						}
					}
					GUILayout.EndVertical();
					// GUILayout.Space( 100 );
					// GUILayout.BeginVertical();
					// {
					// 	foreach ( var pair in ctx._registeredServices )
					// 	{
					// 		var name		= pair.Value.GetType().Name;
					// 		var fullName	= pair.Value.GetType().FullName;
					//
					// 		GUILayout.Label( $"ns: {fullName.Replace(name, "")}" );
					// 	}
					// }
					// GUILayout.EndVertical();
					// GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndHorizontal();
			}
		}
		#endif
	}

	public static class GameContextExt
	{
		public static T GetService<T>( this GameObject context )	where T:class => context.scene.GetService<T>( );
		public static T GetService<T>( this MonoBehaviour context )	where T:class => context.gameObject.scene.GetService<T>( );
		public static T GetService<T>( this Scene context )			where T:class => GameContext.GetCtx( context ).GetService<T>();
	}
}