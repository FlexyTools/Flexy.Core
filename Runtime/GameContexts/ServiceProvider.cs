namespace Flexy.Core.GameContexts
{
	public abstract class ServiceProvider : MonoBehaviour, IService
	{
		public void OrderedInit(GameContext ctx) { }
		public abstract void ProvideServices( GameContext ctx );
	}
}