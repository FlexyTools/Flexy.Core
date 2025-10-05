using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[InitializeOnLoad]
	public static class UnityEditorTopToolbar
	{
		static UnityEditorTopToolbar( )
		{
			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;
		}

		public static void AddIMGUIContainerToLeftPocket	( String name, Action onGUI, EPlace place ) => AddVisualElementToLeftPocket		( name, new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1, flexDirection = FlexDirection.Row}}, place );
		public static void AddIMGUIContainerToRightPocket	( String name, Action onGUI, EPlace place ) => AddVisualElementToRightPocket	( name, new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1, flexDirection = FlexDirection.Row}}, place );
		public static void AddVisualElementToLeftPocket		( String name, VisualElement e, EPlace place )
		{
			e.name = name;
			( place switch 
			{
				EPlace.Left		=> LeftPocket_Left,
				EPlace.Center	=> LeftPocket_Center,
				EPlace.Right	=> LeftPocket_Right,
			} ).Add( e );
		}
		public static void AddVisualElementToRightPocket	( String name, VisualElement e, EPlace place )
		{
			e.name = name;
			( place switch 
			{
				EPlace.Left		=> RightPocket_Left,
				EPlace.Center	=> RightPocket_Center,
				EPlace.Right	=> RightPocket_Right,
			} ).Add( e );
		}

		private static readonly VisualElement LeftPocket_Left	= new( ){ name = "Left Pocket - Left",		style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement LeftPocket_Center	= new( ){ name = "Left Pocket - Center",	style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement LeftPocket_Right	= new( ){ name = "Left Pocket - Right",		style = { flexGrow = 1, flexDirection = FlexDirection.RowReverse } };
		
		private static readonly VisualElement RightPocket_Left	= new( ){ name = "Right Pocket - Left",		style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement RightPocket_Center= new( ){ name = "Right Pocket - Center",	style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement RightPocket_Right	= new( ){ name = "Right Pocket - Right",	style = { flexGrow = 1, flexDirection = FlexDirection.RowReverse } };

		private static ScriptableObject _editorToolbarPanel;		
		
		private static void ImguiUI		( Action onGUI )	
		{
			GUILayout.BeginHorizontal( );
			try						{ onGUI( ); }
			catch (Exception ex)	{ Debug.LogException( ex ); }
			GUILayout.EndHorizontal( );
		}
		private static void EditorUpdate( )					
		{
			if ( _editorToolbarPanel != null ) 
				return;
			
			var toolbars		= Resources.FindObjectsOfTypeAll( typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.Toolbar" ) );
			_editorToolbarPanel	= toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;

			if ( _editorToolbarPanel == null ) 
				return;
			
			var root		= _editorToolbarPanel.GetType( ).GetField( "m_Root", BindingFlags.NonPublic | BindingFlags.Instance );
			var rootElement	= root.GetValue( _editorToolbarPanel ) as VisualElement;
			
			var leftToolbar = rootElement.Q( "ToolbarZoneLeftAlign" );
			leftToolbar.Add( LeftPocket_Left );
			leftToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			leftToolbar.Add( LeftPocket_Center );
			leftToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			leftToolbar.Add( LeftPocket_Right );
            
			var rightToolbar = rootElement.Q( "ToolbarZoneRightAlign" );
			
			rightToolbar.Add( RightPocket_Right );
			rightToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			rightToolbar.Add( RightPocket_Center );
			rightToolbar.Add( new( ){ name = "FlexibleSpace", style = { flexGrow = 999, flexDirection = FlexDirection.Row } } );
			rightToolbar.Add( RightPocket_Left );
		}
		
		public enum EPlace
		{
			Left, 
			Center,
			Right
		}
		
				
		public class Preferences : SettingsProvider
		{
			[SettingsProvider]
			public static SettingsProvider CreateProvider() => new Preferences("Preferences/Flexy/Core/Unity Toolbar", SettingsScope.User);

			private Preferences(String path, SettingsScope scope) : base(path, scope) { }

			private static IEnumerable<VisualElement> GetPredefinedContainers()
			{
				yield return LeftPocket_Left;
				yield return LeftPocket_Center;
				yield return LeftPocket_Right;
				
				yield return RightPocket_Left;
				yield return RightPocket_Center;
				yield return RightPocket_Right;
			} 

			public override void OnActivate(String searchContext, VisualElement root)
			{
				var label = new Label("Unity Toolbar"){ style = { paddingLeft = 10, paddingTop = 6, marginBottom = 10, fontSize = 19, unityFontStyleAndWeight = FontStyle.Bold}};
				root.Add(label);
			
				var scroll = new ScrollView { style = { paddingLeft = 10, paddingTop = 6 } };
				root.Add(scroll);

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
					scroll.Add(groupBox);
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
			

			private List<ElementData> LoadGroup(String groupName, VisualElement container)
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

			private void SaveGroup(VisualElement container)
			{
				var prefKey = $"Flexy/Core/Unity Toolbar_{container.name}";
				var currentList = container.Children().Select(ve => new ElementData { Name = ve.name, Enabled = ve.visible }).ToList();

				var saveObj = new SavedElements { List = currentList };
				var newJson = JsonUtility.ToJson(saveObj);
				EditorPrefs.SetString(prefKey, newJson);
			}

			[Serializable]
			private class SavedElements 
			{
				public List<ElementData> List; 
			}
			
			[Serializable]
			private class ElementData
			{
				public String Name;
				public Boolean Enabled = true;
				[NonSerialized]
				public VisualElement Element;
			}
		}
	}
}