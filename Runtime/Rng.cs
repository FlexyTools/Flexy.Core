namespace Flexy.Core;

public static class Noise
{
	public static UInt32 SquirrelNoise5	( Int32 val, UInt32 seed )
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
		
		var mangledBits = (UInt32) val;
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
}

public struct Rng
{
	UInt32 _state;
	UInt32 _seed;

	public Rng(UInt32 initialState, UInt32 seed)
	{
		_state = initialState;
		_seed = seed;
	}
	
	UInt32 NextState()
	{
		_state = Noise.SquirrelNoise5((Int32)_state, _seed);
		return _state;
	}
}