namespace Flexy.Core;

public ref struct SpanList<T>
{
	public	SpanList( Span<T> backingSpan )						
	{
		_backingSpan	= backingSpan;
		_count			= 0;
	}
	public	SpanList( Span<T> backingSpan, IList<T> initial )	
	{
		_backingSpan = backingSpan;
		_count = Math.Min(_backingSpan.Length, initial.Count);

		for (var i = 0; i < _count; i++)
			_backingSpan[i] = initial[i];
	}
		
	private	Span<T>	_backingSpan;
	private	Int32	_count;
		
	public	Int32	Count => _count;
	public	T		this[Int32 index]	
	{
		get
		{
			if (index>= _count)
				throw new ArgumentOutOfRangeException( nameof(index) );
		
			
			return  _backingSpan[index];
		}
		set
		{
			if (index>= _count)
				throw new ArgumentOutOfRangeException( nameof(index) );
				
			_backingSpan[index] = value;
		}
	}
			
	public	void	Add			( T item )					
	{
		if( _count >= _backingSpan.Length ) //array fully filled
			throw new InvalidOperationException( "SpanList can not reallocate to bigger capacity" );
		
		var index = _count;
		_count++;
		_backingSpan[index] = item;
	}
	public	void	AddRange	( IEnumerable<T> items )	
	{
		foreach (var item in items)
			Add(item);
	}
		
	public Span<T>.Enumerator GetEnumerator	( )				
	{
		return _backingSpan[.._count].GetEnumerator();
	}
}