using Flexy.Core.Tweens;

namespace Flexy.Core;

public static class TweenExtensions
{
	public static	Builder<Single, SingleLerper>		    Value		        ( this FTween ft, Single from,	Single to,	Single duration, Ease ease = default )					=> Value<Single, SingleLerper>		( ft, from, to, duration, ease );
	public static	Builder<Int32, Int32Lerper>			    Value		        ( this FTween ft, Int32 from,	Int32 to,	Single duration, Ease ease = default )					=> Value<Int32, Int32Lerper>		( ft, from, to, duration, ease );
	                                                                        	
	public static	Builder<Vector3, Vector3Lerper>		    Position	        ( this FTween ft,	Transform tr, Vector3 from,	Vector3 to,		Single duration, Ease ease = default )		=> ft.Value<Vector3, Vector3Lerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.position = value );
	public static	Builder<Single, SingleLerper>		    PositionX	        ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.position = tr.position with {x=value} );
	public static	Builder<Single, SingleLerper>		    PositionY	        ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.position = tr.position with {y=value} );
	public static	Builder<Single, SingleLerper>		    PositionZ	        ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.position = tr.position with {z=value} );
	                                                                        	
	public static	Builder<Vector3, Vector3Lerper>		    LocalPosition	    ( this FTween ft,	Transform tr, Vector3 from,	Vector3 to,		Single duration, Ease ease = default )		=> ft.Value<Vector3, Vector3Lerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.localPosition = value );
	public static	Builder<Single, SingleLerper>		    LocalPositionX	    ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.localPosition = tr.localPosition with {x=value} );
	public static	Builder<Single, SingleLerper>		    LocalPositionY	    ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.localPosition = tr.localPosition with {y=value} );
	public static	Builder<Single, SingleLerper>		    LocalPositionZ	    ( this FTween ft,	Transform tr, Single from,	Single to,		Single duration, Ease ease = default )		=> ft.Value<Single, SingleLerper>( from, to, duration, ease )	.BindTo( tr, static (tr, value) => tr.localPosition = tr.localPosition with {z=value} );
	
	public static	Builder<Quaternion, QuaternionLerper>	Rotation	        ( this Builder<Quaternion, QuaternionLerper> b,	Transform tr, Quaternion from,	Quaternion to,	Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.rotation = value );
	public static	Builder<Single, SingleLerper>			RotationX	        ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.eulerAngles = tr.eulerAngles with {x=value} );
	public static	Builder<Single, SingleLerper>			RotationY	        ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.eulerAngles = tr.eulerAngles with {y=value} );
	public static	Builder<Single, SingleLerper>			RotationZ	        ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.eulerAngles = tr.eulerAngles with {z=value} );
	                                                                        	
	public static	Builder<Quaternion, QuaternionLerper>	LocalRotation       ( this Builder<Quaternion, QuaternionLerper> b,	Transform tr, Quaternion from,	Quaternion to,	Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.localRotation = value );
	public static	Builder<Single, SingleLerper>			LocalRotationX      ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.localEulerAngles = tr.localEulerAngles with {x=value} );
	public static	Builder<Single, SingleLerper>			LocalRotationY      ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.localEulerAngles = tr.localEulerAngles with {y=value} );
	public static	Builder<Single, SingleLerper>			LocalRotationZ      ( this Builder<Single, SingleLerper> b,			Transform tr, Single from,		Single to,		Single duration, Ease ease = default )		=> b.BindTo( tr, static (tr, value) => tr.localEulerAngles = tr.localEulerAngles with {z=value} );
	
	private static	Builder<T, TLerp>						Value<T, TLerp> ( this FTween ts, T from , T to, Single duration, Ease ease = default ) where T: unmanaged where TLerp: unmanaged, ITweenLerper<T> => new( from, to , duration, ease );
}