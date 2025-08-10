using System.Linq;

namespace Flexy.Core.Binding
{
	public abstract class BindableBehaviour : MonoBehaviour, IBindersNotifier
	{
		protected			Boolean				_isBindUnready	= false;

		private readonly	Dictionary<String, List<Binder>>	_attachedBinders		= new Dictionary<String, List<Binder>>( );
    
		public				Boolean				ReadyForBind			
		{
			get => !_isBindUnready;
			set => _isBindUnready = !value;
		}

		public		void			AttachBinder				( Binder binder )	
		{
			if( !_attachedBinders.TryGetValue( binder.MemberName, out var list ) )
				_attachedBinders.Add( binder.MemberName, ( list = new List<Binder>( ) ) );
			
			list.Remove( binder );
			list.Add( binder );
			
		}
		public		void			DetachBinder				( Binder binder )	
		{
			if( _attachedBinders.TryGetValue( binder.MemberName, out var list ) )
				list.Remove( binder );
		}
	  
		public		virtual void	MakeBindReadyAndRebindAll	( )					
		{
			_isBindUnready			= false;
			RebindAll();
		}
		public		void			MakeBindUnready				( )					
		{
			_isBindUnready			= true;
		}

		public	virtual void		RebindAll				    ( params String[] excludeNames )	
		{
			if (_isBindUnready)
				return;
			
			DoRebindProperty	( "*", excludeNames );
		}
		protected 	void 			RebindProperty				( params String[] names )			
		{
			foreach ( var n in names )
				DoRebindProperty( n );
			
		}
		protected 	void 			RebindProperty				( String name )						
		{
			DoRebindProperty( name );
		}
		
		private		void			DoRebindAllProperties		( params String[] excludeNames )
		{
			using var array = TempList<String>.Rent( _attachedBinders.Count );
			
			try
			{
				foreach ( var attachedBinder in _attachedBinders )
					array.Add( attachedBinder.Key );

				for ( var i = 0; i < array.Count; i++ )
				{
					var propName = array[ i ];
					DoRebindProperty( propName, excludeNames );
				}
			}
			catch ( Exception ex )	{ Debug.LogException( ex ); }
		}
		private		void			DoRebindProperty			( String name, params String[] excludeNames )		
		{
			if( _attachedBinders == null || _attachedBinders.Count == 0 )
				return;
			
			if( name == "*" )
			{
				DoRebindAllProperties( excludeNames );
				return;
			}
			
			if ( !_attachedBinders.TryGetValue( name, out var list ) )
				return;
			
			//Profiler.BeginSample( $"Do Rebind Property: {name}" );
			
			using var tempList = TempList<Binder>.Rent( list.Count );

			try
			{
				for( var i = 0; i < list.Count; i++ )
				{
					var binder			= list[i];
					var unityObject		= binder.Component as UnityEngine.Object;
				
					if( unityObject != null && !unityObject )
					{
						list.RemoveAt( i );
						i--;
						continue;
					}
					tempList.Add( binder );
				}

				for ( var i = 0; i < tempList.Count; i++ )
				{
					var binder = tempList[ i ];

					if ( excludeNames != null && excludeNames.Contains( binder.MemberName ) )
						continue;

					if ( !binder.IsAlive(  ) || !binder.isActiveAndEnabled )
						continue;

					//Profiler.BeginSample( "Rebind" );

					try
					{
						Binder.Internal.RebindOn( binder );
					}
					catch( Exception ex )
					{
						Debug.LogException( ex );
					}

					//Profiler.EndSample( );
				}
			}
			catch ( Exception ex )	
			{ 
				Debug.LogException( ex );
			}			
			
			//Profiler.EndSample( );
		}
		
		
		#if UNITY_EDITOR
		[RuntimeInspectorGui]
		internal void RuntimeUI()
		{
			if( !UnityEditor.EditorApplication.isPlaying || !gameObject.scene.IsValid( ) )
				return;
			
			var binders = _attachedBinders;
			
			if( binders.Count == 0 )
				return;
			
			UnityEditor.EditorGUILayout.HelpBox( "Attached binders", UnityEditor.MessageType.Info );
			
			try						
			{
				GUI.enabled = false;
				
				
				
				foreach ( var binderList in binders )
				{
					GUILayout.Space(8);
					
					foreach ( var item in binderList.Value )
						UnityEditor.EditorGUILayout.ObjectField( new GUIContent( item.MemberName ), item, typeof(Binder), true );	
				}
			}
			finally
			{
				GUI.enabled = true;
			}
		}
		#endif
	}
	
	#if UNITY_EDITOR
	[UnityEditor.CustomEditor( typeof(BindableBehaviour), true), UnityEditor.CanEditMultipleObjects]
	public class Editor : Flexy.Core.Editor.Editor_WithRuntimeGui{ } 
	#endif
}