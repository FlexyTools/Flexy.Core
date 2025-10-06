namespace Flexy.Core.GameContexts
{
	public abstract class ServiceProvider : MonoBehaviour
	{
		public abstract void ProvideServices( GameContext ctx );
	}
}