using System.Collections;
using System.Runtime.CompilerServices;

namespace Flexy.Core
{
	public struct RoList<T> : IReadOnlyList<T>
	{
		private RoList(IList<T> list) => _list = list;

		private readonly IList<T> _list;

		public T		this [ Int32 index ]	
		{
			[MethodImpl(256)]
			get => _list[index];
			set => throw new InvalidOperationException( );
		}
		public Boolean	IsNull		{ [MethodImpl(256)]		get => _list == null;	}
		public Int32	Count		{ [MethodImpl(256)]		get => _list.Count;		}
		public Boolean	IsReadOnly	{ [MethodImpl(256)]		get => true;			}
		
		[MethodImpl(256)] public		Boolean			Contains		( T item )	=> _list.Contains( item );
		[MethodImpl(256)] public		Int32			IndexOf			( T item )	=> _list.IndexOf( item );
		[MethodImpl(256)] public		IEnumerator<T>	GetEnumerator	( )			=> _list.GetEnumerator( );
		[MethodImpl(256)] IEnumerator	IEnumerable.	GetEnumerator	( )			=> _list.GetEnumerator( );
		
		[MethodImpl(256)] void							CopyTo			( T[] array, Int32 arrayIndex )	=> _list.CopyTo(array,arrayIndex);

		public static implicit operator RoList<T>		( T[] list )	=> new( list );
		public static implicit operator RoList<T>		( List<T> list )=> new( list );

		// [MethodImpl(256)] void			IList<T>.		Insert			( Int32 index, T item )		=> throw new InvalidOperationException( );
		// [MethodImpl(256)] void			IList<T>.		RemoveAt		( Int32 index )				=> throw new InvalidOperationException( );
		// [MethodImpl(256)] void			ICollection<T>.	Add				( T item )					=> throw new InvalidOperationException( );
		// [MethodImpl(256)] void			ICollection<T>.	Clear			( )							=> throw new InvalidOperationException( );
		// [MethodImpl(256)] Boolean		ICollection<T>.	Remove			( T item )					=> throw new InvalidOperationException( );
	}
}