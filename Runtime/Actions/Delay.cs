namespace Flexy.Core.Actions;

[Serializable]
public class Delay : FlexyActionAsync
{
	[SerializeField]	Single	_seconds;
	[SerializeField]	Boolean	_unscaledTime;
	
	public override		UniTask DoAsync	( ActionCtx ctx ) => UniTask.Delay( (Int32)(_seconds * 1000), _unscaledTime ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime );
}