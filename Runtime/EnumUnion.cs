using System.Runtime.InteropServices;

namespace Flexy.Core;

public static class EnumUnion
{
	public static Int32	TtoV<T>( T t )		where T:Enum 	=> (Int32) new EnumUnion<T> { Enum	= t }.Raw;
	public static Int64	TtoV64<T>( T t )	where T:Enum 	=> new EnumUnion<T> { Enum	= t }.Raw;
	public static T		VtoT<T>( Int32 v )	where T:Enum 	=> new EnumUnion<T> { Raw = v }.Enum;
	public static T		VtoT<T>( Int64 v )	where T:Enum 	=> new EnumUnion<T> { Raw = v }.Enum;
}

[StructLayout(LayoutKind.Explicit)]
public ref struct EnumUnion<T> where T:Enum
{
	[FieldOffset(0)] public T		Enum;
	[FieldOffset(0)] public Int64	Raw;
}