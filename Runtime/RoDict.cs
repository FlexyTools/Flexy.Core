using System.Collections;

namespace Flexy.Core
{
	public struct RoDict<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private RoDict(IDictionary<TKey, TValue> dict) => _dict = dict;

		private readonly IDictionary<TKey, TValue> _dict;

		public	Int32				Count		=> _dict.Count;
		public	Boolean				IsReadOnly	=> _dict.IsReadOnly;
		
		public	IEnumerable<TKey>	Keys		=> _dict.Keys;
		public	IEnumerable<TValue>	Values		=> _dict.Values;
		
		public	TValue				this [ TKey key ]
		{
			get => _dict[key];
			set => throw new NotImplementedException( );
		}
		
		public	Boolean		Contains		( KeyValuePair<TKey, TValue> item )	=> _dict.Contains( item );
		public	Boolean		ContainsKey		( TKey key )						=> _dict.ContainsKey( key );
		public	Boolean		TryGetValue		( TKey key, out TValue value )		=> _dict.TryGetValue( key, out value );
		
		public		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ( )	=> _dict.GetEnumerator( );
		IEnumerator	IEnumerable.GetEnumerator ( )								=> GetEnumerator( );
		
		void	CopyTo	( KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex )	=> _dict.CopyTo( array, arrayIndex );
		
		public static implicit operator RoDict<TKey, TValue>( Dictionary<TKey, TValue> dict ) => new( dict );
	}
}