using UnityEngine.Assertions;

namespace Flexy.Core
{
	[Serializable]
    public abstract class XComponent						//Base class to prevent using Unity classes as components 
    {
		public XObject	Owner {get; internal set;}
		public String	Name => Owner.Name;
		protected internal virtual void OnValidate( XObject obj ) { }
    }
    
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class XComponentAllowAttribute : Attribute
    {
        public readonly Type BaseXComponentType;
        public readonly Boolean AllowDuplicateComponents;

        public XComponentAllowAttribute( Type baseXComponentType, Boolean allowDuplicateComponents )
        {
            Assert.IsTrue( typeof(XComponent).IsAssignableFrom( baseXComponentType ), $"{baseXComponentType} is not GdiComponent" );

            BaseXComponentType = baseXComponentType;
            AllowDuplicateComponents = allowDuplicateComponents;
        }
    }
	
	[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class XComponentRequireAttribute : Attribute
	{
		public readonly Type RequiredXComponent;
		public readonly Boolean AllowDuplicateComponents;

		public XComponentRequireAttribute( Type baseXComponentType, Boolean allowDuplicateComponents = false )
		{
			Assert.IsTrue( typeof(XComponent).IsAssignableFrom( baseXComponentType ), $"{baseXComponentType} is not GdiComponent" );

			RequiredXComponent = baseXComponentType;
			AllowDuplicateComponents = allowDuplicateComponents;
		}
	}
    
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class XComponentBasedAttribute : Attribute
	{
		public Boolean	NoMainComponent;
		public Type		BaseComponentType;
	}
	
	[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class XComponentMainAttribute : Attribute { }
	
	[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class XComponentAttribute : Attribute 
	{
		public Boolean AllowDuplicateComponents;
	}
}