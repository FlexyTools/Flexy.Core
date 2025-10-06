namespace Flexy.Core;

public class RuntimeStaticClearAttribute :	RuntimeInitializeOnLoadMethodAttribute { public RuntimeStaticClearAttribute() :	base(RuntimeInitializeLoadType.SubsystemRegistration) { } }
public class RuntimeStaticInitAttribute :	RuntimeInitializeOnLoadMethodAttribute { public RuntimeStaticInitAttribute() :	base(RuntimeInitializeLoadType.AfterAssembliesLoaded) { } }