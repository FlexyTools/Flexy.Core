namespace Flexy.Core
{
	public abstract class ServiceProvider : MonoBehaviour
	{
		public abstract void ProvideServices( GameContext ctx );
	}
}