using Unity.Mathematics;

namespace Flexy.Core.Tweens;

public struct		Int32Lerper		: ITweenLerper<Int32>		{ public Int32		Lerp( Int32 a, Int32 b, Single t )			=> (Int32)Mathf.LerpUnclamped( a, b, t ); }
public struct		Int64Lerper		: ITweenLerper<Int64>		{ public Int64		Lerp( Int64 a, Int64 b, Single t )			=> (Int64)math.lerp( (Double)a, b, t ); }
public struct		SingleLerper	: ITweenLerper<Single>		{ public Single		Lerp( Single a, Single b, Single t )		=> Mathf.LerpUnclamped( a, b, t ); }
public struct		DoubleLerper	: ITweenLerper<Double>		{ public Double		Lerp( Double a, Double b, Single t )		=> math.lerp( a, b, t ); }
public struct		Vector2Lerper	: ITweenLerper<Vector2>		{ public Vector2	Lerp( Vector2 a, Vector2 b, Single t )		=> Vector2.LerpUnclamped( a, b, t ); }
public struct		Vector3Lerper	: ITweenLerper<Vector3>		{ public Vector3	Lerp( Vector3 a, Vector3 b, Single t )		=> Vector3.LerpUnclamped( a, b, t ); }
public struct		Vector4Lerper	: ITweenLerper<Vector4>		{ public Vector4	Lerp( Vector4 a, Vector4 b, Single t )		=> Vector4.LerpUnclamped( a, b, t ); }
public struct		QuaternionLerper: ITweenLerper<Quaternion>	{ public Quaternion	Lerp( Quaternion a, Quaternion b, Single t )=> Quaternion.LerpUnclamped( a, b, t ); }
public struct		ColorLerper		: ITweenLerper<Color>		{ public Color		Lerp( Color a, Color b, Single t )			=> Color.LerpUnclamped( a, b, t ); }
public struct		Color32Lerper	: ITweenLerper<Color32>		{ public Color32	Lerp( Color32 a, Color32 b, Single t )		=> Color32.LerpUnclamped( a, b, t );  }
public struct		RectLerper		: ITweenLerper<Rect>		{ public Rect		Lerp( Rect a, Rect b, Single t )			=> new( math.lerp( a.min, b.min, t ), math.lerp( a.size, b.size, t ) ); }