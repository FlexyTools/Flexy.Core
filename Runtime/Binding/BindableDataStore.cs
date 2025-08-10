using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

namespace Flexy.Core.Binding
{
	public class BindableDataStore : BindableBehaviour
	{
		[SerializeField] private String[]	_keys;
		[SerializeField] private GameObject	_exposedObject;

		private readonly Dictionary<String, Object> _objectsDict	= new Dictionary<String, Object>( );
		private readonly Dictionary<String, Int64>	_dataDict		= new Dictionary<String, Int64>( );
		
		public Object MainObject { get; set; }
		public GameObject ExposedObject => _exposedObject;

		public void SetValue    ( String key, Sprite value )	
		{
		  ThrowOnWrongKey( key );

		  _objectsDict[key] = value;
		}
		public void SetValue    ( String key, Enum value )	
		{
		  ThrowOnWrongKey( key );

		  _objectsDict[key] = value;
		}		
		public void SetValue    ( String key, Color value )		
		{
		  ThrowOnWrongKey( key );

		  _objectsDict[key] = value;
		}
		public void SetValue    ( String key, String value )	
		{
			ThrowOnWrongKey( key );

			_objectsDict[key] = value;
		}
		public void SetValue    ( String key, Int32 value )		
		{
			ThrowOnWrongKey( key );

			_dataDict[key] = value;
		}
		public void SetValue    ( String key, Int64 value )		
		{
			ThrowOnWrongKey( key );

			_dataDict[key] = value;
		}
		public void SetValue    ( String key, Boolean value )	
		{
			ThrowOnWrongKey( key );

			_dataDict[key] = value ? 1 : 0;
		}
		public void SetValue    ( String key, Single value )	
		{
			ThrowOnWrongKey( key );

			_dataDict[key] = UnsafeUtility.As<Single, Int32>(ref value);
		}
		public void SetValue	( String key, Func<BindableDataStore, Boolean> getter )	
		{
			ThrowOnWrongKey( key );

			_objectsDict[key] = getter;
		}
		public void SetValue	( String key, Func<BindableDataStore, Int64> getter )	
		{
			ThrowOnWrongKey( key );

			_objectsDict[key] = getter;
		}
		public void SetValue	( String key, Func<BindableDataStore, Int32> getter )	
		{
			ThrowOnWrongKey( key );

			_objectsDict[key] = getter;
		}
		public void SetValue	( String key, Func<BindableDataStore, Single> getter)	
		{
			ThrowOnWrongKey( key );

			_objectsDict[key] = getter;
		}

		[Bindable]	public Int64	GetInt64	( String key )
		{
			return GetDataVal64( key );
		}
		[Bindable]	public Int32	GetInt      ( String key )
		{
			return GetDataVal( key );
		}
		[Bindable]	public Single	GetSingle   ( String key )
		{
			var val = GetDataVal( key );
			
			return UnsafeUtility.As<Int32, Single>(ref val);
		}
		[Bindable]	public Sprite	GetSprite   ( String key )
		{
			return (Sprite)GetObjectVal( key );
		}
		[Bindable]	public Color	GetColor	( String key )
		{
			return (Color)GetObjectVal( key );
		}
		[Bindable]	public String	GetString   ( String key )
		{
			return (String)GetObjectVal( key );
		}
		[Bindable]	public Boolean	GetBoolean  ( String key )
		{
			return GetDataVal( key ) != 0;
		}

		private	Object	GetObjectVal	( String key )
		{
			Object val;
			return _objectsDict.TryGetValue( key, out val ) ? val : null;
		}                           	
		private	Int32	GetDataVal		( String key )
		{
			//try get data val
			{
				Int64 val;
				if( _dataDict.TryGetValue( key, out val ) )
					return (Int32)val;
			}

			//try get dynamic val
			{
				Object val;
				if( !_objectsDict.TryGetValue( key, out val ) )
					return 0;

				switch( val )
				{
					case Func<BindableDataStore, Int32> a:		return a(this);
					case Func<BindableDataStore, Boolean> a:	return a(this)? 1: 0;
					case Func<BindableDataStore, Single> a:		
					{
						var value = a(this);

						return UnsafeUtility.As<Single, Int32>(ref value);
					}
				}
			}

			return 0;
		}
		private	Int64	GetDataVal64	( String key )
		{
			//try get data val
			{
				Int64 val;
				if( _dataDict.TryGetValue( key, out val ) )
					return val;
			}

			//try get dynamic val
			{
				Object val;
				if( !_objectsDict.TryGetValue( key, out val ) )
					return 0;
				
				switch( val )
				{
					case Func<BindableDataStore, Int64> a: 		return a(this);
					case Func<BindableDataStore, Int32> a: 		return a(this);
					case Func<BindableDataStore, Boolean> a:	return a(this)? 1: 0;
					case Func<BindableDataStore, Single> a:		
					{
						var value = a(this);

						return UnsafeUtility.As<Single, Int32>(ref value);
					}
				}
			}

			return 0;
		}
		
		public Action<BindableDataStore>		Click;
		public Action<BindableDataStore, Int32>	ClickWithData;

		[Callable] public void DoClick( )
		{
			Click?.Invoke( this );
		}
		[Callable] public void DoClickWithData( Int32 data )
		{
			ClickWithData?.Invoke( this, data );
		}
		
		private void ThrowOnWrongKey ( String key )
		{
			if( _keys != null && _keys.Length > 0 && _keys.All( k => k != key ) )
				throw new ArgumentException( "BindableDataStore does not contain key: " + key );
		}
		
	}
}