using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Flexy.Core.Editor
{
    public class FlexyAdvancedDropdown : AdvancedDropdown
    {
	    static FlexyAdvancedDropdown( )
	    {
		    var prop		= typeof(AdvancedDropdownItem).GetProperty( "content", BindingFlags.Instance | BindingFlags.NonPublic );
		    var getMethod	= prop.GetGetMethod( true );
		    
		    var d = (Func<AdvancedDropdownItem, GUIContent>) Delegate.CreateDelegate( typeof(Func<AdvancedDropdownItem, GUIContent>), getMethod );
		    
		    GetContent = d;
	    }
		public FlexyAdvancedDropdown( IEnumerable<String> names, Action<Int32> nameSelectedCallback ) : this( new(), names, nameSelectedCallback ){}
		public FlexyAdvancedDropdown( AdvancedDropdownState state, IEnumerable<String> names, Action<Int32> nameSelectedCallback ) : base( state )	
	    {
		    _names = names;
		    _nameSelectedCallback = nameSelectedCallback;
	    }
	    
        private readonly	IEnumerable<String>		_names;
        private readonly	Action<Int32>			_nameSelectedCallback;

        protected override	AdvancedDropdownItem	BuildRoot		( )								
        {
            var root = new AdvancedDropdownItem( "Types" );
            
            var index = 0;
            foreach ( var typeName in _names )
            {
	            var nameParts			= typeName.Split('.');
	            var currentDepathItem	= root;

	            foreach ( var part in nameParts )
	            {
					#if UNITY_6000_5_OR_NEWER
		            var item = currentDepathItem.childList.FirstOrDefault( c => c.ToString().Equals( part ) );
		            #else
		            var item = currentDepathItem.children.FirstOrDefault( c => c.ToString().Equals( part ) );
		            #endif
		            
		            if( item == null )
					{
						item = new( part );
						currentDepathItem.AddChild( item );
					}
		            
		            currentDepathItem = item;
	            }
	            
				currentDepathItem.id = index;
                index++;
            }

            CleanupHierarchy( root );
            
			#if UNITY_6000_5_OR_NEWER
            // Add empty items to make drop dawn height adequate
            while( root.childList.Count() < 24 )
			{
				root.AddChild( new( "" ){ id = -1} );
			}
			#else
			// Add empty items to make drop dawn height adequate
            while( root.children.Count() < 24 )
			{
				root.AddChild( new( "" ){ id = -1} );
			}
			#endif

            return root;
            
            static void CleanupHierarchy( AdvancedDropdownItem node )
            {
				#if UNITY_6000_5_OR_NEWER
	            var children = (List<AdvancedDropdownItem>)node.childList;
				
	            while( children.Count == 1 && children[0].childList.Any( ) )
	            {
		            // crop single nested node
		            var child = children[0];
		            GetContent( node ).text += "." + child.name;
		            
		            children.Clear( );
		            children.AddRange( child.childList );
	            }
				#else
	            var children = (List<AdvancedDropdownItem>)node.children;
				
	            while( children.Count == 1 && children[0].children.Any( ) )
	            {
		            // crop single nested node
		            var child = children[0];
		            GetContent( node ).text += "." + child.name;
		            
		            children.Clear( );
		            children.AddRange( child.children );
	            }
				#endif
	            
	            foreach (var child in children)
	            {
		            CleanupHierarchy( child );	
	            }
            }
        }
        protected override	void					ItemSelected	( AdvancedDropdownItem item )	
        {
			var index = item.id;
            if ( index >= 0 )
                _nameSelectedCallback.Invoke( index );
        }
		
		private static readonly Func<AdvancedDropdownItem, GUIContent> GetContent;
    }
}