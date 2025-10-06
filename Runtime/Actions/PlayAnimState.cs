namespace Flexy.Core.Actions
{
	[Serializable]
	public class PlayAnimState : FlexyActionAsync
	{
		[SerializeField]	Animator	_animator = null!;
		[SerializeField]	String		_stateName = null!;
		
		public override async UniTask DoAsync( ActionCtx ctx )
		{
			_animator.Play( _stateName, 0 );
			
			await UniTask.Delay( TimeSpan.FromSeconds( _animator.GetNextAnimatorStateInfo(0).length ) );
		}
	}
}