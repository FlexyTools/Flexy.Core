using System.Globalization;
using System.Threading;

namespace Flexy.Core
{
	public class SetupInvariantCultureService : MonoBehaviour, IService
	{
		public void OrderedInit( GameContext ctx )
		{
			CultureInfo.DefaultThreadCurrentCulture		= CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture	= CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture			= CultureInfo.InvariantCulture;
		}
	}
}