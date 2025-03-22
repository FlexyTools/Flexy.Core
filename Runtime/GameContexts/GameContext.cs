using System.Linq;
using System.Reflection;
using System.Collections;
using Flexy.AssetRefs;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#if VCONTAINER_PACKAGE
using VContainer.Internal;
using VContainer;
using VContainer.Diagnostics;
using VContainer.Unity;
#endif

namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+200)]
#if VCONTAINER_PACKAGE
	public class GameContext : LifetimeScope
#else
	public class GameContext : MonoBehaviour
#endif
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void StaticClear( )
		{
			_global = null;
			_sceneToCtxRegistry.Clear( );

#if UNITY_EDITOR && VCONTAINER_PACKAGE
			var fld = typeof(DiagnositcsContext).GetField( "collectors", BindingFlags.Static | BindingFlags.NonPublic );
			fld.SetValue( null, new Dictionary<string, DiagnosticsCollector>() );

			var fld2 = typeof(DiagnositcsContext).GetField( "OnContainerBuilt", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
			fld2.SetValue(null, null);
#endif
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

        [Header("Game Ctx")]
		[SerializeField]	String				_name;
		[SerializeField]	GameObject			_services;
		[FormerlySerializedAs("SceneRegistration")]
		public				ELinkCtxTo	        LinkTo;

		protected static	GameContext			_global;
		internal			GameContext			_parent;
		private readonly	List<GameContext>	_children = new(4);
		private				GameObject			_systems;
		private				Boolean				_isAlive;
		
		private static readonly		Dictionary<Scene, GameContext>	_sceneToCtxRegistry = new ( );
		private readonly			Dictionary<Type, Object>		_registeredServicesDict	= new ( );
		

		public static	GameContext		Global					=> _global.OrNull( ) ?? (_global = CreateGlobalContext());
		public			GameContext		ParentContext			=> _parent;

		public			Boolean			InitDone				{ get; private set; }
		
		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject );
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene ); // go.transform.root.TryGetComponent<GameContext>( out var rootCtx ) ? rootCtx : GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			RegisterGameScene		( Scene scene )
		{
			Debug.Log( $"[GameCtx] {_name} - Register scene: {scene.name}" );
			_sceneToCtxRegistry[scene] = this;
		}

		public	Boolean					IsAlive					=> _isAlive;
		public	String					Name					=> _name;

		#if VCONTAINER_PACKAGE
		protected override		void	Configure				( IContainerBuilder builder )
		{
			foreach ( var group in _registeredServicesDict.GroupBy( p => p.Value, p => p.Key ) )
			{
				var rb = builder.RegisterInstance( group.Key );

				foreach ( var type in group )
					rb.As( type );
			}
		}
		#endif

		#if VCONTAINER_PACKAGE
		protected override 
		#else
		protected		 		
		#endif
						void			Awake					( )		{
			_isAlive = true;
			
			if( _global == null )
			{
				_global = this;
				LinkTo = ELinkCtxTo.AllScenes;
				DontDestroyOnLoad( gameObject );
			}
			else if ( _parent == null )
			{
				_parent = transform.parent == null ? GetCtx(gameObject.scene) : GetCtx(transform.parent);
				#if VCONTAINER_PACKAGE
				parentReference.Object = _parent;
				#endif
			}

			if( String.IsNullOrWhiteSpace( _name ) )
				_name = gameObject.name + " Context";

			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {_name} - Awake" );

			switch( LinkTo )
			{
				case ELinkCtxTo.AllScenes:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {_name} - Register Scenes: All" );
					RegisterGameScene( _global.gameObject.scene );
					var count = SceneManager.sceneCount;
					for ( var i = 0; i < count; i++ )
						RegisterGameScene( SceneManager.GetSceneAt( i ) );

					break;
				}
				case ELinkCtxTo.LocalScene:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {_name} - Register Scenes: One" );
					RegisterGameScene( gameObject.scene );
					break;
				}
				default:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {_name} - Register Scenes: None" );
					break;
				}
			}

			RegisterCtxServices( );

			#if VCONTAINER_PACKAGE
			base.Awake( );
			#endif
			
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
		#if VCONTAINER_PACKAGE
		protected override 
		#else
		protected		 	
		#endif
						void			OnDestroy				( )		
		{
			_isAlive = false;

			if ( !_parent )
				return;

			foreach ( var pair in _sceneToCtxRegistry.ToArray( ) )
			{
				if( pair.Value == this )
					_sceneToCtxRegistry[pair.Key] = _parent;
			}
			#if VCONTAINER_PACKAGE
			base.OnDestroy( );
			#endif
		}

		public			void			SetName					( String newName )
		{
			_name = newName;
		}
		public			void			SetParent				( GameContext ctx )
		{
			_parent = ctx;

			#if VCONTAINER_PACKAGE
			parentReference.Object = ctx;


			// Rebind internal container from new parent
			Build( );
			#endif
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
		public			T				GetService<T>			( ) where T : class
		{
			if( _registeredServicesDict.TryGetValue( typeof(T), out var svc ) )
				return svc as T;

			if( _parent )
			{
				var result = _parent.GetService<T>( );
				if( result != null )
					return result;
			}

			#if VCONTAINER_PACKAGE
			return Container.ResolveOrDefault<T>( );
			#else
			return default;
			#endif
		}
		public			void			SetService<T>			( T service )	where T : class
		{
			if( service == null )
				return;

			var typeActual	= service.GetType( );

			Debug.Log	( $"[GameCtx] {Name} - SetService: {GetDisplayServiceName(typeActual)}" );
			try { _registeredServicesDict.Add( typeActual, service ); }
			catch ( Exception ex ) { Debug.LogException( ex ); }

			if ( typeActual.GetCustomAttribute<ServiceTypesAttribute>( ) is {} si )
			{
				foreach( var serviceType in si.InterfaceType )
				{
					if ( !serviceType.IsAssignableFrom( typeActual ) )
						continue;

					Debug.Log	( $"[GameCtx] {Name} - SetService: {GetDisplayServiceName(serviceType)} => {GetDisplayServiceName(typeActual)}" );
					try { _registeredServicesDict.Add( serviceType, service ); }
					catch ( Exception ex ) { Debug.LogException( ex ); }
				}
			}

			#if VCONTAINER_PACKAGE
			// Services added dynamically after container build will not be added to VContainer and can not be resolved as dependency but only by GetService 
			#endif
		}

		protected virtual 		void	InitializeServices		( IService[] services )
		{
			foreach ( var service in services )
			{
				try						{ service.OrderedInit( this ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
			}
		}
		private async			UniTask	DoInitializeAsyncServices( IServiceAsync[] asyncServices )
		{
			await InitializeAsyncServices( asyncServices );
			InitDone = true;
		}
		protected virtual async	UniTask	InitializeAsyncServices	( IServiceAsync[] asyncServices )
		{
			foreach ( var service in asyncServices )
			{
				try						{ await service.OrderedInitAsync( this ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
			}
		}
		
		private static	String			GetDisplayServiceName	( Type svcType )
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
			var go = new GameObject( "Flexy Global Game Context", typeof(GameContext) );
			DontDestroyOnLoad( go );

			var ctx = go.GetComponent<GameContext>( );
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

		public enum ELinkCtxTo: Byte
		{
			AllScenes,
			LocalScene,
			None
		}

		#if UNITY_EDITOR
		[RuntimeInspectorUI( Repaint = true )]
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
				GUILayout.Label( $"{ctx.Name} ({ctx.GetType().Name})");
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
					
					#if VCONTAINER_PACKAGE
					GUILayout.Space( 10 );
					GUILayout.Label( "VContainer:" );
					GUILayout.Space( 4 );
					{
						var registry	= (Registry)ctx.Container	.GetType().GetField( "registry",	BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( ctx.Container );
						var hashTable	=				registry	.GetType().GetField( "hashTable",	BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( registry );
						var table		= (IList)		hashTable	.GetType().GetField( "table",		BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( hashTable );
						
						var typeField = default(FieldInfo);
						
						foreach ( IList arr in table )
						{
							if ( arr != null )
								
								foreach ( var item in arr )
								{
									if ( typeField == null )
										typeField	= item.GetType().GetField( "Type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
									
									var type = (Type)typeField.GetValue( item );
									
									if ( type == null )
										continue;
									
									
									var key			= GetDisplayServiceName(type);

									if( key is "Object" or "LifetimeScope" or "IObjectResolver" or "EntryPointDispatcher" )
										continue;
									
									GUILayout.Label( key );
								}
						}
					}
					#endif
					
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
		public static T GetService<T>( this GameObject context )	where T:class => context.scene.GetService<T>( );
		public static T GetService<T>( this MonoBehaviour context )	where T:class => context.gameObject.scene.GetService<T>( );
		public static T GetService<T>( this Scene context )			where T:class => GameContext.GetCtx( context ).GetService<T>();
	}
}
