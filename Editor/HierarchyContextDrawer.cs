namespace Flexy.Core.Editor;

[InitializeOnLoad]
public static class HierarchyContextDrawer
{
	static HierarchyContextDrawer()
	{
		#if UNITY_6000_5_OR_NEWER
		EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnEntityHierarchyGUI;
		EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnEntityHierarchyGUI;
		#else
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
		#endif
	}
	
	private static GUIStyle? _rightAlignedStyle;
	
	#if UNITY_6000_5_OR_NEWER
	
	private static void OnEntityHierarchyGUI( EntityId entityId, Rect selectionRect )
	{
		if (!EditorApplication.isPlaying || !GameContext.IsGlobalAlive)
			return;
	
		Scene scene = default;
		var isSceneRow = false;

		var instance = EditorUtility.EntityIdToObject(entityId);

		if (instance is not null)
			return;

		var globalScene = GameContexts.GameContext.Global.gameObject.scene;

		if (globalScene.handle == SceneHandle.FromRawData(EntityId.ToULong(entityId)))
		{
			scene = globalScene;
			isSceneRow = true;
		}
		else
		{
			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var s = SceneManager.GetSceneAt(i);
				// The instanceID for the Scene header in Hierarchy matches the Scene's handle
				if (s.handle == SceneHandle.FromRawData(EntityId.ToULong(entityId)))
				{
					scene = s;
					isSceneRow = true;
					break;
				}
			}
		}

		if (!isSceneRow)
			return;

		var ctx = GameContexts.GameContext.GetCtx(scene);

		var labelRect = selectionRect;
		labelRect.width -= 10;
		labelRect.xMin = labelRect.xMax - 160;  
		
		if (_rightAlignedStyle == null)
		{
			_rightAlignedStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
			};
		}
		
		if (GUI.Button(labelRect, ctx.name, _rightAlignedStyle))
			EditorGUIUtility.PingObject(ctx);
	}
	
	#else
	
	
	private static void OnHierarchyGUI( Int32 instanceID, Rect selectionRect )
	{
		if (!EditorApplication.isPlaying || !GameContexts.GameContext.IsGlobalAlive)
			return;
	
		Scene scene = default;
		var isSceneRow = false;

		var instance = EditorUtility.InstanceIDToObject(instanceID);

		if (instance is not null)
			return;

		var globalScene = GameContexts.GameContext.Global.gameObject.scene;

		if (globalScene.GetHashCode() == instanceID || globalScene.handle == instanceID)
		{
			scene = globalScene;
			isSceneRow = true;
		}
		else
		{
			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var s = SceneManager.GetSceneAt(i);
				// The instanceID for the Scene header in Hierarchy matches the Scene's handle
				if (s.GetHashCode() == instanceID || s.handle == instanceID)
				{
					scene = s;
					isSceneRow = true;
					break;
				}
			}
		}

		if (!isSceneRow)
			return;

		var ctx = GameContexts.GameContext.GetCtx(scene);

		var labelRect = selectionRect;
		labelRect.width -= 10;
		labelRect.xMin = labelRect.xMax - 160;  
		
		if (_rightAlignedStyle == null)
		{
			_rightAlignedStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
			};
		}
		
		if (GUI.Button(labelRect, ctx.name, _rightAlignedStyle))
			EditorGUIUtility.PingObject(ctx);
	}
	
	#endif	
}
