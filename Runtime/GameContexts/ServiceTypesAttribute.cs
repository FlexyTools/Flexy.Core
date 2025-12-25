namespace Flexy.Core.GameContexts
{
	public class ServiceTypesAttribute: Attribute
	{
		public ServiceTypesAttribute ( Type firstInterfaceType, params Type[] additionalTypes )
		{
			InterfaceType = new List<Type>(1+additionalTypes.Length) { firstInterfaceType };
			InterfaceType.AddRange(additionalTypes);
		}
		
		public Boolean SkipImplementation = false;
		public readonly List<Type> InterfaceType;
	}
}