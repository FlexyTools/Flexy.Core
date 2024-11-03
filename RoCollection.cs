using System.Collections;

namespace Flexy.Core
{
	public struct RoCollection<T> : IReadOnlyCollection<T>
	{
		public RoCollection( ICollection<T> decoratedCollection) => _decoratedCollection = decoratedCollection;
		
		private readonly ICollection<T> _decoratedCollection;

		public Int32		Count		=> _decoratedCollection.Count;
		public Boolean		IsReadOnly	=> true;
		
		public IEnumerator<T>	GetEnumerator( )	=> _decoratedCollection.GetEnumerator( );
		IEnumerator IEnumerable.GetEnumerator( )	=> GetEnumerator( );

		public Boolean	Contains( T item )						=> _decoratedCollection.Contains	( item );
		public void		CopyTo	( T[] array, Int32 arrayIndex )	=> _decoratedCollection.CopyTo		( array, arrayIndex );
	}
}