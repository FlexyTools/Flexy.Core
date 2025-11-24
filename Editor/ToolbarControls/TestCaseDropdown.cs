using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class TestCaseDropdown
{
#if UNITY_6000_3_OR_NEWER
	[MainToolbarElement("Flexy/Test Cases", defaultDockPosition = MainToolbarDockPosition.Middle)]
	public static MainToolbarElement CreateToolbarElement()
	{
		var type = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element = (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
#endif

	static TestCaseDropdown( )
	{
		#if !UNITY_6000_3_OR_NEWER
		UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Test Case", OnTestRunGUI, UnityEditorTopToolbar.EPlace.Left );
		#endif
		//EditorSceneManager.sceneClosed		+= s		=> EditorPrefs.SetString( Test_Selected, null );
		EditorSceneManager.sceneOpened		+= (_, _)	=> PlayerPrefs.SetString( Test_Selected, null );
	}
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void CleanUpRun( ) => IsTestLaunched_InThisSession = default;

	private const String	Test_Selected	= "Flexy.Core.TestCase: Selected";
	public static Boolean	IsTestLaunched_InThisSession;
	
	private static List<TestRunSource> TestRunSources = new();
	
	public static void		AddTestProvider			( String providerName, Func<IEnumerable<String>> testsCollectionProvider )
	{
		TestRunSources.Add(new(providerName, testsCollectionProvider));	
	}
	public static Boolean	TryGetTestCaseToLaunch	( String providerName, out String testCase )	
	{
		testCase = null!;
		if (IsTestLaunched_InThisSession)
			return false;
		
		var testSelected = PlayerPrefs.GetString(Test_Selected); 
		
		if (testSelected.StartsWith(providerName + ": "))
		{
			IsTestLaunched_InThisSession = true;
			testCase = testSelected[(providerName.Length+2)..]; 
			return true;
		}
		
		return false;
	}
	private static void		OnTestRunGUI			( )	
	{
		var style = EditorStyles.label;
		style.richText = true;
		
		var testSelected = PlayerPrefs.GetString(Test_Selected);
        
		if (String.IsNullOrWhiteSpace(testSelected))
			testSelected = "None";
		
		var selectedNiceName = ObjectNames.NicifyVariableName( testSelected );
		
		if (EditorGUILayout.DropdownButton(new( $"{selectedNiceName}" ), FocusType.Passive, EditorStyles.toolbarPopup))
		{
			var menu = new GenericMenu();
			
			menu.AddItem(new("None"), false, SetTestRunName, null);
			
			foreach (var testRunSource in TestRunSources)
			{
				try
				{
					var testRuns = testRunSource.GetTestRuns().ToArray();
				
					if (!testRuns.Any())
						continue;
					
					menu.AddSeparator("");

					menu.AddItem( new( $"- {testRunSource.Name} -" ), false, null );

					foreach (var testRun in testRuns)
					{
						menu.AddItem( new( $" {ObjectNames.NicifyVariableName(testRun)} " ), false, SetTestRunName, (testRunSource.Name, testRun) );
					}
				}
				catch (Exception ex) { Debug.LogException(ex); }
			}
			
			menu.ShowAsContext();
		}
		
		static void		SetTestRunName	( Object userdata )
		{
			if (userdata == null)
			{
				PlayerPrefs.SetString( Test_Selected, null );
			}
			else
			{
				var pair = ((String prefix, String testName))userdata;
				PlayerPrefs.SetString( Test_Selected, $"{pair.prefix}: {pair.testName}" );
			}
		}
	}

	private static VisualElement Creator( ) => new IMGUIContainer(OnTestRunGUI){style = { marginLeft = 10, marginRight = 10}};

	private record struct TestRunSource ( String Name, Func<IEnumerable<String>> GetTestRuns );
}