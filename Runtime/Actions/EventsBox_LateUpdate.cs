using Flexy.Core;
using Flexy.Core.Actions;

namespace asd.Module.Action.TurnBased
{

	public class EventsBox_LateUpdate : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_lateUpdate;
		
		private void LateUpdate		( ) => _lateUpdate	.Raise( this );
	}
}