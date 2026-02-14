#if FLEXY_LOG

global using Debug = Flexy.Log.Debug;

#else

using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Flexy.Core;

internal static class Debug
{
	public static				Boolean		isDebugBuild	=> UnityEngine.Debug.isDebugBuild;

	[HideInCallstack] public static	void	Log				( object log, Object? context = null, [CallerFilePath] String filePath = "", [CallerLineNumber] Int32 lineNumber = 0, [CallerMemberName] String memberName = ""  ) => UnityEngine.Debug.unityLogger.Log(LogType.Log,		(object)FormatLogString(log, filePath, lineNumber, memberName, 	context), context);
	[HideInCallstack] public static	void	LogWarning		( object log, Object? context = null, [CallerFilePath] String filePath = "", [CallerLineNumber] Int32 lineNumber = 0, [CallerMemberName] String memberName = ""  ) => UnityEngine.Debug.unityLogger.Log(LogType.Warning,	(object)FormatLogString(log, filePath, lineNumber, memberName, 	context), context);
	[HideInCallstack] public static	void	LogError		( object log, Object? context = null, [CallerFilePath] String filePath = "", [CallerLineNumber] Int32 lineNumber = 0, [CallerMemberName] String memberName = ""  ) => UnityEngine.Debug.unityLogger.Log(LogType.Error,		(object)FormatLogString(log, filePath, lineNumber, memberName, 	context), context);

	[HideInCallstack] public static	void	LogException	( Exception ex, Object? context = null ) => UnityEngine.Debug.unityLogger.LogException(ex, context);

	[HideInCallstack] static		String	FormatLogString	( object log, String filePath, Int32 lineNumber, String memberName, Object? context )	
	{
		var str = log is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : log.ToString();						
	
		if (String.IsNullOrEmpty(str))
			return String.Empty;

		return context 
			? $"[{Path.GetFileNameWithoutExtension(filePath)}] {context.name} - {memberName}: {str}    at    <color=#cccccc>line {lineNumber}</color>" 
			: $"[{Path.GetFileNameWithoutExtension(filePath)}] {memberName}: {str}    at    <color=#cccccc>line {lineNumber}</color>";
	}
}

#endif