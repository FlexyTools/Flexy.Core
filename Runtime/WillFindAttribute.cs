namespace Flexy.Core
{
	[AttributeUsage(AttributeTargets.Field)]
	public class WillFindAttribute: PropertyAttribute
	{
		public WillFindAttribute ( String text = "Will Find In Parents" )
		{
			Text = text;
		}

		public String Text;
	}
}