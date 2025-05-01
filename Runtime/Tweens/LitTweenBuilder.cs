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
		var builder = LMotion.Create<T, To, Ta>( tween.Data.From, tween.Data.To, tween.Data.DurationMs / 1000.0f )
			.WithEase( (LitMotion.Ease)tween.Data.Ease )
			.WithLoops( tween.Data.LoopsCount, (LoopType)tween.Data.LoopType )
			.WithDelay( tween.Data.DelayMs / 1000.0f, (DelayType)tween.Data.DelayType )
			.WithOnComplete( tween.Data.Callbacks.Completed )
			.WithOnCancel( tween.Data.Callbacks.Canceled );
		//.WithScheduler( MotionScheduler.UpdateIgnoreTimeScale );
		
		var bindData = tween.Data.BindData;
		
		switch(tween.Data.BindData.StateCount)
		{
			case 0: builder.Bind((Action<T>)bindData.BindedAction); break;
			case 1: builder.Bind(bindData.State1, (Action<T, Object>)bindData.BindedAction); break;
			case 2: builder.Bind(bindData.State1, bindData.State2, (Action<T, Object, Object>)bindData.BindedAction); break;
			case 3: builder.Bind(bindData.State1, bindData.State2, bindData.State3, (Action<T, Object, Object, Object>)bindData.BindedAction); break;
		}
		
		// builder.buffer.StateCount		= bindData.StateCount;
		// builder.buffer.State0			= bindData.State1;
		// builder.buffer.State1			= bindData.State2;
		// builder.buffer.State2			= bindData.State3;
		// builder.buffer.UpdateAction		= bindData.BindedAction;
		
		return builder;
	}
}

public static class LitTweenExtensions
{
	public static TweenHandle	ToTweenHandle	( this MotionHandle h )	=> new() { Id = h.StorageId, SubId = h.Index, Version = h.Version };
	public static MotionHandle	ToMotionHandle	( this TweenHandle h )	=> new() { StorageId = (Int32)h.Id, Index = h.SubId, Version = h.Version };
}

#endif