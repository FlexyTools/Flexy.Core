#if VCONTAINER_PACKAGE
using System.Collections;
using System.Linq;
using System.Reflection;
using VContainer;
using VContainer.Diagnostics;
using VContainer.Internal;
using VContainer.Unity;

namespace Flexy.Core.GameContexts;

[RequireComponent(typeof(GameContext))]	
public class VContainerExtension: LifetimeScope, IGameContextExtension 
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void StaticClear( )
	{
#if UNITY_EDITOR 
		var fld = typeof(DiagnositcsContext).GetField( "collectors", BindingFlags.Static | BindingFlags.NonPublic );
		fld.SetValue( null, new Dictionary<string, DiagnosticsCollector>() );

		var fld2 = typeof(DiagnositcsContext).GetField( "OnContainerBuilt", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
		fld2.SetValue(null, null);
#endif
	}

	private Dictionary<Type, Object>? _registeredServicesDict;
	private Boolean _builded = false;

	protected override	void	Awake		( )								
	{
		if (_builded) 
			return;
		
		_builded = true;
		base.Awake();
	}
	protected override	void	Configure	( IContainerBuilder builder )	
	{
		if (_registeredServicesDict == null) 
			return;
		
		foreach ( var group in _registeredServicesDict.GroupBy( p => p.Value, p => p.Key ) )
		{
			var rb = builder.RegisterInstance( group.Key );

			foreach ( var type in group )
				rb.As( type );
		}
	}
		
	public				void	SetParent				( GameContext parent )											
	{
		parentReference.Object = parent.GetComponent<LifetimeScope>();
	}
	public				void	RegisterInitialServices	( Dictionary<Type, Object> registeredServicesDict )				
	{
		_registeredServicesDict = registeredServicesDict;
		
		Awake();		
	}
	public				T		GetService<T>			( ) where T : class												
	{
		return Container.ResolveOrDefault<T>( );
	}
	public				void	SetService<T>			( Type serviceType, T service, Boolean replace) where T : class	
	{
		
	}

#if UNITY_EDITOR
	[RuntimeInspectorGui( Repaint = true )]
	public void RuntimeGUI	( )
	{
		if( !Application.isPlaying || !gameObject.scene.IsValid( ) )
			return;

		GUILayout.Space( 20 );
		GUILayout.Label( "VContainer Services:" );
		GUILayout.Space( 4 );
		{
			var registry	= (Registry)	Container	.GetType().GetField( "registry",	BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Container );
			var hashTable	=				registry	.GetType().GetField( "hashTable",	BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( registry );
			var table		= (IList)		hashTable	.GetType().GetField( "table",		BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( hashTable );
				
			var typeField = default(FieldInfo);
				
			foreach ( IList arr in table )
			{
				if (arr == null) 
					continue;
					
				foreach (var item in arr)
				{
					if ( typeField == null )
						typeField	= item.GetType().GetField( "Type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
							
					var type = (Type)typeField.GetValue( item );
							
					if ( type == null )
						continue;
							
							
					var key			= GameContext.GetDisplayServiceName(type);

					if( key is "Object" or "LifetimeScope" or "IObjectResolver" or "EntryPointDispatcher" )
						continue;
							
					GUILayout.Label( key );
				}
			}
		}
	}
#endif
}
#endif