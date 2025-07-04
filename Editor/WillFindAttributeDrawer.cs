using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor
{
	[CustomPropertyDrawer(typeof(WillFindAttribute))]
	public class WillFindAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty( position, label, property );

			//base.OnGUI(position, property, label);
			position = EditorGUI.PrefixLabel(position, label);

			if( property.propertyType == SerializedPropertyType.ObjectReference )
			{ 
				if( property.objectReferenceValue == null )
				{
					var pos = position;
					pos.width = 120;
					EditorGUI.HelpBox(pos, ((WillFindAttribute)attribute).Text, MessageType.None);	
					position.xMin += 120;
				}
			}
			
			EditorGUI.PropertyField(position, property, GUIContent.none);
			
			EditorGUI.EndProperty( );
		}
	}
}