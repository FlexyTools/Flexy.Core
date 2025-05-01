using Unity.Burst;
using static Unity.Mathematics.math;

namespace Flexy.Core.Tweens;

public enum Ease : Byte
{
	Linear,
	
	InSine,
	OutSine,
	InOutSine,
	
	InQuad,
	OutQuad,
	InOutQuad,
	
	InCubic,
	OutCubic,
	InOutCubic,
	
	InQuart,
	OutQuart,
	InOutQuart,
	
	InQuint,
	OutQuint,
	InOutQuint,
	
	InExpo,
	OutExpo,
	InOutExpo,
	
	InCirc,
	OutCirc,
	InOutCirc,
	
	InElastic,
	OutElastic,
	InOutElastic,
	
	InBack,
	OutBack,
	InOutBack,
	
	InBounce,
	OutBounce,
	InOutBounce,
	
	CustomAnimationCurve
}


[BurstCompile]
public static class EaseUtility
{
	public static readonly Ease Default = Ease.InOutSine;
	
	[BurstCompile]
	public static Single Evaluate( this Ease ease, Single t )
	{
		return ease switch
		{
			Ease.Linear		=> Linear		( t ),
			Ease.InSine		=> InSine		( t ),
			Ease.OutSine	=> OutSine		( t ),
			Ease.InOutSine	=> InOutSine	( t ),
			Ease.InQuad		=> InQuad		( t ),
			Ease.OutQuad	=> OutQuad		( t ),
			Ease.InOutQuad	=> InOutQuad	( t ),
			Ease.InCubic	=> InCubic		( t ),
			Ease.OutCubic	=> OutCubic		( t ),
			Ease.InOutCubic	=> InOutCubic	( t ),
			Ease.InQuart	=> InQuart		( t ),
			Ease.OutQuart	=> OutQuart		( t ),
			Ease.InOutQuart	=> InOutQuart	( t ),
			Ease.InQuint	=> InQuint		( t ),
			Ease.OutQuint	=> OutQuint		( t ),
			Ease.InOutQuint	=> InOutQuint	( t ),
			Ease.InExpo		=> InExpo		( t ),
			Ease.OutExpo	=> OutExpo		( t ),
			Ease.InOutExpo	=> InOutExpo	( t ),
			Ease.InCirc		=> InCirc		( t ),
			Ease.OutCirc	=> OutCirc		( t ),
			Ease.InOutCirc	=> InOutCirc	( t ),
			Ease.InElastic	=> InElastic	( t ),
			Ease.OutElastic	=> OutElastic	( t ),
			Ease.InOutElastic => InOutElastic( t ),
			Ease.InBack		=> InBack		( t ),
			Ease.OutBack	=> OutBack		( t ),
			Ease.InOutBack	=> InOutBack	( t ),
			Ease.InBounce	=> InBounce		( t ),
			Ease.OutBounce	=> OutBounce	( t ),
			Ease.InOutBounce => InOutBounce	( t ),
			_ => Evaluate( Default, t ),
		};
	}
	
	const Single c1 = 1.70158f;
	const Single c2 = c1 * 1.525f;
	const Single c3 = c1 + 1;
	const Single c4 = 2 * PI / 3;
	const Single c5 = 2 * PI / 4.5f;
	const Single n1 = 7.5625f; 
	const Single d1 = 2.75f;
	
	[BurstCompile] public static Single     SCurve		( Single t )    => 3*t*t - 2*t*t*t;
	[BurstCompile] public static Single     Linear		( Single t )    => t;
	[BurstCompile] public static Single     InSine		( Single t )    => 1 - cos(t * PI / 2);
	[BurstCompile] public static Single     OutSine		( Single t )    => sin(t * PI / 2);
	[BurstCompile] public static Single     InOutSine	( Single t )    => -(cos(PI * t) - 1) / 2;
	[BurstCompile] public static Single     InQuad		( Single t )    => t * t;
	[BurstCompile] public static Single     OutQuad		( Single t )    => 1 - (1 - t) * (1 - t);
	[BurstCompile] public static Single     InOutQuad	( Single t )    => t < 0.5f ? 2 * t * t : 1 - pow(-2 * t + 2, 2) / 2;
	[BurstCompile] public static Single     InCubic		( Single t )    => t * t * t;
	[BurstCompile] public static Single     OutCubic	( Single t )    => 1 - pow(1 - t, 3);
	[BurstCompile] public static Single     InOutCubic	( Single t )    => t < 0.5f ? 4 * t * t * t : 1 - pow(-2 * t + 2, 3) / 2;
	[BurstCompile] public static Single     InQuart		( Single t )    => t * t * t * t;
	[BurstCompile] public static Single     OutQuart	( Single t )    => 1 - pow(1 - t, 4);
	[BurstCompile] public static Single     InOutQuart	( Single t )    => t < 0.5 ? 8 * t * t * t * t : 1 - pow(-2 * t + 2, 4) / 2;
	[BurstCompile] public static Single     InQuint		( Single t )    => t * t * t * t * t;
	[BurstCompile] public static Single     OutQuint	( Single t )    => 1 - pow(1 - t, 5);
	[BurstCompile] public static Single     InOutQuint	( Single t )    => t < 0.5f ? 16 * t * t * t * t * t : 1 - pow(-2 * t + 2, 5) / 2;
	[BurstCompile] public static Single     InExpo		( Single t )    => t == 0 ? 0 : pow(2, 10 * t - 10);
	[BurstCompile] public static Single     OutExpo		( Single t )    => t == 1 ? 1 : 1 - pow(2, -10 * t);
	[BurstCompile] public static Single     InOutExpo	( Single t )    => t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? pow(2, 20 * t - 10) / 2 : (2 - pow(2, -20 * t + 10)) / 2;
	[BurstCompile] public static Single     InCirc		( Single t )    => 1 - sqrt(1 - pow(t, 2));
	[BurstCompile] public static Single     OutCirc		( Single t )    => sqrt(1 - pow(t - 1, 2));
	[BurstCompile] public static Single     InOutCirc	( Single t )	=> t < 0.5 ? (1 - sqrt(1 - pow(2 * t, 2))) / 2 : (sqrt(1 - pow(-2 * t + 2, 2)) + 1) / 2;
	[BurstCompile] public static Single     InBack		( Single t )	=> c3 * t * t * t - c1 * t * t;
	[BurstCompile] public static Single     OutBack		( Single t )	=> 1 + c3 * pow(t - 1, 3) + c1 * pow(t - 1, 2);
	[BurstCompile] public static Single     InOutBack	( Single t )	=> t < 0.5f ? pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2) / 2 : (pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
	[BurstCompile] public static Single     InElastic	( Single t )	=> t == 0 ? 0 : t == 1 ? 1 : -pow(2, 10 * t - 10) * sin((t * 10 - 10.75f) * c4);
	[BurstCompile] public static Single     OutElastic	( Single t )	=> t == 0 ? 0 : t == 1 ? 1 : pow(2, -10 * t) * sin((t * 10 - 0.75f) * c4) + 1;
	[BurstCompile] public static Single     InOutElastic( Single t )	=> t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? -(pow(2, 20 * t - 10) * sin((20 * t - 11.125f) * c5)) / 2 : pow(2, -20 * t + 10) * sin((20 * t - 11.125f) * c5) / 2 + 1;
	[BurstCompile] public static Single     InBounce	( Single t )    => 1 - OutBounce(1 - t);
	[BurstCompile] public static Single     OutBounce	( Single t )	=> t < 1/d1 ? n1*t*t : t<2/d1 ? n1*(t-=1.5f/d1)*t+0.75f : t<2.5/d1 ? n1*(t-=2.25f/d1)*t+0.9375f : n1*(t-=2.625f/d1)*t+0.984375f;
	[BurstCompile] public static Single     InOutBounce	( Single t )	=> t < 0.5f ? (1 - OutBounce(1 - 2 * t)) / 2 : (1 + OutBounce(2 * t - 1)) / 2;
	
	
	/// <summary>
	/// An interpolation using ease in and ease out. If easeIn and easeOut are both zero then the interpolation is nearly identical to Lerp.
	/// https://www.desmos.com/calculator/bim76l9s5u
	/// <para>Both easeIn and easeOut are clamped between 0f and 1f.</para>
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <param name="easeIn">If 0, no easing is added to the beginning of the interpolation. More easing is added the closer the value is to 1.</param>
	/// <param name="easeOut">If 0, no easing is added to the end of the interpolation. More easing is added the closer the value is to 1.</param>
	/// <param name="t"></param>
	/// <returns></returns>
	public static float Earp(float start, float end, float easeIn, float easeOut, float t)
	{
		easeIn = Mathf.Clamp(easeIn, 0.001f, 0.999f);
		easeOut = Mathf.Clamp(easeOut, 0.001f, 0.999f);
		float e = easeOut + easeIn;

		float z = easeOut / (easeIn + easeOut);
		z = Mathf.Clamp(z, 0.001f, 0.999f);

		float y = (-((1f - z) / (z * Mathf.Pow(0.5f + t, e * (7.5f + (5f * t))) - z + 1f)) + ((1f - z) / (z * Mathf.Pow(1.5f, e * (12.5f)) - z + 1f))) / (((1f - z) / (z * Mathf.Pow(1.5f, e * (12.5f)) - z + 1f)) - ((1f - z) / (z * Mathf.Pow(0.5f, e * (7.5f)) - z + 1f)));

		return start + ((end - start) * y);
	}
	
	
	/// <summary>
	/// A custom interpolation that oscillates around the end value to create a bounce effect. A larger 'frequency' value increases both the number and magnitude of the oscillations.
	/// https://www.desmos.com/calculator/jd06nf9u8s
	/// https://discord.com/channels/489222168727519232/530716058957643796/1157160260050636900
	/// </summary>
	/// <returns></returns>
	public static float Berp(float start, float end, float time, float frequency = 2.7f)
	{
		frequency = Mathf.Clamp(frequency, 0.0f, 50.0f);
		float b = frequency;

		float a = 2.2f - (0.01156f * (frequency - 2.7f));
		float c = 1.2f + (0.01156f * (frequency - 2.7f));

		time = Mathf.Clamp01(time);
		time = (time + Mathf.Pow(1.0f - time, a) * Mathf.Sin(Mathf.PI * b * time * time * time * time)) * (1.0f + c - (c * time));

		return start + (end - start) * time;
	}
	
	public static Single	EaseFunc	( Single t, EaseType type )
	{
		switch (type)
		{
			case EaseType.Linear: return t;
			case EaseType.Sqrt: return (Single)Math.Sqrt(t);
			case EaseType.Sqr: return 1 - (Single)Math.Pow(t - 1, 2);
			case EaseType.Qubic: return (Single)Math.Pow(t - 1, 3) + 1;
			case EaseType.Quad: return 1 - (Single)Math.Pow(t - 1, 4);
			case EaseType.SqrInSqrOut: if (t < 0.5f) return 2 * (Single)Math.Pow(t, 2); return 1 - 2 * (Single)Math.Pow(t - 1, 2); 
			case EaseType.QubicInQubicOut: if (t < 0.5f) return 4 * (Single)Math.Pow(t, 3); return 4 * (Single)Math.Pow(t - 1, 3) + 1;
			case EaseType.LinearInverse: return EaseFunc(t, EaseType.Linear); 
			case EaseType.NormalDistribution5: var x = 10*(t - 0.5f); return (float)(Math.Exp(-x*x/2)/Math.Sqrt(2*Math.PI));
		}
		return EaseFunc(t, EaseType.Linear);
	}
	
	public enum EaseType
	{
		Linear,
		Sqrt,
		Sqr,
		Qubic,
		Quad,
		QubicInQubicOut,
		SqrInSqrOut,
		LinearInverse,
		NormalDistribution5,
	}
	
	// https://www.youtube.com/watch?v=bFOAipGJGA0
	// https://www.ryanjuckett.com/damped-springs/
}