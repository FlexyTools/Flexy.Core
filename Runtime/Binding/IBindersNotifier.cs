namespace Flexy.Core.Binding
{
	public interface IBindersNotifier
	{
		Boolean ReadyForBind		
		{
			get;
		}
		
		void AttachBinder( ABinder binder );
		void DetachBinder( ABinder binder );
	}
}