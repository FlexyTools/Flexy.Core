namespace Flexy.Core.Actions
{
	[Serializable]
	public class SetEnabled : FlexyActionSync
	{
		[SerializeField] MonoBehaviour	_target;
		[SerializeField] Boolean		_enabled;
		
		public override void Do(ActionCtx ctx)
		{
			_target.enabled = _enabled;
		}
	}
}