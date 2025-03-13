#if LIT_MOTION_PACKAGE

using LitMotion;
using LitMotion.Adapters;
using DelayType = LitMotion.DelayType;

namespace Flexy.Core.Tweens;

public class LitTweenRunner : TweenBackend
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void Init( )
	{
		Ref = new LitTweenRunner( );
	}
	
	public override		TweenHandle		Run<T, TLerp>( Builder<T, TLerp> tween )	
	{
		switch ( tween )
		{
			case Builder<Int32, Int32Lerper> b:		return Run( b );
			case Builder<Single, SingleLerper> b:	return Run( b );
		}	
		
		return default;
	}
	public override		void			Complete	( TweenHandle h )		
	{
		h.ToMotionHandle().Complete();
	}
	public override		void			Cancel		( TweenHandle h )		
	{
		h.ToMotionHandle().Cancel();
	}
	public override		Boolean			IsValid		( TweenHandle h )		
	{
		return h.ToMotionHandle().IsActive();
	}
	public override		UniTask.Awaiter	Await		( TweenHandle h )		
	{
		return h.ToMotionHandle().ToUniTask().GetAwaiter( );
	}

	
	private static TweenHandle Run( Builder<Int32, Int32Lerper> tween )		=> Lit(tween).RunWithoutBinding( ).ToTweenHandle( );
	private static TweenHandle Run( Builder<Single, SingleLerper> tween )	=> Lit(tween).RunWithoutBinding( ).ToTweenHandle( );
	
	private static MotionBuilder<Int32, IntegerOptions, IntMotionAdapter>	Lit( Builder<Int32, Int32Lerper> tween )	=> LitBuilder<Int32, IntegerOptions, IntMotionAdapter, Int32Lerper>(tween);
	private static MotionBuilder<Single, NoOptions, FloatMotionAdapter>		Lit( Builder<Single, SingleLerper> tween )	=> LitBuilder<Single, NoOptions, FloatMotionAdapter, SingleLerper>(tween);
	
	private static MotionBuilder<T, To, Ta> LitBuilder<T, To, Ta, Tl>( Builder<T, Tl> tween ) where T: unmanaged where Tl: unmanaged, ITweenLerper<T> where To: unmanaged, IMotionOptions where Ta:unmanaged, IMotionAdapter<T, To> 
	{
		var builder = LMotion.Create<T, To, Ta>( tween.From, tween.To, tween.DurationMs / 1000.0f )
			.WithEase( (LitMotion.Ease)tween.Ease )
			.WithLoops( tween.LoopsCount, (LoopType)tween.LoopType )
			.WithDelay( tween.DelayMs / 1000.0f, (DelayType)tween.DelayType )
			.WithOnComplete( tween.Callbacks.Completed )
			.WithOnCancel( tween.Callbacks.Canceled );
		//.WithScheduler( MotionScheduler.UpdateIgnoreTimeScale );
		
		builder.buffer.CallbackData.StateCount		= tween.BindData.StateCount;
		builder.buffer.CallbackData.State1			= tween.BindData.State1;
		builder.buffer.CallbackData.State2			= tween.BindData.State2;
		builder.buffer.CallbackData.State3			= tween.BindData.State3;
		builder.buffer.CallbackData.UpdateAction	= tween.BindData.BindedAction;
		
		return builder;
	}
}

public static class LitTweenExtensions
{
	public static TweenHandle	ToTweenHandle	( this MotionHandle h )	=> new() { Id = h.StorageId, SubId = h.Index, Version = h.Version };
	public static MotionHandle	ToMotionHandle	( this TweenHandle h )	=> new() { StorageId = (Int32)h.Id, Index = h.SubId, Version = h.Version };
}

#endif