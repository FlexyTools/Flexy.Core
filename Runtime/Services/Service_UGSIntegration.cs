#if UNITY_SERVICES_CORE

using Unity.Services.Core;

namespace Flexy.Core
{
    public class Service_UGSIntegration : MonoBehaviour, IServiceAsync
    {
		public async UniTask OrderedInitAsync( GameContext ctx )
		{
			try
			{
				var options = new InitializationOptions();
				await UnityServices.InitializeAsync(options).AsUniTask(  );
                
				Debug.Log( $"[UGS] - Init Success" );
			}
			catch (Exception ex) 
			{
				Debug.LogError( $"[UGS] - Init Error {ex}" );
			}
		}
	}
}

#endif