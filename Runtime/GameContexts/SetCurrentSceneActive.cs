using UnityEngine.SceneManagement;

namespace Flexy.Core.GameContexts
{
	[DefaultExecutionOrder(Int16.MinValue+150)]
	public class SetCurrentSceneActive : MonoBehaviour
	{
		private void Start()
		{
			SceneManager.SetActiveScene( gameObject.scene );
		}
	}
}