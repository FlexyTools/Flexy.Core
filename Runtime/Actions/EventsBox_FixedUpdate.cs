using Flexy.Core;
using Flexy.Core.Actions;

namespace asd.Module.Action.TurnBased
{
	public class EventsBox_FixedUpdate : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_fixedUpdate;
		
		private void FixedUpdate		( ) => _fixedUpdate	.Raise( this );
	}
}