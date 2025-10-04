namespace Flexy.Core;

public class PlymorphicAttribute : PropertyAttribute 
{
	public PlymorphicAttribute( ){}
	public PlymorphicAttribute( Type baseType ) { BaseType = baseType; }

	public Type		BaseType		{ get; set; }
}

public class InlineFieldsAttribute : Attribute
{
	public InlineFieldsAttribute( params String[] fieldNames ) { FieldNames = fieldNames; }
	
	public String[] FieldNames		{ get; set; }
}