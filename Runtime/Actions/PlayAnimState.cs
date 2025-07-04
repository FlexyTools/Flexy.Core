namespace Flexy.Core.Actions
{
	[Serializable]
	public class PlayAnimState : FlexyActionAsync
	{
		[SerializeField]	Animator	_animator;
		[SerializeField]	String		_stateName;
		
		public override async UniTask DoAsync( ActionCtx ctx )
		{
			_animator.Play( _stateName, 0 );
			
			await UniTask.Delay( TimeSpan.FromSeconds( _animator.GetNextAnimatorStateInfo(0).length ) );
		}
	}
}