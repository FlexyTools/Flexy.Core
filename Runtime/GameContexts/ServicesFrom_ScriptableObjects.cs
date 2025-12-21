namespace Flexy.Core.GameContexts
{
	public class ServicesFrom_ScriptableObjects : ServiceProvider
	{
		[SerializeField] ScriptableObject[] Services = null!;
		
		public override void ProvideServices( GameContext ctx )
		{
			foreach ( var serv in Services )
				ctx.SetService( serv );
		}
	}
}