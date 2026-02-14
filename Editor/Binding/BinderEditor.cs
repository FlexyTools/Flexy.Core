namespace Flexy.Core.Editor.Binding;

[CustomEditor(typeof(Binder), true)]
public class BinderEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		
		DrawPropertiesExcluding(serializedObject, "m_Script");
			
		serializedObject.ApplyModifiedProperties();
	}
}