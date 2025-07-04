using Flexy.Core.Tweens;

namespace Flexy.Core.Actions;

[Serializable]
public class Move : FlexyActionAsync
{
	[SerializeField] Single		_duration;
	[SerializeField] Ease		_ease;
	[SerializeField] Vector3	_destination;

	
	public override async UniTask DoAsync( ActionCtx ctx )
	{
		var tr	= ctx.CtxObj.transform;
		await Tween.Position( tr, tr.position, _destination, _duration, _ease ).Run( );
	}
}
[Serializable]
public class MoveLocal : FlexyActionAsync
{
	[SerializeField] Single		_duration;
	[SerializeField] Ease		_ease;
	[SerializeField] Vector3	_destination;
	
	public override async UniTask DoAsync( ActionCtx ctx )
	{
		var tr	= ctx.CtxObj.transform;
		await Tween.LocalPosition( tr, tr.position, _destination, _duration, _ease ).Run( );
	}
}

[Serializable]
public class DeltaMove : FlexyActionAsync
{
	[SerializeField] Single		_duration;
	[SerializeField] Ease		_ease;
	[SerializeField] Vector3	_delta;
	
	public override async UniTask DoAsync( ActionCtx ctx )
	{
		var tr	= ctx.CtxObj.transform;
		var pos	= tr.position;
		await Tween.Position( tr, pos, pos + _delta, _duration, _ease ).Run( );
	}
}

[Serializable, MovedFrom("TweenLocalPositionDelta")]
public class DeltaMoveLocal : FlexyActionAsync
{
	[SerializeField] Single		_duration;
	[SerializeField] Ease		_ease;
	[SerializeField] Vector3	_delta;
	
	
	public override async UniTask DoAsync( ActionCtx ctx )
	{
		var tr	= ctx.CtxObj.transform;
		var pos	= tr.position;
		await Tween.LocalPosition( tr, pos, pos + _delta, _duration, _ease ).Run( );
	}
}

