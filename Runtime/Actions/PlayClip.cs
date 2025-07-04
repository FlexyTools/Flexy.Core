namespace Flexy.Core.Actions;

[Serializable]
public class PlayClip : FlexyActionAsync
{
	[SerializeField]	Animation		_animation;
	[SerializeField]	AnimationClip	_clip;
	
	public override async UniTask DoAsync( ActionCtx ctx )
	{
		_animation.RemoveClip( "DynamicClip" );
		_animation.AddClip( _clip, "DynamicClip" );
		
		_animation.Play( "DynamicClip" );
		
		await UniTask.Delay( TimeSpan.FromSeconds( _clip.length ) );
	}
}