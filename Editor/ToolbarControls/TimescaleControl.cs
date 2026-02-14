#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif

namespace Flexy.Core.Editor.ToolbarControls;

[InitializeOnLoad]
public static class TimescaleControl
{
	#if UNITY_6000_3_OR_NEWER
	[MainToolbarElement("Flexy/Time Scale", defaultDockPosition = MainToolbarDockPosition.Middle)]
	public static MainToolbarElement CreateToolbarElement	( )	
	{
		var type = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element = (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
	
	private static VisualElement	Creator			( )		
	{
		var element	= new VisualElement {style = {flexDirection = FlexDirection.Row}};
		var slider	= new TimescaleSlider();
		var button	= new EditorToolbarButton("R", () => Time.timeScale = 1);
		
		element.Add(slider);
		element.Add(button);
		
		return element;
	}

	private class TimescaleSlider : EditorToolbarSlider
	{
		public TimescaleSlider(): base("Time Scale", default, -3, 2) 
		{
			_label = (Label)this[0][1][3];
			schedule.Execute( () => SetValueWithoutNotify((Single)Math.Log10(Time.timeScale)) ).Every(500); 
		}

		private readonly Label _label;

		public override void SetValueWithoutNotify( Single newval )	
		{
			base.SetValueWithoutNotify(newval);
			
			var ts = Time.timeScale = (Single)Math.Pow(10, newval);
				
			switch (ts)
			{
				case >= 10:		_label.text = $"x{Math.Round(ts):F0}";			break;
				case >= 3:		_label.text = $"x{Math.Round(ts*10)/10f:F1}";	break;
				case > 0.01f:	_label.text = $"x{Time.timeScale:F2}";			break;
				default:		_label.text = $"x{Time.timeScale:F3}";			break;
			}
		}
	}
	#else
	static TimescaleControl( ) { UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( "Time Scale", OnTestRunGUI, UnityEditorTopToolbar.EPlace.Center ); }
	private static void				OnTestRunGUI	( )		
	{
		GUILayout.BeginHorizontal( GUILayout.MaxWidth(300), GUILayout.Height(14) );
		{
			GUILayout.Label( "Time Scale:" );
			
			var x = (Single)Math.Log10(Time.timeScale); 
			
			var newval = GUILayout.HorizontalSlider( x, -3, 2, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true) );
			
			if( !Mathf.Approximately(newval, x) )
				Time.timeScale = (Single)Math.Pow(10, newval); 
			
			var ts = Time.timeScale;
			
			if( ts >= 10 )
			{
				ts = (Single)Math.Round(ts);
				GUILayout.Label( $"x{ts:F0}", GUILayout.Width(45) );
			}
			else if( ts >= 3 )
			{
				ts = (Single)Math.Round(ts*10)/10f;
				GUILayout.Label( $"x{ts:F1}", GUILayout.Width(45) );
			}
			else if( ts > 0.01 )
			{
				GUILayout.Label( $"x{Time.timeScale:F2}", GUILayout.Width(45) );
			}
			else
			{
				GUILayout.Label( $"x{Time.timeScale:F3}", GUILayout.Width(45) );
			}
			
			if( GUILayout.Button( "R", EditorStyles.toolbarButton, GUILayout.Height(14), GUILayout.Width(20) ) )
			{
				Time.timeScale = 1;
			}
		}
		GUILayout.EndHorizontal( );
	}
	#endif
}