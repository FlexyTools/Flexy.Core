namespace Flexy.Core.Extensions;

public static class Ex_GameObject
{
	public static	void	ClearEditorDirty	( this UnityEngine.GameObject go )
	{
		#if UNITY_EDITOR
		
		if (!go || go.scene.IsValid() || String.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(go)))
			return;
			
		UnityEditor.EditorUtility.ClearDirty(go);
			
		#endif
	}
}