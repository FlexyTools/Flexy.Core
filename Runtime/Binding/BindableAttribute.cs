namespace Flexy.Core.Binding
{
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method )]
	public class BindableAttribute : UnityEngine.Scripting.PreserveAttribute 
	{
		public BindableAttribute	( ){ }
		public BindableAttribute	( String? description, Boolean isWarning )
		{
			Description	= description;
			IsWarning	= isWarning;
		}

		public	String?		Description;
		public	Boolean		IsWarning;

		/// <summary>
		/// Specify narrower type than bindable member. Used in editor to show bindable members of expected dynamic type in inspector 
		/// Can be field or property name of type String or Type
		/// </summary>
		public	String?		TypeProvider;
	}
	
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method )]
	public class BindableGroupAttribute : UnityEngine.Scripting.PreserveAttribute  
	{
		public BindableGroupAttribute	( String groupName ) => GroupName = groupName;
		
		public String GroupName;
	}

}