using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[CustomPropertyDrawer(typeof(PlymorphicAttribute))]
    public class PolymorphicPropertyDrawer : PropertyDrawer
    {
		private const	String			NullName = "None";
		
        public override	VisualElement	CreatePropertyGUI	( SerializedProperty property )						
		{
			return CreatePropertyGUI( property, property.displayName );
		}
		public			VisualElement	CreatePropertyGUI	( SerializedProperty property, String displayName )	
        {
			if ( property.propertyType != SerializedPropertyType.ManagedReference )
				return new PropertyField( property );
			
			var attr		= (PlymorphicAttribute)attribute;
			var baseType	= attr?.BaseType ?? GetType( property.managedReferenceFieldTypename );
			
			var foldout = new Foldout
			{
				text = displayName,
				pickingMode = PickingMode.Ignore, 
				bindingPath = property.propertyPath
			};
			
			var toggle = foldout.Q<Toggle>(null, Foldout.toggleUssClassName);
			var toggleRow = toggle.hierarchy[0];
			var checkmark = toggleRow.hierarchy[0];
			var toggleLabel = toggleRow.hierarchy[1];
			
			toggle.style.marginTop = 0;
			toggle.style.marginBottom = 0;
			toggleLabel.style.display = DisplayStyle.None;
			
			var propField	= new ContainerField( displayName ){ style = { flexGrow = 1, flexShrink = 1, overflow = Overflow.Visible, marginTop = 0, marginBottom = 0} };
			propField.AddToClassList(ContainerField.alignedFieldUssClassName);
			propField.style.marginLeft = 0;
			propField.labelElement.style.alignSelf = Align.Center;
			
			toggleRow.hierarchy.Add(propField);
			
			var button		= new Button		{ text = "⦿", style = { maxWidth = 20, paddingLeft = 3, paddingRight = 3}};
			var propsInline	= new VisualElement { name = "PropsInline", style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
			
			propField.contentContainer.Add( propsInline );
			propField.contentContainer.Add( new(){ style = { flexGrow = 0.01f, minWidth = 2} } );
			propField.contentContainer.Add( button );
			
			propField.userData		= property;
			
			SetupChooseItemButton( propField, button, baseType, type => SetNewInstance(type, property) );
			
			BuildUI(foldout, checkmark, propField, propsInline, property, displayName);
			
			foldout.TrackPropertyValue(property, _ => BuildUI(foldout, checkmark, propField, propsInline, property, displayName));
			
			return foldout;
		}

		private			void			BuildUI				( Foldout foldout, VisualElement checkmark, ContainerField propField, VisualElement inline, SerializedProperty property, String displayName )
		{
			var block = foldout.contentContainer;
		
			inline.Unbind();
			block.Unbind();
			block.Clear();
			inline.Clear();
			
			SetFieldName( displayName, propField );
			PopulateInnerProps( inline, block, property );
			
			inline.Bind( property.serializedObject );
			block.Bind( property.serializedObject );
			
			if( block.hierarchy.childCount == 0 )
			{
				foldout.value = false;
				checkmark.style.visibility = Visibility.Hidden; 
			}
			else
			{
				foldout.value = false;
				checkmark.style.visibility = Visibility.Visible;
			}
		}
		private			void			SetFieldName		( String displayName, ContainerField field )
		{
			var property	= (SerializedProperty)field.userData;
			var path		= property.propertyPath;
			
			var selectedType		= GetType( property.managedReferenceFullTypename );
			var selectedTypeName	= NicifyTypeName(selectedType);
			
			if( path[^1] == ']' )
			{
				var startIndex	= path.LastIndexOf('[');
				var index		= Int32.Parse( path[(startIndex+1)..^1] );
				
				field.label =  $"{index:D2}.{selectedTypeName}";
			}
			else
			{
				field.label = displayName + ": " + selectedTypeName;
			}
		}

		private static	void			SetupChooseItemButton( VisualElement root, Button btn, Type propertyType, Action<Type> onSelectedNewType )
        {
	        var assignableTypes = (List<Type>)btn.userData;
	        
	        if( assignableTypes == null )
			{
				assignableTypes = GetAssignableTypes(propertyType);
				btn.userData = assignableTypes;
			}
	        
	        btn.clickable.clicked += ShowDropdown;

	        void ShowDropdown()
	        {
		        var dropdown = new FlexyAdvancedDropdown( assignableTypes.Select(NicifyTypeFullName), index => onSelectedNewType( assignableTypes[index] ) );
		        var buttonMatrix = root.worldTransform;
		        var position = new Vector3(buttonMatrix.m03, buttonMatrix.m13, buttonMatrix.m23);
		        var size = root.contentRect.size;
		        size.x = Math.Max( 400, size.x ); 
		        var buttonRect = new Rect(position, size );
		        
		        dropdown.Show(buttonRect);
	        }
        }
		
		private static	Type			GetType				( String typename )		
		{
			if (String.IsNullOrWhiteSpace( typename ))
				return null!;
			
			var parts		= typename.Split( ' ' );
			return Type.GetType( $"{parts[1]}, {parts[0]}", false );
		}
		private static	String			NicifyTypeFullName	( Type type )			=> type == null ? NullName : ObjectNames.NicifyVariableName( type.FullName );
		private static	String			NicifyTypeName		( Type type )			=> type == null ? NullName : ObjectNames.NicifyVariableName( type.Name );
        private static	List<Type>		GetAssignableTypes	( Type type )			
        {
			var nonUnityTypes	= TypeCache.GetTypesDerivedFrom(type).Where(IsAssignableNonUnityType).ToList();
			nonUnityTypes.Sort( (l, r) => String.Compare( l.FullName, r.FullName, StringComparison.Ordinal) );
			nonUnityTypes.Insert(0, null);
			return nonUnityTypes;

			Boolean IsAssignableNonUnityType(Type type)
			{
				return type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsSubclassOf(typeof(UnityEngine.Object)) && type.GetCustomAttributes().All( a => !a.GetType().Name.Contains( "BakingType" )  );
			}
        }

        private void	SetNewInstance		( Type newType, SerializedProperty property )		
        {
			var newValue = newType != null ? FormatterServices.GetUninitializedObject(newType) : null;
			property.managedReferenceValue = newValue;
			property.serializedObject.ApplyModifiedProperties();
        }
		private void	PopulateInnerProps	( VisualElement propsInline, VisualElement propsBlock, SerializedProperty property )						
		{
			propsBlock.hierarchy.Clear();
			
			if (property.managedReferenceValue == null)
				return;
			
			var attr			= property.managedReferenceValue.GetType().GetCustomAttribute<InlineFieldsAttribute>();
			var inlineFields	= attr != null ? attr.FieldNames : Array.Empty<String>( );
			
			var copy	= property.Copy();
			var depth	= copy.depth + 1;

			for (var i = 0; copy.NextVisible( i==0 ) && copy.depth >= depth; i++)
			{
				var putInline = (!copy.isArray || copy.propertyType == SerializedPropertyType.String) && ( i < 1 || inlineFields.Contains( copy.name ));
				if (putInline)
					propsInline.Add( new PropertyField(copy, String.Empty){ style = { flexGrow = 1f} } );
				
				else
					propsBlock.Add( new PropertyField(copy) );
			}
		}
    }
	
	public class ContainerField : BaseField<ContainerField.voidd>
	{
		public ContainerField( String label ) : base( label, new (){ style = { flexGrow = 0 } } ) 
		{
		
		}
		
		public struct voidd{}
	}
}