namespace Flexy.Core.Binding
{
	public interface IBindersNotifier
	{
		Boolean ReadyForBind		
		{
			get;
		}
		
		void AttachBinder( Binder binder );
		void DetachBinder( Binder binder );
	}
}