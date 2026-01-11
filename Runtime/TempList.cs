using System.Buffers;
using System.Runtime.CompilerServices;

namespace Flexy.Core;

public struct TempList<T> : IDisposable
{
	public static TempList<T> Rent( Int32 minCapacity )	
	{
		return new TempList<T>{ _array = ArrayPool<T>.Shared.Rent( minCapacity ) };
	}
	public static TempList<T> Rent( IList<T> initial )	
	{
		var list = new TempList<T>{ _array = ArrayPool<T>.Shared.Rent( initial.Count ) };
			
		list.AddRange( initial );
			
		return list;
	}
	public static TempList<T> Rent( Int32 minCapacity, IEnumerable<T> initial )	
	{
		var list = new TempList<T>{ _array = ArrayPool<T>.Shared.Rent( minCapacity ) };
			
		list.AddRange( initial );
			
		return list;
	}
		
	private T[]		_array;
	private Int32	_count;

	public Int32 Count => _count;

	public T this[Int32 index]
	{
		get
		{
			if (index >= _count)
				throw new ArgumentOutOfRangeException( nameof(index) );
				
			return  _array[index];
		}
		set
		{
			if (index >= _count)
				throw new ArgumentOutOfRangeException( nameof(index) );
			
			_array[index] = value;
		}
	}
		
	public void Dispose	( )
	{
		if( _array != null )
		{
			for (var i = 0; i < _array.Length; i++)
				_array[i] = default!;
				
			ArrayPool<T>.Shared.Return( _array );
		}
					
		_array = null!;
	}
			
	public void Add		( T item )	
	{
		if( _array.Length <= _count ) //array fully filled
		{
			var biggerArray = ArrayPool<T>.Shared.Rent( _count * 15 / 10 );
			_array.CopyTo( biggerArray, 0 );
			ArrayPool<T>.Shared.Return( _array );
			_array = biggerArray;
		}
		
		var index = _count;
		_count++;
		_array[index] = item;
	}
	public void AddRange( IEnumerable<T> items )
	{
		foreach ( var item in items )
			Add( item );
	}
		
	[MethodImpl(256)] public Span<T>AsSpan( ) => _array.AsSpan( .._count );
	[MethodImpl(256)] public Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();
}