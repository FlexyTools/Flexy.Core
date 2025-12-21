namespace Flexy.Core.GameContexts
{
	public class ServicesFrom_MonoBehaviours : ServiceProvider
	{
		[SerializeField] MonoBehaviour[] _services = null!;
			
		public override void ProvideServices( GameContext ctx )
		{
			foreach ( var serv in _services )
				ctx.SetService( serv );
		}
	}
}