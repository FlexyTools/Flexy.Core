namespace Flexy.Core.Actions
{
	[Serializable]
	public class SetActive : FlexyActionSync
	{
		[SerializeField] GameObject	_target;
		[SerializeField] Boolean	_reverse;
		
		public override void Do(ActionCtx ctx)	
		{
			_target.SetActive( !_reverse );
		}
		public override void DoBack(ActionCtx ctx)	
		{
			_target.SetActive( _reverse );
		}
	}
}
