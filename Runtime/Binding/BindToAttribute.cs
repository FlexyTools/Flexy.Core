namespace Flexy.Core.Binding
{
	[AttributeUsage(AttributeTargets.Field|AttributeTargets.Class)]
	public class BindToAttribute: PropertyAttribute
	{
		public		BindToAttribute			( Type bindToType )			
		{
			BindToType = bindToType;
		}

		public		Type					BindToType;
	}
}