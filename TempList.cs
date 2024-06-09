using System.Buffers;
using System.Collections;

namespace Flexy.Core
{
	public struct TempList<T> : IReadOnlyList<T>, IDisposable
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
		
		private T[]		_array;
		private Int32	_count;

		public Int32 Count => _count;

		public T this[Int32 index]
		{
			get
			{
				if ( index >= 0 && index < _count )
					return  _array[ index ];
				throw new ArgumentOutOfRangeException( nameof(index) );
			}
			set
			{
				if ( index >= 0 && index < _count )
					_array[ index ] = value;
				throw new ArgumentOutOfRangeException( nameof(index) );
			}
		}

		public void Dispose()
		{
			if( _array != null )
				ArrayPool<T>.Shared.Return( _array );
					
			_array = null;
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
		
		public IEnumerator<T> GetEnumerator( )
		{
			var a	= _array;
			var max	= _count;
				
			for( var i = 0; i < max; i++ )
				yield return a[i];
		}
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return GetEnumerator( );
		}
	}
}