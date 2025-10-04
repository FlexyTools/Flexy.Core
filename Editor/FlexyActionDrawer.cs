using Flexy.Core.Actions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[CustomPropertyDrawer(typeof(FlexyAction))]
	public class FlexyActionDrawer : PolymorphicPropertyDrawer
	{
	}

	[CustomPropertyDrawer(typeof(FlexyEvent))]
	public class FlexyEventDrawer : PolymorphicPropertyDrawer
	{
		public override VisualElement CreatePropertyGUI( SerializedProperty property )
		{
			return CreatePropertyGUI( property.FindPropertyRelative( "_action" ), property.displayName );
		}
	}
}