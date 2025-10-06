namespace Flexy.Core.Services
{
	public class Setup_TaskExceptionsLogger: MonoBehaviour, IService
	{
		public void OrderedInit(GameContext ctx)
		{
			UniTaskScheduler.UnobservedTaskException += Debug.LogException;		
		}
	}
}