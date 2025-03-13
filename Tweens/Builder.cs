using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Flexy.Core.Tweens;

public struct FTween { }

public struct Builder<T, TLerp> where T : unmanaged where TLerp : unmanaged, ITweenLerper<T>
{
	public Builder( T from, T to, Single duration, Ease ease = Ease.InOutSine, ETime tweenTime = ETime.DeltaTime )
	{
		Data				= default;
		Data.From			= from;
		Data.To				= to;
		Data.DurationMs		= (UInt16)Mathf.RoundToInt(duration * 1000);
		Data.Ease			= ease;
		Data.TweenTime		= tweenTime;
		Data.LoopsCount		= 1;
	}
	
	internal TweenData<T, TLerp> Data;
	
	public	Builder<T, TLerp> WithDelay				( Single delay, EDelay	delayType	= EDelay.BeforeStart )		{ Data.DelayMs		= (UInt16)Mathf.RoundToInt(delay*1000); Data.DelayType	= delayType;	return this; }
	public	Builder<T, TLerp> WithLoops				( UInt16 count, ELoop	loopType	= ELoop.Repeat )			{ Data.LoopsCount	= count;								Data.LoopType	= loopType;		return this; }
	
	public	Builder<T, TLerp> OnComplete			( Action callback )			{ Data.Callbacks.Completed	= callback;		return this; }
	public	Builder<T, TLerp> OnCancel				( Action callback )			{ Data.Callbacks.Canceled	= callback;		return this; }
	
	public	Builder<T, TLerp> BindTo				(							Action<T>				evaluator )	{ Data.BindData = TweenBindData.Create( evaluator );				return this; }
	public	Builder<T, TLerp> BindTo<TO1>			( TO1 o1,					Action<TO1,T>			evaluator )	{ Data.BindData = TweenBindData.Create( evaluator, o1 );			return this; }
	public	Builder<T, TLerp> BindTo<TO1, TO2>		( TO1 o1, TO2 o2,			Action<TO1,TO2,T>		evaluator )	{ Data.BindData = TweenBindData.Create( evaluator, o1, o2 );		return this; }
	public	Builder<T, TLerp> BindTo<TO1, TO2, TO3>	( TO1 o1, TO2 o2, TO3 o3,	Action<TO1,TO2,TO3,T>	evaluator )	{ Data.BindData = TweenBindData.Create( evaluator, o1, o2, o3 );	return this; }
	
	public	TweenHandle Run( ) => TweenBackend.Ref.Run( this );
}

public enum		ETime : byte
{
	DeltaTime			= 0,
	UnscaledDeltaTime	= 1,
//	Realtime			= 2,
}
public enum		EDelay : byte
{
	BeforeStart		= 0,
	BeforeEveryLoop	= 1,
}
public enum		ELoop : byte
{
	Repeat		= 0,
	Yoyo		= 1,
	Incremental	= 2,
}

public interface ITweenLerper<T> where T: unmanaged		
{ 
	public T Lerp( T a, T b, Single t ); 
}

public struct TweenData<T, TLerp> where T : unmanaged where TLerp : unmanaged, ITweenLerper<T>
{
	public	T				From;
	public	T				To;
	public	UInt16			DurationMs;
	public	UInt16			DelayMs;
	public	UInt16			LoopsCount;
	public	Ease			Ease;
	public	EDelay			DelayType;
	public	ELoop			LoopType;
	public	ETime			TweenTime;
	public	TLerp			Lerper;
	
	public	TweenBindData	BindData;
	public	TweenCallbacks	Callbacks;
}

public struct TweenHandle
{
	public Int64	Id;
	public Int32	SubId;
	public Int32	Version;
	
	public void				Complete	( ) => TweenBackend.Ref.Complete	( this );
	public void				Cancel		( ) => TweenBackend.Ref.Cancel		( this );
	public Boolean			IsValid		( ) => TweenBackend.Ref.IsValid		( this );
	public UniTask.Awaiter	GetAwaiter	( ) => TweenBackend.Ref.Await		( this );
}

public struct TweenCallbacks
{
	public	Action	Completed;
	public	Action	Canceled;
}

public struct TweenBindData
{
	public Object	State1;
	public Object	State2;
	public Object	State3;
	public Byte		StateCount;
	
	private Object	_bindedAction;
	public	Object	BindedAction => _bindedAction;
	
	public static TweenBindData Create					( Object updateAction )							=> new(){ _bindedAction = updateAction, StateCount = 0 };
	public static TweenBindData Create<TO1>				( Object updateAction, TO1 o1 )					=> new(){ _bindedAction = updateAction, StateCount = 1, State1 = o1 };
	public static TweenBindData Create<TO1, TO2>		( Object updateAction, TO1 o1, TO2 o2 )			=> new(){ _bindedAction = updateAction, StateCount = 2, State1 = o1, State2 = o2  };
	public static TweenBindData Create<TO1, TO2, TO3>	( Object updateAction, TO1 o1, TO2 o2, TO3 o3 )	=> new(){ _bindedAction = updateAction, StateCount = 3, State1 = o1, State2 = o2, State3 = o3 };
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void InvokeUnsafe<TValue>(in TValue value) where TValue : unmanaged
	{
		switch ( StateCount )
		{
			case 0: UnsafeUtility.As<Object, Action<TValue>>						( ref _bindedAction )?.Invoke( value );							break;
			case 1: UnsafeUtility.As<Object, Action<Object, TValue>>				( ref _bindedAction )?.Invoke( State1, value );					break;
			case 2: UnsafeUtility.As<Object, Action<Object, Object, TValue>>		( ref _bindedAction )?.Invoke( State1, State2, value );			break;
			case 3: UnsafeUtility.As<Object, Action<Object, Object, Object, TValue>>( ref _bindedAction )?.Invoke( State1, State2, State3, value );	break;
		}
	}
}

public abstract class TweenBackend
{
	public static TweenBackend Ref = new FlexyTweenService.Runner( );
	
	public	abstract	TweenHandle		Run<T, TLerp>( Builder<T, TLerp> tween ) where T: unmanaged where TLerp : unmanaged, ITweenLerper<T>;
	public	abstract	void			Complete	( TweenHandle handle );
	public	abstract	void			Cancel		( TweenHandle handle );
	public	abstract	Boolean			IsValid		( TweenHandle handle );
	public	abstract	UniTask.Awaiter	Await		( TweenHandle handle );
}