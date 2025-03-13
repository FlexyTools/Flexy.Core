namespace Flexy.Core
{
	public class MonoBehServices : ServiceProvider
	{
		[SerializeField] MonoBehaviour[] _services;
			
		public override void ProvideServices( GameContext ctx )
		{
			foreach ( var serv in _services )
				ctx.SetService( serv );
		}
	}
}