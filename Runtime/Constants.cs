namespace Flexy.Core;

public static class Constants
{	
	#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG
	public const Boolean	IsDevBuildOrEditor =	true;
	#else
	public const Boolean	IsDevBuildOrEditor =	false;
	#endif
	
	public const Boolean	IsClient	= !Constants.IsServer;
	public const Boolean	IsServer	= IsUnityServer || NetCore;
	
		
	#if NETCOREAPP
	public const Boolean	NetCore = true;
	#else
	public const Boolean	NetCore = false;
	#endif
	
	#if UNITY_EDITOR
	public const Boolean	IsEditor = true;
	#else
	public const Boolean	IsEditor = false;
	#endif
	
	#if UNITY_SERVER 
	public const Boolean	IsUnityServer = true;
	#else
	public const Boolean	IsUnityServer = false;
	#endif
	
	#if UNITY_ANDROID
	public const Boolean	IsAndroid =	true;
	#else
	public const Boolean	IsAndroid =	false;
	#endif
	
	#if UNITY_IOS
	public const Boolean	IsIos =	true;
	#else
	public const Boolean	IsIos =	false;
	#endif
	
	#if UNITY_STANDALONE
	public const Boolean	IsStandalone = true;
	#else
	public const Boolean	IsStandalone = false;
	#endif

	#if UNITY_STANDALONE_WIN
	public const Boolean	IsStandaloneWin = true;
	#else
    public const Boolean	IsStandaloneWin = false;
	#endif

	#if UNITY_STANDALONE_OSX
	public const Boolean	IsStandaloneOsx = true;
	#else
	public const Boolean	IsStandaloneOsx = false;
	#endif
    
	#if UNITY_WSA
	public const Boolean	IsUWP =	true;
	#else
	public const Boolean	IsUWP =	false;
	#endif

	public const Boolean	IsStandaloneOrUWP	= IsStandalone || IsUWP;
	public const Boolean	IsMobile			= IsAndroid || IsIos;
}