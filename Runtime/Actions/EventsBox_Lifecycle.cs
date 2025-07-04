using Flexy.Core;
using Flexy.Core.Actions;

namespace asd.Module.Action.TurnBased
{
	public class EventsBox_Lifecycle : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_awake;
		[SerializeField]	FlexyEvent	_start;
		[SerializeField]	FlexyEvent	_enable;
		[SerializeField]	FlexyEvent	_disable;
		[SerializeField]	FlexyEvent	_destroy;
		
		private void Awake		( ) => _awake	.Raise( this );
		private void Start		( ) => _start	.Raise( this );
		private void OnEnable	( ) => _enable	.Raise( this );
		private void OnDisable	( ) => _disable	.Raise( this );
		private void OnDestroy	( ) => _destroy	.Raise( this );
	}
}