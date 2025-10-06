namespace Flexy.Core.Binding
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CallableAttribute : UnityEngine.Scripting.PreserveAttribute
	{
		public		CallableAttribute			( ) { ArgumentType = typeof(void); }
		public		CallableAttribute			( Type argumentType ) { ArgumentType = argumentType; }
		
		public		Type	ArgumentType {get; init;}
	}
}