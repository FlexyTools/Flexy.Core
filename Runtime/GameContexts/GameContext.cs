using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.SceneManagement;

namespace Flexy.Core.GameContexts
{
	[HelpURL("https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/ScriptingApi/GameContext.md")]

	[DefaultExecutionOrder(Int16.MinValue+200)]
	public class GameContext : MonoBehaviour
	{
		[RuntimeStaticClear]	static void StaticClear	( )	
		{
			_global = null!;
			_sceneToCtxLinks.Clear();
		}
		[RuntimeStaticInit]		static void StaticInit	( )	
		{
			SceneManager.sceneUnloaded -= ClearSceneRegistration;
			SceneManager.sceneUnloaded += ClearSceneRegistration;

			SceneManager.activeSceneChanged -= RegisterCreatedScene;
			SceneManager.activeSceneChanged += RegisterCreatedScene;

			SceneManager.sceneLoaded -= RegisterSideLoadedScene;
			SceneManager.sceneLoaded += RegisterSideLoadedScene;

			#if FLEXY_ASSETREFS
			AssetRefs.AssetsLoader.NewSceneCreatedAndLoadingStarted -= RegisterLoadedScene;
			AssetRefs.AssetsLoader.NewSceneCreatedAndLoadingStarted += RegisterLoadedScene;
			#endif
		}

        [Header("Game Ctx")]
        [Tooltip("Optional GameObject to register service from")]
		[SerializeField]	GameObject?			_services;
		public				ELinkCtxTo	        LinkTo;
		
		protected static	GameContext			_global = null!;
		private				GameContext?		_parent;
		private readonly	List<GameContext>	_children = new(4);
		private				Boolean				_isAlive;
		private				IGameContextExtension?	_ext;
		
		private static readonly		Dictionary<Scene, GameContext>	_sceneToCtxLinks		= new();
		private readonly			Dictionary<Type, Object>		_registeredServicesDict	= new();
		private readonly			List<Object>					_registeredServicesList	= new();
		
		public static	GameContext		Global					=> _global.OrNull() is not null ? _global : _global = CreateGlobalContext();
		public			GameContext?	Parent					=> _parent;
		public	IGameContextExtension?	Ext						=> _ext;
		public	Boolean					IsAlive					=> _isAlive;

		public			EInitStatus		InitStatus				{ get; protected set; }
		public			Object?			InitializingService		{ get; protected set; }
		public			String?			FailReason				{ get; protected set; }

		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject );
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxLinks.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			LinkScene				( Scene scene )		
		{
			Debug.Log($"[GameCtx] {name} - Link scene: {scene.name}");
			_sceneToCtxLinks[scene] = this;
		}

		protected		void			Awake					( )		
		{
			_isAlive = true;
			
			_ext = GetComponent<IGameContextExtension>();
			
			if( _global == null )
			{
				_global = this;
				LinkTo = ELinkCtxTo.AllScenes;
				DontDestroyOnLoad(gameObject);
			}
			else if (_parent == null)
			{
				_parent = transform.parent == null ? GetCtx(gameObject.scene) : GetCtx(transform.parent);
				_ext?.SetParent(_parent);
				
				if (_parent)
					_parent._children.Add(this);
			}
			else
			{
				_ext?.SetParent(_parent);
			}

			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Awake \t parent:{_parent}", this );

			switch (LinkTo)
			{
				case ELinkCtxTo.AllScenes:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: All", this );
					LinkScene( _global.gameObject.scene );
					var count = SceneManager.sceneCount;
					for ( var i = 0; i < count; i++ )
						LinkScene( SceneManager.GetSceneAt(i) );

					break;
				}
				case ELinkCtxTo.LocalScene:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: One", this );
					LinkScene( gameObject.scene );
					break;
				}
				default:
				{
					Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - Register Scenes: None", this );
					break;
				}
			}

			RegisterCtxServices();
			
			if (_registeredServicesList.Count > 0)
			{
				var services				= _registeredServicesList.OfType<IService>()		.OrderBy( s => s.Order ).ToArray();
				var asyncServices			= _registeredServicesList.OfType<IServiceAsync>()	.OrderBy( s => s.Order ).ToArray();
				
				RunServiceInitialisation(services, asyncServices).Forget(Debug.LogException);
			}
		}
		protected		void			OnDestroy				( )		
		{
			_isAlive = false;
			
			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {name} - OnDestroy \t parent:{_parent}", this );
			
			if (!_parent)
				return;

			_parent!._children.Remove(this);

			foreach ( var pair in _sceneToCtxLinks.ToArray( ) )
			{
				if (pair.Value == this)
					_sceneToCtxLinks[pair.Key] = _parent;
			}
		}

		public			void			SetParent				( GameContext ctx )				
		{
			_parent = ctx;
			_ext?.SetParent(_parent);
		}
		public			T				GetService<T>			( ) where T : class				
		{
			if (_registeredServicesDict.TryGetValue(typeof(T), out var svc))
				return (T)svc;

			svc = _ext?.GetService<T>();

			if (svc != null)
				return (T)svc;

			if (_parent)
				return _parent!.GetService<T>();

			throw new InvalidOperationException( $"Service {typeof(T).Name} not found" );
		}
		public			Boolean			TryGetService<T>		( [NotNullWhen(true)] out T? service ) where T : class	
		{
			if (_registeredServicesDict.TryGetValue(typeof(T), out var svc))
			{
				service = (T)svc;
				return true;
			}

			svc = _ext?.GetService<T>();

			if (svc != null)
			{
				service = (T)svc;
				return true;
			}
			
			if (_parent != null)
				return _parent.TryGetService(out service);
				
			service = null;
			return false;
		}
		public			void			SetService<T>			( T service, Boolean replace = false )	where T : class	
		{
			if (service == null)
				return;

			var typeActual	= service.GetType();

			Debug.Log	( $"[GameCtx] {name} - SetService: {GetDisplayServiceName(typeActual)}" );
			try 
			{
				if (replace)	_registeredServicesDict[typeActual] = service;
				else			_registeredServicesDict.Add(typeActual, service); 
			}
			catch ( Exception ex ) { Debug.LogException(ex); }

			if (!_registeredServicesList.Contains(service))
				_registeredServicesList.Add(service);

			try { _ext?.SetService(typeActual, service, replace); }
			catch ( Exception ex ) { Debug.LogException(ex); }

			if (typeActual.GetCustomAttribute<ServiceTypesAttribute>() is {} si)
			{
				foreach (var serviceType in si.InterfaceType)
				{
					if (!serviceType.IsAssignableFrom(typeActual))
						continue;

					Debug.Log	( $"[GameCtx] {name} - SetService: {GetDisplayServiceName(serviceType)} => {GetDisplayServiceName(typeActual)}" );
					try 
					{
						if (replace)	_registeredServicesDict[serviceType] = service;
						else			_registeredServicesDict.Add(serviceType, service);
					}
					catch ( Exception ex ) { Debug.LogException(ex); }
					
					try { _ext?.SetService(serviceType, service, replace); }
					catch ( Exception ex ) { Debug.LogException(ex); }
				}
			}
		}
		public static	String			GetDisplayServiceName	( Type svcType )										
		{
			var result = "";

			if (svcType.DeclaringType is {} dc)
				result = dc.Name + ".";

			if (svcType.IsGenericType)
			{
				result += svcType.Name[..^2] + "<" + svcType.GetGenericArguments()[0].Name + ">";
			}
			else
			{
				result += svcType.Name;
			}

			return result;
		}

		public async UniTask<EInitStatus> WaitInitialization	( )									
		{
			while (InitStatus == EInitStatus.InProgress)
				await UniTask.Yield();
			
			return InitStatus;
		}
		protected virtual 		void	InitializeServices		( IService[] services )				
		{
			foreach (var service in services)
			{
				try						
				{ 
					service.OrderedInit(this); 
				}
				catch (Exception ex)	
				{
					Debug.LogException(ex);
					InitStatus = EInitStatus.Failed;
					FailReason = $"Exception: {ex.Message}, InnerException: {ex.InnerException?.Message}";
					break;
				}
			}
		}
		protected virtual async	UniTask	InitializeAsyncServices	( IServiceAsync[] asyncServices )	
		{
			foreach ( var service in asyncServices )
			{
				try						
				{ 
					InitializingService = service;
					await service.OrderedInitAsync(this); 
				}
				catch ( Exception ex )	
				{
					Debug.LogException(ex);
					InitStatus = EInitStatus.Failed;
					FailReason = $"Exception: {ex.Message}, InnerException: {ex.InnerException?.Message}";
					break;
				}
			}
			
			InitializingService = null;
		}
		private async			UniTask	RunServiceInitialisation( IService[] services, IServiceAsync[] asyncServices )	
		{
			InitializeServices(services);
		
			if (InitStatus == EInitStatus.Failed) 
				return;
			
			await InitializeAsyncServices( asyncServices );
			
			if (InitStatus != EInitStatus.Failed) 
				InitStatus = EInitStatus.Done;
		}

		private 		void			RegisterCtxServices		( )												
		{
			_registeredServicesDict.Add(typeof(GameContext), this);
		
			if (GetType() != typeof(GameContext))
				_registeredServicesDict.Add(GetType(), this);
		
			foreach (var svc in gameObject.GetComponents<MonoBehaviour>().Where( s => s is IService or IServiceAsync && s != this ) )
				SetService(svc);

			if (_services)
			{
				foreach (var svc in _services!.GetComponents<MonoBehaviour>())
					SetServiceImpl( svc );

				foreach (Transform tr in _services.transform)
				foreach (var svc in tr.GetComponents<MonoBehaviour>())
					SetServiceImpl(svc);
			}

			void SetServiceImpl( MonoBehaviour service )
			{
				if (!service)	//Probably script class was defined out on this platform
					return;

				if (service is ServiceProvider sp)
					sp.ProvideServices(this);
				else
					SetService(service);
			}
			
			_ext?.RegisterInitialServices( _registeredServicesDict );
		}
		private static	GameContext		CreateGlobalContext		( )												
		{
			if (!Application.isPlaying)
				return null!;
		
			var go = new GameObject( "Flexy GlobalCtx Autogenerated", typeof(GameContext) );
			DontDestroyOnLoad(go);
			
			var ctx = go.GetComponent<GameContext>( );
			ctx.name = "Flexy GlobalCtx Autogenerated";
			
			Debug.Log( $"[GameCtx] [Frame:{Time.frameCount}] {ctx.name} - Autogenerate", ctx );

			return ctx;
		}
		private static 	void			ClearSceneRegistration	( Scene scene )									
		{
			Debug.Log( $"{Time.frameCount} [GameCtx] ClearSceneRegistration {scene.name}" );
			_sceneToCtxLinks.Remove(scene);
		}
		private static 	void			RegisterCreatedScene	( Scene oldScene, Scene newScene )				
		{
			if (_sceneToCtxLinks.ContainsKey(oldScene) && !_sceneToCtxLinks.ContainsKey(newScene))
            {
				var ctx = _sceneToCtxLinks[oldScene];
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.name} - Register created scene: {newScene.name}" );
				ctx.LinkScene(newScene);
			}
		}
		private static 	void			RegisterSideLoadedScene	( Scene newScene, LoadSceneMode loadSceneMode )	
		{
			if (!_sceneToCtxLinks.ContainsKey(newScene))
			{
				var scene	= SceneManager.GetActiveScene();
				var ctx		= _sceneToCtxLinks[scene];
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.name} - Register Side loaded scene: {newScene.name}" );
				ctx.LinkScene(newScene);
			}
		}
		private static 	void			RegisterLoadedScene		( Scene ctx, Scene newScene )					
		{
			GetCtx( ctx ).LinkScene( newScene );
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
			if (!Application.isPlaying || !gameObject.scene.IsValid())
				return;

			GUILayout.Space(10);
			GUILayout.Label("Registered Services:", UnityEditor.EditorStyles.boldLabel);

			if (_parent)
			{
				GUILayout.Space(5);
				DrawCtxAndParentLine(this);
			}
			else
			{
				var ctxs = FindObjectsByType<GameContext>( FindObjectsInactive.Include, FindObjectsSortMode.None );

				Array.Sort(ctxs, (l, r) => IsInParent(l, r) ? -1 : 1);

				static Boolean IsInParent( GameContext l, GameContext r )
				{
					for (var ctx = l._parent; ctx != null; ctx = ctx._parent)
						if (ctx == r)
							return true;

					return false;
				}

				foreach (var context in ctxs)
				{
					GUILayout.Space(5);
					DrawCtx(context);
				}

				GUILayout.Space(10);
				GUILayout.Label("Scene -> Game Context  mapping", UnityEditor.EditorStyles.boldLabel);
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginVertical();

					foreach (var pair in _sceneToCtxLinks)
						GUILayout.Label( $"{pair.Key.name} => {pair.Value.name}" );

					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			static void DrawCtxAndParentLine( GameContext ctx )
			{
				DrawCtx(ctx);

				if (ctx._parent != null)
				{
					GUILayout.Space(5);
					DrawCtxAndParentLine(ctx._parent);
				}
			}
			
			static void DrawCtx( GameContext ctx )
			{
				//Header
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label( $"{ctx.name}");
					
					if (ctx._parent != null)
					{
						var orig = GUI.color;
						GUI.color = new Color(orig.r, orig.g, orig.b, 0.5f);
						GUILayout.Label( $" :  {ctx._parent.name}");
						GUI.color = orig;
					}
					
					GUILayout.FlexibleSpace();
					if (GUILayout.Button( "?" ))
					   UnityEditor.EditorGUIUtility.PingObject(ctx);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();
					{
						foreach (var pair in ctx._registeredServicesDict)
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
		public static T		GetService<T>( this Component src )		where T:class => GameContext.GetCtx(src).GetService<T>();
		public static T		GetService<T>( this GameObject src )	where T:class => GameContext.GetCtx(src).GetService<T>();
		public static T		GetService<T>( this Scene src )			where T:class => GameContext.GetCtx(src).GetService<T>();
		
		public static Boolean	TryGetService<T>( this Component src,	[NotNullWhen(true)] out T? service )	where T:class => GameContext.GetCtx(src).TryGetService(out service);
		public static Boolean 	TryGetService<T>( this GameObject src,	[NotNullWhen(true)] out T? service )	where T:class => GameContext.GetCtx(src).TryGetService(out service);
		public static Boolean 	TryGetService<T>( this Scene src,		[NotNullWhen(true)] out T? service )	where T:class => GameContext.GetCtx(src).TryGetService(out service);
	}
	
	public enum EInitStatus
	{
		InProgress	= 0,
		Failed		= 1,
		Done		= 2,
	}
	
	public interface ICachedContext
	{
		public GameContext	Ctx				{ get; set; }
		public Component	CallSource		{ get; set; }
		public void			RecacheFacade	( ){}
	}
	
	public static class CachedContextExt
	{
		public static	ref T	GetCached<T>	( this ref T cache, Component callSource )	where T:struct, ICachedContext	
		{
			if (cache.Ctx is { IsAlive: true }) 
				return ref cache;
				
			cache.Ctx = GameContext.GetCtx(callSource);
			cache.CallSource = callSource;
			cache.RecacheFacade();

			return ref cache;
		}
		public static	void	RecacheCtx<T>	( this ref T cache )						where T:struct, ICachedContext	
		{
			cache.Ctx = GameContext.GetCtx(cache.CallSource);
			cache.RecacheFacade();
		}
	}
	
	#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(GameContext), true)]
	public class GameContextEditor : Editor_WithRuntimeGui
	{
		public override void OnInspectorGUI()
		{
			var style = new GUIStyle(UnityEditor.EditorStyles.helpBox) { richText = true };
			GUILayout.Space(10);
			
			var message = "<size=16><b>Services registration flow</b></size>\n\n" +
			              "Register self to services\n" +
			              "Than all IService behaviours on this GameObject\n" +
			              "Then all MonoBehaviours from Services GameObject\n" +
			              "Than all MonoBehaviours from Services direct children";
			              
			var icon = UnityEditor.EditorGUIUtility.IconContent("console.infoicon").image as Texture2D;
			
			GUILayout.Label(new GUIContent(message, icon), style);
		}
	}
	#endif
}