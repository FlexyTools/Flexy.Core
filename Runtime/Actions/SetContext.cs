	namespace Flexy.Core.Actions;

[Serializable]
public class SetContext : FlexyActionSync
{
	[SerializeField]	Component	_newContext = null!;
	
	public override void Do(ActionCtx ctx)
	{
		ctx.CtxObj = _newContext;
	}
}