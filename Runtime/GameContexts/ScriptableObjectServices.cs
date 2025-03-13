namespace Flexy.Core
{
	public class ScriptableObjectServices : ServiceProvider
	{
		[SerializeField] ScriptableObject[] Services;
		
		public override void ProvideServices( GameContext ctx )
		{
			foreach ( var serv in Services )
				ctx.SetService( serv );
		}
	}
}