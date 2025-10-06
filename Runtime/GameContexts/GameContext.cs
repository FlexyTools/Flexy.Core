using System.Linq;
using System.Reflection;
using Flexy.AssetRefs;
using UnityEngine.SceneManagement;

namespace Flexy.Core.GameContexts
{
	[DefaultExecutionOrder(Int16.MinValue+200)]
	public class GameContext : MonoBehaviour
	{
		[RuntimeStaticClear]	static void StaticClear	( )
		{
			_global = null!;
			_sceneToCtxRegistry.Clear();
		}
		[RuntimeStaticInit]		static void StaticInit	( )
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

        [Header("Game Ctx")]
		[SerializeField]	GameObject?			_services;
		[SerializeField]	Boolean				_ignoreServiceInitFailures;
		public				ELinkCtxTo	        LinkTo;
		
		protected static	GameContext			_global = null!;
		private				GameContext?		_parent;
		private readonly	List<GameContext>	_children = new(4);
		private				Boolean				_isAlive;
		private				IGameContextExtension?	_ext;
		
		private static readonly		Dictionary<Scene, GameContext>	_sceneToCtxRegistry = new ( );
		private readonly			Dictionary<Type, Object>		_registeredServicesDict	= new ( );
		
		public static	GameContext		Global					=> _global.OrNull() is not null ? _global : _global = CreateGlobalContext();
		public			GameContext?	Parent					=> _parent;

		public			EInitializing	InitStatus				{ get; protected set; }

		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject );
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			RegisterGameScene		( Scene scene )		
		{
			Debug.Log( $"[GameCtx] {name} - Register scene: {scene.name}" );
			_sceneToCtxRegistry[scene] = this;
		}

		public	Boolean					IsAlive					=> _isAlive;

		protected		void			Awake					( )		
		{
			_isAlive = true;
			
			_ext = GetComponent<IGameContextExtension>( );
			
			if( _global == null )
			{
				_global = this;
				LinkTo = ELinkCtxTo.AllScenes;
				DontDestroyOnLoad( gameObject );
			}
			else if ( _parent == null )
			{
				_parent = transform.parent == null ? GetCtx(gameObject.scene) : GetCtx(transform.parent);
				_ext?.SetParent(_parent);
			}

			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Awake \t parent:{_parent}", this );

			switch( LinkTo )
			{
				case ELinkCtxTo.AllScenes:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: All", this );
					RegisterGameScene( _global.gameObject.scene );
					var count = SceneManager.sceneCount;
					for ( var i = 0; i < count; i++ )
						RegisterGameScene( SceneManager.GetSceneAt( i ) );

					break;
				}
				case ELinkCtxTo.LocalScene:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: One", this );
					RegisterGameScene( gameObject.scene );
					break;
				}
				default:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: None", this );
					break;
				}
			}

			RegisterCtxServices( );

			_ext?.RegisterAdditionalServices( _registeredServicesDict );
			
			if( _registeredServicesDict.Count > 0 )
			{
				var services				= _registeredServicesDict.Values.OfType<IService>( ).OrderBy( s => s.Order ).ToArray( );
				var asyncServices			= _registeredServicesDict.Values.OfType<IServiceAsync>( ).OrderBy( s => s.Order ).ToArray( );
				
				InitializeServices			( services );
				DoInitializeAsyncServices	( asyncServices ).Forget( Debug.LogException );
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
			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - OnDestroy \t parent:{_parent}", this );
			
			_isAlive = false;

			if ( !_parent )
				return;

			foreach ( var pair in _sceneToCtxRegistry.ToArray( ) )
			{
				if( pair.Value == this )
					_sceneToCtxRegistry[pair.Key] = _parent;
			}
		}

		public			void			SetParent				( GameContext ctx )				
		{
			_parent = ctx;
			_ext?.SetParent(_parent);
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
		public			T?				GetService<T>			( ) where T : class				
		{
			if( _registeredServicesDict.TryGetValue( typeof(T), out var svc ) )
				return svc as T;

			if( _parent )
			{
				var result = _parent.GetService<T>( );
				if( result != null )
					return result;
			}

			return _ext?.GetService<T>();
		}
		public			void			SetService<T>			( T service )	where T : class	
		{
			if( service == null )
				return;

			var typeActual	= service.GetType( );

			Debug.Log	( $"[GameCtx] {name} - SetService: {GetDisplayServiceName(typeActual)}" );
			try { _registeredServicesDict.Add( typeActual, service ); }
			catch ( Exception ex ) { Debug.LogException( ex ); }

			if ( typeActual.GetCustomAttribute<ServiceTypesAttribute>( ) is {} si )
			{
				foreach( var serviceType in si.InterfaceType )
				{
					if ( !serviceType.IsAssignableFrom( typeActual ) )
						continue;

					Debug.Log	( $"[GameCtx] {name} - SetService: {GetDisplayServiceName(serviceType)} => {GetDisplayServiceName(typeActual)}" );
					try { _registeredServicesDict.Add( serviceType, service ); }
					catch ( Exception ex ) { Debug.LogException( ex ); }
				}
			}

			#if VCONTAINER_PACKAGE
			// Services added dynamically after container build will not be added to VContainer and can not be resolved as dependency but only by GetService 
			#endif
		}

		public async		UniTask<EInitializing> WaitInitializing	( )								
		{
			while (InitStatus == EInitializing.InProgress)
				await UniTask.Yield();
			
			return InitStatus;
		}
		protected virtual 		void	InitializeServices		( IService[] services )				
		{
			foreach ( var service in services )
			{
				try						
				{ 
					service.OrderedInit( this ); 
				}
				catch ( Exception ex )	
				{
					Debug.LogException( ex );
					if( !_ignoreServiceInitFailures )
					{
						InitStatus = EInitializing.InitFail;
						break;
					}
				}
			}
		}
		protected virtual async	UniTask	InitializeAsyncServices	( IServiceAsync[] asyncServices )	
		{
			foreach ( var service in asyncServices )
			{
				try						
				{ 
					await service.OrderedInitAsync( this ); 
				}
				catch ( Exception ex )	
				{
					Debug.LogException( ex );
					if( !_ignoreServiceInitFailures )
					{
						InitStatus = EInitializing.InitFail;
						break;
					}
				}
			}
		}
		private async			UniTask	DoInitializeAsyncServices( IServiceAsync[] asyncServices )	
		{
			if( InitStatus == EInitializing.InitFail ) 
				return;
			
			await InitializeAsyncServices( asyncServices );
			
			if( InitStatus != EInitializing.InitFail ) 
				InitStatus = EInitializing.Done;
		}
		
		public static	String			GetDisplayServiceName	( Type svcType )								
		{
			var result = "";

			if( svcType.DeclaringType is {} dc )
				result = dc.Name + ".";

			if( svcType.IsGenericType )
			{
				result += svcType.Name[..^2] + "<" + svcType.GetGenericArguments()[0].Name + ">";
			}
			else
			{
				result += svcType.Name;
			}

			return result;
		}
		private static	GameContext		CreateGlobalContext		( )												
		{
			var go = new GameObject( "Flexy GlobalCtx Autogenerated", typeof(GameContext) );
			DontDestroyOnLoad( go );
			
			var ctx = go.GetComponent<GameContext>( );
			ctx.name = "Flexy GlobalCtx Autogenerated";
			
			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {ctx.name} - Autogenerate", ctx );

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
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.name} - Register created scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		private static 	void			RegisterSideLoadedScene	( Scene newScene, LoadSceneMode loadSceneMode )	
		{
			if( !_sceneToCtxRegistry.ContainsKey( newScene ) )
			{
				var scene	= SceneManager.GetActiveScene( );
				var ctx		= _sceneToCtxRegistry[scene];
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.name} - Register Side loaded scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		private static 	void			RegisterLoadedScene		( Scene ctx, Scene newScene )					
		{
			GetCtx( ctx ).RegisterGameScene( newScene );
		}

		public enum ELinkCtxTo: Byte
		{
			AllScenes,
			LocalScene,
			None
		}

		#if UNITY_EDITOR
		[RuntimeInspectorGui( Repaint = true )]
		public void RuntimeGUI	( )
		{
			if( !Application.isPlaying || !gameObject.scene.IsValid( ) )
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
						GUILayout.Label( $"{pair.Key.name} => {pair.Value.name}" );
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
				GUILayout.Label( $"{ctx.name} ({ctx.GetType().Name})");
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
							var key			= GetDisplayServiceName(pair.Key);
							var name		= GetDisplayServiceName(pair.Value.GetType());

							GUILayout.Label( key != name ? $"{key} => {name}" : $"{name}" );
						}
					}
					
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndHorizontal();
			}
		}
		#endif
	}
	
	public static class GameContextExt
	{
		public static T? GetService<T>( this MonoBehaviour src )	where T:class => GameContext.GetCtx( src ).GetService<T>();
		public static T? GetService<T>( this GameObject src )		where T:class => GameContext.GetCtx( src ).GetService<T>();
		public static T? GetService<T>( this Scene src )			where T:class => GameContext.GetCtx( src ).GetService<T>();
	}
	
	public enum EInitializing
	{
		InProgress = 0,
		InitFail = 1,
		Done = 2,
	}
	
	public interface ICachedContext
	{
		public GameContext	Ctx			{ get; set; }
		public Component	CallSource	{ get; set; }
	}
	
	public static class CachedContextExt
	{
		public static	T	GetCached<T>	( this ref T cache, Component callSource ) where T:struct, ICachedContext
		{
			if (cache.Ctx is { IsAlive: true }) 
				return cache;
				
			cache.Ctx = GameContext.GetCtx(callSource);
			cache.CallSource = callSource;

			return cache;
		}
	}
}