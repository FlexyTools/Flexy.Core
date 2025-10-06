namespace Flexy.Core.Actions
{
	public class EventsBox_LateUpdate : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_lateUpdate;
		
		private void LateUpdate		( ) => _lateUpdate	.Raise( this );
	}
}