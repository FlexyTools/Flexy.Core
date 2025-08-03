using Flexy.Core.Actions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[CustomPropertyDrawer(typeof(FlexyAction))]
	public class FlexyActionDrawer : PolymorphPropertyDrawer
	{
	}

	[CustomPropertyDrawer(typeof(FlexyEvent))]
	public class FlexyEventDrawer : PolymorphPropertyDrawer
	{
		public override VisualElement CreatePropertyGUI( SerializedProperty property )
		{
			return CreatePropertyGUI( property.FindPropertyRelative( "_action" ), property.displayName );
		}
	}
}