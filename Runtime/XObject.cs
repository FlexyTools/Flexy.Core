// using Flexy.JsonXs;
// using Flexy.JsonXs.Format;

namespace Flexy.Core
{
    //[JsonXObject(false, true, SerializeProperties = true)]
    public abstract class XObject : ScriptableObject
    {
	    public static Int32 GlobalDisableValidate;
		
        [SerializeField, HideInInspector]
        //[JsonX(JsonXObject.C0, isInline:true), XToken(ETokenString.Bare)]
        private		String		_guid;
        
		#if !UNITY_EDITOR
		//[JsonX(JsonXObject.N)]		
		public		String		Name	{ get => name; set => name = value; }
		#else
		//[JsonX(JsonXObject.N)]
		public		String		Name	=> name.Trim().Replace(" ","-");
		#endif
        
        #if !UNITY_EDITOR
        public	    String		Guid	=> _guid;
		#else
		public	    String		Guid	
		{
			get
			{
				var path = UnityEditor.AssetDatabase.GetAssetPath( this );
				if( String.IsNullOrEmpty( path ) )
					return _guid;
				
				return (_guid = UnityEditor.AssetDatabase.GUIDFromAssetPath( path ).ToString( ));
			}
		}
		#endif

		public XComponent MainComponent => _components?[0];

		//[JsonX(":Components", order:250, suppressEmpty:true)]
        [SerializeReference]
        private List<XComponent> _components = new( );

		public RoList<XComponent> Components => _components;

		public	Boolean		HasComponent<T>		( ) where T : XComponent					
		{
			foreach ( var component in _components )
			{
				if( component is T tc )
					return true;
			}
		    
			return false;
		}
		public	T			AddComponent<T>		( T c ) where T : XComponent				
		{
			_components.Add( c );
			c.Owner = this;
			
			return c;
		}
		public	T			GetComponent<T>		( ) where T : XComponent					
		{
			foreach ( var component in _components )
			{
				if( component is T tc )
					return tc;
			}
		    
			return default(T);
		}
		public	Boolean		TryGetComponent<T>	( out T c ) where T : XComponent			
		{
			foreach ( var component in _components )
			{
				if( component is T tc )
				{
					c = tc;
					return true;
				}
			}
		    
			c = default;
			return false;
		}
		public	void		RemoveComponentAt	( UInt32 index )							
		{
			if( index >= _components.Count )
				return;
			
			if( _components[(Int32)index] != null )
				_components[(Int32)index].Owner = null;
			
			_components.RemoveAt( (Int32)index );
		}
		public	void		RemoveComponent<T>	( T component ) where T : XComponent		
		{
			component.Owner = null;
			_components.Remove( component );
		}
		public	void		ClearComponents		( )											
		{
			_components.Clear( );
		}
		
		public	void		SetupOwnerToComponents( )										
		{
			foreach( var c in _components )
				if( c is not null )
					c.Owner = this;
		}
		
#if UNITY_EDITOR

	    protected virtual void OnValidate()
	    {
		    if( GlobalDisableValidate > 0 )
			    return;
		    
			try						{ _guid = UnityEditor.AssetDatabase.GUIDFromAssetPath( UnityEditor.AssetDatabase.GetAssetPath( this ) ).ToString( ); }
			catch( Exception ex )	{ Debug.LogException( ex ); }
			
			foreach ( var component in _components )
				component.Owner = this;
			
		    foreach ( var component in _components )
				try						{ component.OnValidate( this ); }
				catch( Exception ex )	{ Debug.LogException( ex ); }
	    }

	    public static class Editor
        {
            public static void ResetGuid( XObject @object, Hash128 guid )
            {
                @object._guid = guid.ToString( );
            }  
        }
		
		public class Internal
		{
			public static void OnValidate(XObject target) => target.OnValidate( );
		}
#endif
	}
}