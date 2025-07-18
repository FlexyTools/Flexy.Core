using UnityEditor;

namespace Flexy.Core.Editor;

public static class HighlightObjectUtil
{
	[MenuItem("CONTEXT/Object/Ping!")]
	static void Ping(MenuCommand command)
	{
		EditorGUIUtility.PingObject(command.context);
	}
}