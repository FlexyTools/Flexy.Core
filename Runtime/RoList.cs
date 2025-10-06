namespace Flexy.Core;

public readonly struct RoList<T>
{
	private RoList(List<T> list) => _list = list;

	private readonly List<T> _list;

	public	T		this[Int32 index]	=> _list[index];
	public	Boolean	IsNull				=> _list == null;
	public	Int32	Count				=> _list.Count;

	public Boolean				Contains		( T item )	=> _list.Contains( item );
	public Int32				IndexOf			( T item )	=> _list.IndexOf( item );
	public List<T>.Enumerator	GetEnumerator	( )			=> _list.GetEnumerator( );
		
	public static implicit operator RoList<T>		( List<T> list ) => new( list );
}