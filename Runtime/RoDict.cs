namespace Flexy.Core;

public readonly struct RoDict<TKey, TValue>
{
	private RoDict(Dictionary<TKey, TValue> dict) => _dict = dict;

	private readonly Dictionary<TKey, TValue> _dict;

	public	TValue				this[TKey key]	=> _dict[key];
	public	Boolean				IsNull			=> _dict == null;
	public	Int32				Count			=> _dict.Count;
	public	IEnumerable<TKey>	Keys			=> _dict.Keys;
	public	IEnumerable<TValue>	Values			=> _dict.Values;
		
	public	Boolean		ContainsKey		( TKey key )					=> _dict.ContainsKey( key );
	public	Boolean		TryGetValue		( TKey key, out TValue value )	=> _dict.TryGetValue( key, out value );
	public	Dictionary<TKey, TValue>.Enumerator GetEnumerator ( )		=> _dict.GetEnumerator( );
		
	public static implicit operator RoDict<TKey, TValue>( Dictionary<TKey, TValue> dict ) => new( dict );
}