#if UNITY_6000_6_OR_NEWER
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
#endif

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

		#if UNITY_6000_6_OR_NEWER
		HierarchyWindow.BindViewItem -= OnBindHierarchyViewItem;
		HierarchyWindow.BindViewItem += OnBindHierarchyViewItem;
		HierarchyWindow.UnbindViewItem -= OnUnbindHierarchyViewItem;
		HierarchyWindow.UnbindViewItem += OnUnbindHierarchyViewItem;
		#endif
	}
	
	private static GUIStyle? _rightAlignedStyle;

	#if UNITY_6000_6_OR_NEWER

	private static	void			OnBindHierarchyViewItem		( HierarchyWindow window, HierarchyView view, HierarchyViewItem item )	
	{
		if (item.Handler is not HierarchySceneHandler sceneHandler || item.Node == HierarchyNode.Null)
			return;

		var label = item.RightCustomContainer.Q<Button>("flexy-hierarchy-context");
		if (label == null)
		{
			label = new Button
			{
				name = "flexy-hierarchy-context",
				style =
				{
					flexShrink		= 1, minWidth		= 0,	maxWidth	= 160,	minHeight		= 0, 
					marginLeft		= 8, marginRight	= 10,	marginTop	= 0,	marginBottom	= 0,
					paddingLeft		= 0, paddingRight	= 0,	paddingTop	= 0,	paddingBottom	= 0,
					borderLeftWidth	= 0, borderRightWidth = 0,	borderTopWidth = 0,	borderBottomWidth = 0,
					backgroundColor	= Color.clear,			color		= new Color(0.6f, 0.6f, 0.6f),
					fontSize		= 11,
					unityTextAlign	= TextAnchor.MiddleRight,
					textOverflow	= TextOverflow.Ellipsis,
					overflow		= Overflow.Hidden,
					whiteSpace		= WhiteSpace.NoWrap,
					display			= DisplayStyle.None
				}
			};
			var contextLabel = label;
			label.RegisterCallback<ClickEvent>(evt => 
			{
				if (evt.currentTarget is Button { userData: Scene scene })
				{
					var context = GetSceneContext(scene);
					if (context != null)
						EditorGUIUtility.PingObject(context);
				}
			});
			label.schedule.Execute(() => UpdateContextLabel(contextLabel)).Every(1000);
			item.RightCustomContainer.Add(label);
		}

		label.userData = sceneHandler.GetScene(item.Node);
		UpdateContextLabel(label);
	}
	private static	void			OnUnbindHierarchyViewItem	( HierarchyWindow window, HierarchyView view, HierarchyViewItem item )	
	{
		var label = item.RightCustomContainer.Q<Button>("flexy-hierarchy-context");
		if (label == null)
			return;

		label.userData = null;
		UpdateContextLabel(label);
	}
	private static	void			UpdateContextLabel			( Button label )														
	{
		var context = label.userData is Scene scene ? GetSceneContext(scene) : null;
		label.style.display = context == null ? DisplayStyle.None : DisplayStyle.Flex;
		label.text = context == null ? String.Empty : context.name;
		label.tooltip = label.text;
	}
	private static	GameContext?	GetSceneContext				( Scene scene ) => EditorApplication.isPlaying && GameContext.IsGlobalAlive && scene.IsValid() ? GameContext.GetCtx(scene) : null;

	#endif
	
	#if UNITY_6000_5_OR_NEWER
	
	private static	void			OnEntityHierarchyGUI		( EntityId entityId, Rect selectionRect )	
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
	
	
	private static	void			OnHierarchyGUI				( Int32 instanceID, Rect selectionRect )	
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
