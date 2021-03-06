# TinyMT-CSharp
C# implementation of [TinyMT](https://github.com/MersenneTwister-Lab/TinyMT).

The class `TinyMT.Random` is compatible with the `System.Random`.
So, it can be easily replaced.

## Reference

### Class TinyMT.Random
Tiny Mersenne Twister pseudo-random number generator.

#### Constructors

##### Random()
Initializes a new instance with default seed and parameters.

##### Random(int seed)
Initializes an instance with the specified seed and default parmeters.
###### Parameters
 - `seed`: The seed value of pseudo-random number sequence. If a negative value is specified, the absolute value is used.

##### Random(ulong seed)
Initializes a new instance with the specified seed and default parameters.
###### Parameters
- `seed`: The seed value of pseudo-random number sequence.

##### Random(ulong seed, uint mat1, uint mat2, ulong tmat)
Initializes a new instance with the specified seed and parameters.
###### Parameters
- `seed`: The seed value of pseudo-random number sequence.
- `mat1`: State transition parameter.
- `mat2`: State transition parameter.
- `tmat`: Tempering parameter.

##### Random(ulong[] initKey)
Initializes a new instance with an array of integers used as seeds, and default parameters.
###### Parameters
- `initKey`: The array of 64-bit integers, used as seed.

##### Random(ulong[] initKey, uint mat1, uint mat2, ulong tmat)
Initializes a new instance with an array of integers used as seeds, and parameters.
###### Parameters
- `initKey`: The array of 64-bit integers, used as seed.
- `mat1`: State transition parameter.
- `mat2`: State transition parameter.
- `tmat`: Tempering parameter.

#### Methods

##### int Next()
Returns a non-negative random integer.
###### Returns
A random integer (0 &le; ret &le; `int.MaxValue`).

##### int Next(int maxValue)
Returns a non-negative random integer that is less than the specified maximum.
###### Parameters
- `maxValue`: The exclusive upper bound of the generated random value. this must be greater than or equal to 0.
###### Returns
A random integer (0 &le; ret &lt; `maxValue`).
###### Exceptions
- `ArgumentOutOfRangeException`: `maxValue` is less than 0.


##### int Next(int minValue, int maxValue)
Returns a random integer that is within a specified range.
###### Parameters
- `minValue`: The inclusive lower bound of the generated random value.
- `maxValue`: The exclusive upper bound of the generated random value. this must be greater than or equal to `minValue`
###### Returns
A random integer (`minValue` &le; ret &lt; `maxValue`).
###### Exceptions
`ArgumentOutOfRangeException`: `minValue` is greater than `maxValue`.

##### ulong NextUInt64()
Returns a random 64bit unsigned integer.
###### Returns
A random integer (0 &le; ret &lt; 2^64).

##### void NextBytes(byte[] buffer)
Fills the elements of a specified array of bytes with random numbers.
Each element of the array is set to a random value (0 &le; value &le; `byte.MaxValue`).
###### Parameters
- `buffer`: The array to be filled with random numbers.
###### Exceptions
- `ArgumentNullException`: `buffer` is null.

##### void NextBytes(Span<byte> buffer)
Fills the elements of a specified array of bytes with random numbers.
Each element of the array is set to a random value (0 &le; value &le; byte.MaxValue).
###### Parameters
- `buffer`: The array to be filled with random numbers.

##### double NextDouble()
Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
###### Returns
A double-precision floating point number (0.0 &le; ret &lt; 1.0).

##### double NextDouble01()
Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
###### Returns
A double-precision floating point number (0.0 &le; ret &lt; 1.0).

##### double NextDouble12()
Returns a random floating-point number that is greater than or equal to 1.0, and less than 2.0.
###### Returns
A double-precision floating point number (1.0 &le; ret &lt; 2.0).

##### double NextDoubleOC()
Returns a random floating-point number that is greater than 0.0, and less than or equal to 1.0.
###### Returns
A double-precision floating point number (0.0 &lt; ret &le; 1.0).

##### double NextDoubleOO()
Returns a random floating-point number that is greater than 0.0, and less than 1.0.
###### Returns
A double-precision floating point number (0.0 &lt; ret &lt; 1.0).
