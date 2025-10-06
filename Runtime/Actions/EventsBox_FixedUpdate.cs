namespace Flexy.Core.Actions
{
	public class EventsBox_FixedUpdate : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_fixedUpdate;
		
		private void FixedUpdate		( ) => _fixedUpdate	.Raise( this );
	}
}