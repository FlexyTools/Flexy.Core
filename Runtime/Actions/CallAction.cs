namespace Flexy.Core.Actions;

[Serializable]
public class CallAction: FlexyActionSync
{
	[SerializeField]	MonoBehaviour	_target;
	[SerializeField]	String			_method;
	
	public override void Do(ActionCtx ctx)
	{
		
	}
}

[Serializable]
public class CallAsyncAction: FlexyActionAsync
{
	[SerializeField]	MonoBehaviour	_target;
	[SerializeField]	String			_method;
	
	public override UniTask DoAsync(ActionCtx ctx)
	{
		return UniTask.CompletedTask;
	}
}

[Serializable]
public class BindAction: FlexyActionSync
{
	[SerializeField]	MonoBehaviour	_bindSource;
	[SerializeField]	String			_bindPath;
	
	public override void Do(ActionCtx ctx)
	{
		
	}
}