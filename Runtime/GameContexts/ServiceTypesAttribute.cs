namespace Flexy.Core.GameContexts
{
	public class ServiceTypesAttribute: Attribute
	{
		public ServiceTypesAttribute ( params Type[] interfaceType )
		{
			InterfaceType = interfaceType;
		}
		
		public readonly Type[] InterfaceType;
	}
}