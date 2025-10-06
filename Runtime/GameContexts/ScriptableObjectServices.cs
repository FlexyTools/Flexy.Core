namespace Flexy.Core.GameContexts
{
	public class ScriptableObjectServices : ServiceProvider
	{
		[SerializeField] ScriptableObject[] Services = null!;
		
		public override void ProvideServices( GameContext ctx )
		{
			foreach ( var serv in Services )
				ctx.SetService( serv );
		}
	}
}