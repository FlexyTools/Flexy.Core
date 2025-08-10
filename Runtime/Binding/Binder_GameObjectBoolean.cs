namespace Flexy.Core.Binding
{
	[BindTo(typeof(Boolean))]
	public class Binder_GameObjectBoolean : Binder
	{
		[SerializeField]	Boolean			_disableObjectsOnAwake;
		[SerializeField]	GameObject		_true;
	    [SerializeField]	GameObject		_false;

	    private				Func<Boolean>	_getter;

	    private				void	Awake		( )					
	    {
		    if( _disableObjectsOnAwake )
		    {
			    if( _true != null )		_true	.SetActive( false );
			    if( _false != null )	_false	.SetActive( false );
		    }

		    Init(ref _getter);
	    }

		protected override	void	OnDestroy	( )
		{
			if( _disableObjectsOnAwake )
		    {
			    if( _true != null )		_true	.SetActive( false );
			    if( _false != null )	_false	.SetActive( false );
		    }
			
			base.OnDestroy( );
		}

		protected override void OnDisable()
		{
			if( _disableObjectsOnAwake )
		    {
			    if( _true != null )		_true	.SetActive( false );
			    if( _false != null )	_false	.SetActive( false );
		    }
			
			base.OnDisable();
		}

		protected override	void	Bind	( Boolean init )	
		{
			var isTrue = _getter();

			if( _true != null	&& _true.activeSelf != isTrue )		_true	.SetActive( isTrue );
			if( _false != null	&& _false.activeSelf != !isTrue )	_false	.SetActive( !isTrue );
		}
	}
}