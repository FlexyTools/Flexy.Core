using UnityEngine.UI;

namespace Flexy.Core
{
    /// A concrete subclass of the Unity UI `Graphic` class that just skips drawing.
    /// Useful for providing a raycast target without actually drawing anything.
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyGraphic : Graphic
    {
        public override		void	SetMaterialDirty	( ) { }
        public override		void	SetVerticesDirty	( ) { }

        /// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
        protected override	void	OnPopulateMesh		( VertexHelper vh ) { vh.Clear(); }
    }
    
    #if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor( typeof(EmptyGraphic), false )]
    public class NonDrawingGraphicEditor : UnityEditor.UI.GraphicEditor
    {
	    public override void OnInspectorGUI ( )
	    {
		    serializedObject.Update();
		    //UnityEditor.EditorGUILayout.PropertyField( m_Script );
		    // skipping AppearanceControlsGUI
		    RaycastControlsGUI();
		    serializedObject.ApplyModifiedProperties();
	    }
    }
    #endif
}