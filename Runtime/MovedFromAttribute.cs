namespace Flexy.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class MovedFromAttribute : UnityEngine.Scripting.APIUpdating.MovedFromAttribute
{
	public MovedFromAttribute( String sourceClassName, String sourceNamespace = null, String sourceAssembly = null, Boolean autoUpdateApi = true ) : base(autoUpdateApi, sourceNamespace, sourceAssembly, sourceClassName)
	{
	}
}