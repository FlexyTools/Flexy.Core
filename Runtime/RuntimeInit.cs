namespace Flexy.Core;

public static class RuntimeInit
{
	public const RuntimeInitializeLoadType Clear	= RuntimeInitializeLoadType.SubsystemRegistration;
	public const RuntimeInitializeLoadType Init		= RuntimeInitializeLoadType.AfterAssembliesLoaded;
}