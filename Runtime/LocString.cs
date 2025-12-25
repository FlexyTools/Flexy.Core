using System.Linq;

namespace Flexy.Core;

[Serializable]
public struct LocString
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 
	static void	StaticClear ( )	
	{
		LocService = null;	
	}
	
	public static IService_LocString? LocService;
	
	[SerializeField] String _key;

	public static implicit operator String		( LocString str )	=> str.ToString();
 	public static explicit operator LocString	( String str )		=> new (){_key = str};
  
	public override String ToString	( ) => LocService == null ? _key : LocService.LocalizeKey(_key);
}

public interface IService_LocString
{
	public String[]		AvailableLocalizations	{get;}
	public String		LocalizeKey				( String key );
	
	protected internal void		AddKey					( String key );
	protected internal String	GetLocalizedString		( String key, String locale );
	protected internal void		SetLocalizedString		( String key, String locale, String value );
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(LocString))]
public class LocStringDrawer : UnityEditor.PropertyDrawer
{
	protected static Int32 _selectedLoc;

	private String[] _cachedLocalisations = null!;
	private String[] _localisationOptions = null!;

	public override void OnGUI( Rect position, UnityEditor.SerializedProperty property, GUIContent label )
	{
		UnityEditor.EditorGUI.BeginProperty(position, label, property);
		position = UnityEditor.EditorGUI.PrefixLabel( position, label );
		
		if (LocString.LocService != null && _cachedLocalisations != LocString.LocService.AvailableLocalizations)
		{
			_cachedLocalisations = LocString.LocService.AvailableLocalizations;
			_localisationOptions = _cachedLocalisations.Prepend("k").ToArray();
		}
		
		var locStr = "en";
		
		if (LocString.LocService != null)
		{
			if (_selectedLoc >= _localisationOptions.Length)
				_selectedLoc = _localisationOptions.Length;
		
			locStr = _cachedLocalisations[_selectedLoc];
		}
		
		var prefixPos = position;
		var offset = locStr.Length * 10;
		prefixPos.x -= offset;
		prefixPos.width = offset;
		
		GUI.Label( prefixPos, locStr );
		
		if (LocString.LocService != null)
		{
			var popupPos	= position;
			popupPos.xMin	= popupPos.xMax-20;
			
			_selectedLoc = UnityEditor.EditorGUI.Popup( popupPos, _selectedLoc, _localisationOptions );
			position.xMax -= 20;
		}
		
		var keyProp = property.FindPropertyRelative( "_key" );
		
		if (LocString.LocService == null || _selectedLoc == 0)
		{
			var key		= keyProp.stringValue;
			var newKey	= UnityEditor.EditorGUI.DelayedTextField(position, key);
			keyProp.stringValue = newKey;
			
			if (LocString.LocService != null)
			{
				var locString = LocString.LocService.GetLocalizedString(newKey, String.Empty);
				if (locString == null)
				{
					// There is no such key yet
					GUILayout.BeginHorizontal();
					GUILayout.Label("there is no such key");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Add key"))
						LocString.LocService.AddKey(newKey);
					GUILayout.EndHorizontal();		
				}
			}
		}
		else
		{
			var key				= keyProp.stringValue;
			var localisedString	= LocString.LocService.GetLocalizedString(key, _localisationOptions[_selectedLoc]);
			
			if (localisedString.StartsWith("#k#"))
			{
				// It is key returned because we have en translation stored instead of key
				key = localisedString.Substring(3);
				keyProp.stringValue = key;
			}
			
			UnityEditor.EditorGUI.BeginChangeCheck();
			var newValue = UnityEditor.EditorGUI.DelayedTextField(position, LocString.LocService.GetLocalizedString(key, _localisationOptions[_selectedLoc]));
			
			if (UnityEditor.EditorGUI.EndChangeCheck())
				LocString.LocService.SetLocalizedString(key, _localisationOptions[_selectedLoc], newValue);
		}
		
		UnityEditor.EditorGUI.EndProperty();
	}
}
#endif