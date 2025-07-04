using Flexy.Core.Tweens;

namespace Flexy.Core.Actions;

[Serializable]
public class Delay : FlexyActionAsync
{
	[SerializeField]	Single	_seconds;
	[SerializeField]	ETime	_time;
	
	public override UniTask DoAsync( ActionCtx ctx )
	{
		switch ( _time )
		{
			case ETime.DeltaTime:			return UniTask.Delay( (Int32)(_seconds * 1000), DelayType.DeltaTime );
			case ETime.UnscaledDeltaTime:	return UniTask.Delay( (Int32)(_seconds * 1000), DelayType.UnscaledDeltaTime );
			//case ETime.Realtime:		return UniTask.Delay( (Int32)(_time * 1000), DelayType.Realtime );
			default:					return UniTask.CompletedTask;
		}
	}
}