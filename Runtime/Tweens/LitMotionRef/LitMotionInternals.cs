#if LITMOTION_SUPPORT_UNITASK 
using System;

namespace LitMotion;

public static class LitMotionInternals 
{
	public static void SetLitMotionBuilderBind<T, To, Ta>(MotionBuilder<T, To, Ta> builder, Byte bindDataStateCount, Object bindDataState1, Object bindDataState2, Object bindDataState3, Object bindDataBindedAction) where T : unmanaged where To : unmanaged, IMotionOptions where Ta : unmanaged, IMotionAdapter<T, To>
	{
		builder.buffer.StateCount		= bindDataStateCount;
		builder.buffer.State0			= bindDataState1;
		builder.buffer.State1			= bindDataState2;
		builder.buffer.State2			= bindDataState3;
		builder.buffer.UpdateAction		= bindDataBindedAction;
	}
}
#endif