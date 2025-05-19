using System.Runtime.CompilerServices;

namespace Flexy.Core;

/// <summary>
/// Squirrel Noise 5
/// </summary>
public static class Noise
{
	public const 	Double 	OneOverMaxUint	= 1.0f / 0xFFFFFFFF;
	public const 	Double 	OneOverMaxInt	= 1.0f / 0x7FFFFFFF;
	private const	Int32 	PRIME1 			= 198491317; // Large prime number with non-boring bits
	private const	Int32 	PRIME2 			= 6542989; // Large prime number with distinct and non-boring bits
	private const	Int32 	PRIME3 			= 357239; // Large prime number with distinct and non-boring bits
	
	[MethodImpl(256)] private static	UInt32	SquirrelNoise5			( Int32 index, UInt32 seed )
	{
		//   From https://www.youtube.com/watch?v=LWFzPP8ZbdU
		//   This version is SquirrelNoise5, which was posted on the author's Twitter: https://twitter.com/SquirrelTweets/status/1421251894274625536.
		//   This is a Unity C# adaptation of SquirrelNoise5 - Squirrel's Raw Noise utilities (version 5).
		//   The following code within this scope is licensed by Squirrel Eiserloh under the Creative Commons Attribution 3.0 license (CC-BY-3.0 US).
		
		const UInt32 SQ5_BIT_NOISE1 = 0xd2a80a3f;	// 11010010101010000000101000111111
		const UInt32 SQ5_BIT_NOISE2 = 0xa884f197;	// 10101000100001001111000110010111
		const UInt32 SQ5_BIT_NOISE3 = 0x6C736F4B;	// 01101100011100110110111101001011
		const UInt32 SQ5_BIT_NOISE4 = 0xB79F3ABB;	// 10110111100111110011101010111011
		const UInt32 SQ5_BIT_NOISE5 = 0x1b56c4f5;	// 00011011010101101100010011110101
		
		var mangledBits = (UInt32) index;
		mangledBits 	*= SQ5_BIT_NOISE1;
		mangledBits 	+= seed;
		mangledBits 	^= (mangledBits  >> 9);
		mangledBits 	+= SQ5_BIT_NOISE2;
		mangledBits 	^= (mangledBits  >> 11);
		mangledBits 	*= SQ5_BIT_NOISE3;
		mangledBits 	^= (mangledBits  >> 13);
		mangledBits 	+= SQ5_BIT_NOISE4;
		mangledBits 	^= (mangledBits  >> 15);
		mangledBits 	+= SQ5_BIT_NOISE5;
		mangledBits 	^= (mangledBits  >> 17);
		
		return mangledBits;
	}
	
	[MethodImpl(256)] public static		UInt32	Get1dNoiseUint			( Int32 index, UInt32 seed ) => SquirrelNoise5( index, seed );
	[MethodImpl(256)] public static		UInt32	Get2dNoiseUint			( Int32 indexX, Int32 indexY, UInt32 seed ) => SquirrelNoise5( indexX + (PRIME1 * indexY), seed );
	[MethodImpl(256)] public static		UInt32  Get3dNoiseUint			( Int32 indexX, Int32 indexY, Int32 indexZ, UInt32 seed ) => SquirrelNoise5( indexX + (PRIME1 * indexY) + (PRIME2 * indexZ), seed );
	[MethodImpl(256)] public static		UInt32  Get4dNoiseUint			( Int32 indexX, Int32 indexY, Int32 indexZ, Int32 indexW, UInt32 seed ) => SquirrelNoise5( indexX + (PRIME1 * indexY) + (PRIME2 * indexZ) + (PRIME3 * indexW), seed );

	[MethodImpl(256)] public static		Single	Get1dNoiseZeroToOne		( Int32 index, UInt32 seed ) => (Single)(OneOverMaxUint * SquirrelNoise5( index, seed ));
	[MethodImpl(256)] public static		Single	Get2dNoiseZeroToOne		( Int32 indexX, Int32 indexY, UInt32 seed ) => (Single)(OneOverMaxUint * Get2dNoiseUint( indexX, indexY, seed ));
	[MethodImpl(256)] public static		Single	Get3dNoiseZeroToOne		( Int32 indexX, Int32 indexY, Int32 indexZ, UInt32 seed ) => (Single)(OneOverMaxUint * Get3dNoiseUint( indexX, indexY, indexZ, seed ));
	[MethodImpl(256)] public static		Single	Get4dNoiseZeroToOne		( Int32 indexX, Int32 indexY, Int32 indexZ, Int32 indexT, UInt32 seed ) => (Single)(OneOverMaxUint * Get4dNoiseUint( indexX, indexY, indexZ, indexT, seed ));
	
	[MethodImpl(256)] public static		Single	Get1dNoiseNegOneToOne	( Int32 index, UInt32 seed ) => (Single)(OneOverMaxInt * (Int32) SquirrelNoise5( index, seed ));
	[MethodImpl(256)] public static		Single	Get2dNoiseNegOneToOne	( Int32 indexX, Int32 indexY, UInt32 seed ) => (Single)(OneOverMaxInt * (Int32) Get2dNoiseUint( indexX, indexY, seed ));
	[MethodImpl(256)] public static		Single	Get3dNoiseNegOneToOne	( Int32 indexX, Int32 indexY, Int32 indexZ, UInt32 seed ) => (Single)(OneOverMaxInt * (Int32) Get3dNoiseUint( indexX, indexY, indexZ, seed ));
	[MethodImpl(256)] public static		Single	Get4dNoiseNegOneToOne	( Int32 indexX, Int32 indexY, Int32 indexZ, Int32 indexT, UInt32 seed ) => (Single)(OneOverMaxInt * (Int32) Get4dNoiseUint( indexX, indexY, indexZ, indexT, seed ));
}

/// <summary>
/// Squirrel Noise Rng
/// </summary>
public struct Rng
{
	public Rng(UInt32 initialState, UInt32 seed) { _state = initialState; _seed = seed; }
	
	private UInt32 _state;
	private UInt32 _seed;

	[MethodImpl(256)] public	Boolean	NextBool	( ) => NextState() > UInt32.MaxValue/2;

	[MethodImpl(256)] public	UInt32	NextUInt	( ) => NextState();
	[MethodImpl(256)] public	Int32	NextInt		( ) => (Int32)NextState();
	[MethodImpl(256)] public	Single	NextFloat	( ) => (Single)(Noise.OneOverMaxUint * NextState());
	
	[MethodImpl(256)] public	UInt32	NextUInt	( UInt32 minInclusive, UInt32 maxExclusive )	=> (UInt32)(NextState() * (UInt64)(maxExclusive - minInclusive) >> 32) + minInclusive;
	[MethodImpl(256)] public	Int32	NextInt		( Int32 minInclusive, Int32 maxExclusive )		=> (Int32)(NextState() * (UInt64)(UInt32)(maxExclusive - minInclusive) >> 32) + minInclusive;
	[MethodImpl(256)] public	Single	NextFloat	( Single minInclusive, Single maxInclusive )	=> NextFloat() * (maxInclusive - minInclusive) + minInclusive;
    
	[MethodImpl(256)] private	UInt32	NextState	( )
    {
	    _state = Noise.Get1dNoiseUint((Int32)_state, _seed);
	    return _state;
    }
}