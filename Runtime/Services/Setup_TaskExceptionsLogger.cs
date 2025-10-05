namespace Flexy.Core.GameContexts
{
	public class Setup_TaskExceptionsLogger: MonoBehaviour, IService
	{
		public void OrderedInit(GameContext ctx)
		{
			UniTaskScheduler.UnobservedTaskException += ex => Debug.LogException(ex);		
		}
	}
}