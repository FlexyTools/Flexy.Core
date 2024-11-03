using Flexy.Serialisation;

namespace Flexy.Core;

[Serializable]
public struct LocString : ISerializeAsString
{
	public static Func<String, String> LocalizeFunc;
	//public static String[] Localisations = new []{"key","en", "ru", "ua"};
	public static String[] Localisations = new []{"en"};
	
	[SerializeField] String _key;

	public static implicit operator String( LocString str ) => str.ToString( );
 	public static explicit operator LocString( String str ) => new (){ _key = str };
  
	public override String ToString( ) => LocalizeFunc == null ? _key : LocalizeFunc?.Invoke( _key );
	
	String	ISerializeAsString.ToString		( )				=> "'"+_key;
	void	ISerializeAsString.FromString	( String data )	=>_key = data;
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(LocString))]
public class LocStringDrawer : UnityEditor.PropertyDrawer
{
	private static Int32 _selectedLocIndex;
	
	public override void OnGUI( Rect position, UnityEditor.SerializedProperty property, GUIContent label )
	{
		position = UnityEditor.EditorGUI.PrefixLabel( position, label );
		
		var locStr = LocString.Localisations[_selectedLocIndex];
		
		var prefixPos = position;
		var offset = locStr.Length * 10;
		prefixPos.x -= offset;
		prefixPos.width = offset;
		
		GUI.Label( prefixPos, locStr );
		
		var popupPos	= position;
		popupPos.xMin	= popupPos.xMax-20;
		
		_selectedLocIndex = UnityEditor.EditorGUI.Popup( popupPos, _selectedLocIndex, LocString.Localisations );
		
		position.xMax -= 20;
		var prop = property.FindPropertyRelative( "_key" );
		UnityEditor.EditorGUI.PropertyField( position, prop, GUIContent.none );
	}
}
#endif
