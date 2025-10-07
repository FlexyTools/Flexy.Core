using Unity.Collections.LowLevel.Unsafe;

namespace Flexy.Core.Binding
{
	public class BindableDataStore : BindableBehaviour
	{
		[SerializeField] private GameObject?	_exposedObject;

		private readonly Dictionary<String, Object> _objectsDict	= new();
		private readonly Dictionary<String, Int64>	_dataDict		= new();
		
		public Object?		MainObject		{ get; set; }
		public GameObject?	ExposedObject	=> _exposedObject;

		public void SetValue    ( String key, Sprite value )	=> _objectsDict[key] = value;
		public void SetValue    ( String key, Enum value )		=> _objectsDict[key] = value;
		public void SetValue    ( String key, Color value )		=> SetValue(key, (Color32)value);
		public void SetValue    ( String key, Color32 value )	=> _dataDict[key] = UnsafeUtility.As<Color32, Int32>(ref value);
		public void SetValue    ( String key, String value )	=> _objectsDict[key] = value;
		public void SetValue    ( String key, Int32 value )		=> _dataDict[key] = value;
		public void SetValue    ( String key, Int64 value )		=> _dataDict[key] = value;
		public void SetValue    ( String key, Boolean value )	=> _dataDict[key] = value ? 1 : 0;
		public void SetValue    ( String key, Single value )	=> _dataDict[key] = UnsafeUtility.As<Single, Int32>(ref value);
		public void SetValue	( String key, Func<BindableDataStore, Boolean> getter )	=> _objectsDict[key] = getter;
		public void SetValue	( String key, Func<BindableDataStore, Int64> getter )	=> _objectsDict[key] = getter;
		public void SetValue	( String key, Func<BindableDataStore, Int32> getter )	=> _objectsDict[key] = getter;
		public void SetValue	( String key, Func<BindableDataStore, Single> getter)	=> _objectsDict[key] = getter;

		[Bindable]	public Int64	GetInt64	( String key )	=> GetDataVal64(key);
		[Bindable]	public Int32	GetInt      ( String key )	=> GetDataVal(key);
		[Bindable]	public Single	GetSingle   ( String key )	
		{
			var val = GetDataVal( key );
			
			return UnsafeUtility.As<Int32, Single>(ref val);
		}
		[Bindable]	public Color32	GetColor	( String key )	
		{
			var val = GetDataVal( key );
			return UnsafeUtility.As<Int32, Color32>(ref val);
		}
		[Bindable]	public Sprite?	GetSprite   ( String key )	=> (Sprite?)GetObjectVal(key);
		[Bindable]	public String?	GetString   ( String key )	=> (String?)GetObjectVal(key);
		[Bindable]	public Boolean	GetBoolean  ( String key )	=> GetDataVal(key) != 0;

		private	Object?	GetObjectVal	( String key )	
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
					case Func<BindableDataStore, Color32> a:		
					{
						var value = a(this);

						return UnsafeUtility.As<Color32, Int32>(ref value);
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
				if (_dataDict.TryGetValue( key, out val ))
					return val;
			}

			//try get dynamic val
			{
				Object val;
				if (!_objectsDict.TryGetValue( key, out val ))
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
					case Func<BindableDataStore, Color32> a:		
					{
						var value = a(this);
						return UnsafeUtility.As<Color32, Int32>(ref value);
					}
				}
			}

			return 0;
		}
		
		public Action<BindableDataStore>?			Click;
		public Action<BindableDataStore, Int32>?	ClickWithData;

		[Callable] public void DoClick			( )				
		{
			Click?.Invoke( this );
		}
		[Callable] public void DoClickWithData	( Int32 data )	
		{
			ClickWithData?.Invoke( this, data );
		}
	}
}