#if UNITY_6000_3_OR_NEWER

using System;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor.ToolbarControls;

public class EmptySpaces
{
	[MainToolbarElement("Flexy/EmptySpaces/01", defaultDockPosition = MainToolbarDockPosition.Left, menuPriority = 500)]
	public static MainToolbarElement CreateToolbarElement_01()
	{
		var type	= typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element	= (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
		
	[MainToolbarElement("Flexy/EmptySpaces/02", defaultDockPosition = MainToolbarDockPosition.Left, menuPriority = 500)]
	public static MainToolbarElement CreateToolbarElement_02()
	{
		var type	= typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element	= (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
		
	[MainToolbarElement("Flexy/EmptySpaces/03", defaultDockPosition = MainToolbarDockPosition.Left, menuPriority = 500)]
	public static MainToolbarElement CreateToolbarElement_03()
	{
		var type	= typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element	= (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
		
	[MainToolbarElement("Flexy/EmptySpaces/04", defaultDockPosition = MainToolbarDockPosition.Left, menuPriority = 500)]
	public static MainToolbarElement CreateToolbarElement_04()
	{
		var type	= typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom", true);
		var element	= (MainToolbarElement)Activator.CreateInstance(type, (Func<VisualElement>)Creator);
			
		return element;
	}
		
	private static VisualElement Creator ( ) => new (){style = { width = 120 }};
}
#endif