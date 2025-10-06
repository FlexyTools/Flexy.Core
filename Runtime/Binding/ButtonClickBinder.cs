using Flexy.Core.Actions;
using UnityEngine.UI;

namespace Flexy.Core.Binding
{
	public class ButtonClickBinder : CallBinder
	{
		[SerializeField] Single 	EnableClickDelay		= 0.2f;
		[SerializeField] Single 	ReclickTimeout			= 0.5f;
		[SerializeField] Boolean	DoubleClickMode			= false;
		
		[Header("Events")]
		[SerializeField] FlexyEvent	Clicked;
		[SerializeField] FlexyEvent	Misclicked;

		private		Single	_lastClickTime;
		private		Single	_enableTime;
		private		Action?	_action;
		private		Button	_button = null!;

		private		void	Do			( )	
		{
			if( Time.unscaledTime < _enableTime + EnableClickDelay )
			{
				Misclicked.Raise( this );
				return;
			}

			if ( DoubleClickMode )
			{
				if ( Time.unscaledTime > _lastClickTime + ReclickTimeout )
				{
					_lastClickTime = Time.unscaledTime;
					
					Misclicked.Raise( this );
					return;
				}
			}
			else
			{
				if( Time.unscaledTime < _lastClickTime + ReclickTimeout )
				{
					Misclicked.Raise( this );
					return;
				}
				
				_lastClickTime = Time.unscaledTime;
			}
			
			_action?.Invoke( );
			
			Clicked.Raise( this );
		}

		private		void	OnEnable	( )	
		{
			_enableTime = Time.unscaledTime;
		}
		private		void	Awake		( )	
		{
			_button = GetComponent<Button>( );
			if( _button != null )
				_button.onClick.AddListener( Do );
			
			Init( ref _action );	
		}
		private		void	OnDestroy	( )	
		{
			if( _button )
				_button.onClick.RemoveListener( Do );
		}

		private		String	GetButtonNiceName	( Transform btn )	
		{
			if ( btn.name.Equals( "Button", StringComparison.InvariantCultureIgnoreCase ) )
				return GetButtonNiceName( btn.transform.parent );

			return btn.name.Replace( "Button", "" );
		}
	}
}