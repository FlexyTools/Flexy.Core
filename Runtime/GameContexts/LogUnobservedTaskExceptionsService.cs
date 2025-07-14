namespace Flexy.Core.GameContexts
{
	public class LogUnobservedTaskExceptionsService: MonoBehaviour, IService
	{
		public void OrderedInit(GameContext ctx)
		{
			UniTaskScheduler.UnobservedTaskException += ex => Debug.LogException(ex);		
		}
	}
}