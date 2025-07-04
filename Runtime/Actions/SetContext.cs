	namespace Flexy.Core.Actions;

[Serializable]
public class SetContext : FlexyActionSync
{
	[SerializeField]	Component	_newContext;
	
	public override void Do(ActionCtx ctx)
	{
		ctx.CtxObj = _newContext;
	}
}