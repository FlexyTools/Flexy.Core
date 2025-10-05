namespace Flexy.Core;

public static class GameObjectExtensions
{
	public static	void	ClearEditorDirty	( this GameObject go )
	{
		#if UNITY_EDITOR
		
		if (!go || go.scene.IsValid() || String.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(go)))
			return;
			
		UnityEditor.EditorUtility.ClearDirty(go);
			
		#endif
	}
}