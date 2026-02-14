using Flexy.Core.Editor.Binding;

namespace Flexy.Core.Editor;

public class Preferences : SettingsProvider
{
	[SettingsProvider]
	public static SettingsProvider CreateProvider() => new Preferences("Preferences/Flexy/Core", SettingsScope.User);

	private Preferences(String path, SettingsScope scope) : base(path, scope) { }

	public override	void	OnActivate	( String searchContext, VisualElement root )	
	{
		root.Add( new Label("Flexy.Core"){ style = { marginLeft = 4, marginTop = 2.4f, marginRight = 4, marginBottom = 2.4f, paddingLeft = 2.4f, paddingRight = 2.4f, paddingBottom = 2.4f, fontSize = 19, unityFontStyleAndWeight = FontStyle.Bold}} );
		
		var scroll = new ScrollView { style = { paddingLeft = 10, paddingTop = 2 } };
		root.Add(scroll);
		
		AddBinders(searchContext, scroll);
		#if !UNITY_6000_3_OR_NEWER
		AddToolbar(searchContext, scroll);
		#endif
	}

	private			void	AddBinders	( String searchContext, VisualElement root )	
	{
		var label = new Label("Binders"){ style = { paddingTop = 6, marginBottom = 5, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold}};
		root.Add(label);

		var toggle = new Toggle { style = { flexGrow = 1 } };
		root.Add(toggle);

		toggle.text = "Allow bind to any member";
		toggle.value = EditorPrefs.GetBool(BindSourceDrawer.AllowBindToAnyMemberKey, false);
		toggle.RegisterValueChangedCallback(ev => EditorPrefs.SetBool(BindSourceDrawer.AllowBindToAnyMemberKey, ev.newValue));
		
		toggle = new Toggle { style = { flexGrow = 1 } };
		root.Add(toggle);
		
		toggle.text = "Allow bind to non public members";
		toggle.value = EditorPrefs.GetBool(BindSourceDrawer.AllowBindToNonPublicKey, false);
		toggle.RegisterValueChangedCallback(ev => EditorPrefs.SetBool(BindSourceDrawer.AllowBindToNonPublicKey, ev.newValue));
	}
	
	#if !UNITY_6000_3_OR_NEWER
	private			void	AddToolbar	( String searchContext, VisualElement root )	
	{
		var label = new Label("Unity Toolbar"){ style = { paddingTop = 6, marginBottom = 5, marginTop = 15, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold}};
		root.Add(label);
		
		foreach (var container in GetPredefinedContainers())
		{
			var groupName = container.name;
			var elements = LoadGroup(groupName, container);
			var groupBox = new Box { style = { marginBottom = 12 } };
					
			groupBox.Add(new Label(groupName) { style =
			{
				unityFontStyleAndWeight = FontStyle.Bold,
				fontSize = 13,
				marginBottom = 4
			} });

			var listView = new ListView(elements, 24, MakeItem, (element, index) => SetupItem(element, elements[index], () => SaveGroup(container)))
			{
				reorderable = true,
				showAddRemoveFooter = false,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
			};

			listView.itemsAdded += _ => SaveGroup(container);
			listView.itemsRemoved += _ => SaveGroup(container);
			listView.itemIndexChanged += (_, _) => SaveGroup(container);

			groupBox.Add(listView);
			root.Add(groupBox);
		}

		return;

		static VisualElement MakeItem()
		{
			var row = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween
				}
			};

			var toggle = new Toggle { style = { flexGrow = 1 } };
			row.Add(toggle);

			return row;
		}
				
		static void SetupItem(VisualElement e, ElementData item, Action save)
		{
			var toggle = e.Q<Toggle>();

			toggle.text = item.Name;
			toggle.value = item.Enabled;

			toggle.RegisterValueChangedCallback(ev =>
			{
				item.Enabled = ev.newValue;
				item.Element.visible = item.Enabled;
				save();
			});
		}
	}
	#endif
	
	private		List<ElementData>	LoadGroup	( String groupName, VisualElement container )	
	{
		var prefKey = $"Flexy/Core/Unity Toolbar_{groupName}";
		var json = EditorPrefs.GetString(prefKey, String.Empty);
		var elementsByName = container.Children().ToDictionary(e => e.name);

		if (!string.IsNullOrEmpty(json))
		{
			var loadedData = JsonUtility.FromJson<SavedElements>(json);
			if (loadedData is { List: not null })
			{
				foreach (var elemData in loadedData.List)
				{
					if (elementsByName.TryGetValue(elemData.Name, out var ve))
						ve.visible = elemData.Enabled;
				}
			}
		}

		var currentList = container.Children().Select(ve => new ElementData { Element = ve, Name = ve.name, Enabled = ve.visible }).ToList();

		var saveObj = new SavedElements { List = currentList };
		var newJson = JsonUtility.ToJson(saveObj);
		EditorPrefs.SetString(prefKey, newJson);
				
		return currentList;
	}
	private		void				SaveGroup	( VisualElement container )						
	{
		var prefKey = $"Flexy/Core/Unity Toolbar_{container.name}";
		var currentList = container.Children().Select(ve => new ElementData { Name = ve.name, Enabled = ve.visible }).ToList();

		var saveObj = new SavedElements { List = currentList };
		var newJson = JsonUtility.ToJson(saveObj);
		EditorPrefs.SetString(prefKey, newJson);
	}

	#if !UNITY_6000_3_OR_NEWER
	private		IEnumerable<VisualElement>	GetPredefinedContainers	( )		
	{
		yield return UnityEditorTopToolbar.LeftPocket_Left;
		yield return UnityEditorTopToolbar.LeftPocket_Center;
		yield return UnityEditorTopToolbar.LeftPocket_Right;
				
		yield return UnityEditorTopToolbar.RightPocket_Left;
		yield return UnityEditorTopToolbar.RightPocket_Center;
		yield return UnityEditorTopToolbar.RightPocket_Right;
	}
	#endif

	[Serializable]
	private class SavedElements 
	{
		public List<ElementData>? List; 
	}
			
	[Serializable]
	private class ElementData
	{
		public String Name = null!;
		public Boolean Enabled = true;
		[NonSerialized]
		public VisualElement Element = null!;
	}
}