using System;

namespace TinyMT
{
    /// <summary>Tiny Mersenne Twister pseudo-random number generator.</summary>
    /// <remarks>
    /// This class is compatible with System.Random.
    /// So, it can be easly replaced.
    /// </remarks>
    /// <example>
    /// using System;
    /// using Random = TinyMT.Random;
    /// </example>
    public class Random
    {
        const int MIN_LOOP = 8;

        const int MEXP = 127;
        const int SH0 = 12;
        const int SH1 = 11;
        const int SH8 = 8;
        const ulong MASK = 0x7fffffffffffffff;
        const double MUL = 1.0d / 9007199254740992.0d;

        public ulong[] status = new ulong[2];

        // From the original unit test (check64.out.txt).
        public uint mat1 = 0xfa051f40;
        public uint mat2 = 0xffd0fff4;
        public ulong tmat = 0x58d02ffeffbfffbc;

        /// <summary>Initializes a new instance with default seed and parameters.</summary>
        /// <remarks>Compatible with System.Random.</remarks>
        public Random()
        {
            init((ulong)Environment.TickCount);
        }

        /// <summary>Initializes an instance with the specified seed and default parmeters.</summary>
        /// <param name="seed">The seed value of pseudo-random number sequence. If a negative value is specified, the absolute value is used.</param>
        /// <remarks>Compatible with System.Random.</remarks>
        public Random(int seed)
        {
            init((seed >= 0) ? (ulong)seed : (ulong)(-seed));
        }

        /// <summary>Initializes a new instance with the specified seed and default parameters.</summary>
        /// <param name="seed">The seed value of pseudo-random number sequence.</param>
        public Random(ulong seed)
        {
            init(seed);
        }

        /// <summary>Initializes a new instance with the specified seed and parameters.</summary>
        /// <param name="seed">The seed value of pseudo-random number sequence.</param>
        /// <param name="mat1">State transition parameter.</param>
        /// <param name="mat2">State transition parameter.</param>
        /// <param name="tmat">Tempering parameter.</param>
        public Random(ulong seed, uint mat1, uint mat2, ulong tmat)
        {
            this.mat1 = mat1;
            this.mat2 = mat2;
            this.tmat = tmat;
            init(seed);
        }

        /// <summary>Initializes a new instance with an array of integers used as seeds, and default parameters.</summary>
        /// <param name="initKey">The array of 64-bit integers, used as seed.</param>
        public Random(ulong[] initKey)
        {
            initByArray(initKey);
        }

        /// <summary>Initializes a new instance with an array of integers used as seeds, and parameters.</summary>
        /// <param name="initKey">The array of 64-bit integers, used as seed.</param>
        /// <param name="mat1">State transition parameter.</param>
        /// <param name="mat2">State transition parameter.</param>
        /// <param name="tmat">Tempering parameter.</param>
        public Random(ulong[] initKey, uint mat1, uint mat2, ulong tmat)
        {
            this.mat1 = mat1;
            this.mat2 = mat2;
            this.tmat = tmat;
            initByArray(initKey);
        }

        /// <summary>Always returns 127.</summary>
        /// <returns>127.</returns>
        /// <remarks>Corresponds to the original function tinymt64_get_mexp.</remarks>
        public int GetMExp() => MEXP;

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A random integer (0 &lt;= ret &lt;= int.MaxValue).</returns>
        /// <remarks>Compatible with System.Random.</remarks>
        public virtual int Next()
        {
            return (int)(NextUInt64() & (ulong)int.MaxValue);
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">The exclusive upper bound of the generated random value. this must be greater than or equal to 0.</param>
        /// <returns>A random integer (0 &lt;= ret &lt; <paramref name="maxValue" />).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
        /// <remarks>Compatible with System.Random.</remarks>
        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "'maxValue' must be greater than zero.");
            }

            return (int)(NextDouble() * maxValue);
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the generated random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the generated random value. this must be greater than or equal to <paramref name="minValue" />.</param>
        /// <returns>A random integer (<paramref name="minValue" /> &lt;= ret &lt; <paramref name="maxValue" />).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue" /> is greater than <paramref name="maxValue" />.</exception>
        /// <remarks>Compatible with System.Random.</remarks>
        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "'minValue' cannot be greater than maxValue.");
            }

            var range = maxValue - minValue;
            return (int)(NextDouble() * range) + minValue;
        }

        /// <summary>Returns a random 64bit unsigned integer.</summary>
        /// <returns>A random integer (0 &lt;= ret &lt; 2^64).</returns>
        /// <remarks>Corresponds to the original function tinymt64_generate_uint64.</remarks>
        public ulong NextUInt64()
        {
            nextState();
            return temper();
        }

        /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <remarks>
        /// Each element of the array is set to a random value (0 &lt;= value &lt;= byte.MaxValue).
        /// Compatible with System.Random.
        /// </remarks>
        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException("buffer");
            }

            NextBytes((Span<byte>)buffer);
        }

        /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        /// <remarks>
        /// Each element of the array is set to a random value (0 &lt;= value &lt;= byte.MaxValue).
        /// Compatible with System.Random.
        /// </remarks>
        public virtual void NextBytes(Span<byte> buffer)
        {
            var i = 0;
            for (; i<buffer.Length / 8; i++)
            {
                var n = NextUInt64();
                for (var j=0; j<8; j++)
                {
                    buffer[i*8+j] = (byte)((n >> (j*8)) & 0xff);
                }
            }

            var r = buffer.Length % 8;
            if (r > 0)
            {
                var n = NextUInt64();
                for (var j=0; j<r; j++)
                {
                    buffer[i*8+j] = (byte)((n >> j*8) & 0xff);
                }
            }
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number (0.0 &lt;= ret &lt; 1.0).</returns>
        /// <remarks>Compatible with System.Random and corresponds to the original function tinymt64_generate_double.</remarks>
        public virtual double NextDouble()
        {
            nextState();
            return (double)(temper() >> 11) * MUL;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number (0.0 &lt;= ret &lt; 1.0).</returns>
        /// <remarks>Corresponds to the original function tinymt64_generate_double01.</remarks>
        public double NextDouble01()
        {
            nextState();
            return temperConv() - 1.0d;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 1.0, and less than 2.0.</summary>
        /// <returns>A double-precision floating point number (1.0 &lt;= ret &lt; 2.0).</returns>
        /// <remarks>Corresponds to the original function tinymt64_generate_double12.</remarks>
        public double NextDouble12()
        {
            nextState();
            return temperConv();
        }

        /// <summary>Returns a random floating-point number that is greater than 0.0, and less than or equal to 1.0.</summary>
        /// <returns>A double-precision floating point number (0.0 &lt; ret &lt;= 1.0).</returns>
        /// <remarks>Corresponds to the original function tinymt64_generate_doubleOC.</remarks>
        public double NextDoubleOC()
        {
            nextState();
            return 2.0d - temperConv();
        }

        /// <summary>Returns a random floating-point number that is greater than 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number (0.0 &lt; ret &lt; 1.0).</returns>
        /// <remarks>Corresponds to the original function tinymt64_generate_doubleOO.</remarks>
        public double NextDoubleOO()
        {
            nextState();
            return temperConvOpen() - 1.0d;
        }

        // original function: init_func1
        ulong iniFunc1(ulong x)
        {
            return (x ^ (x >> 59)) * 2173292883993;
        }

        // original function: init_func2
        ulong iniFunc2(ulong x)
        {
            return (x ^ (x >> 59)) * 58885565329898161;
        }

        // original function: period_certification
        void periodCertification()
        {
            if ((status[0] & MASK) == 0 && status[1] == 0)
            {
                status[0] = 'T';
                status[1] = 'M';
            }
        }

        // original function: tinymt64_init
        void init(ulong seed)
        {
            status[0] = seed ^ ((ulong)mat1 << 32);
            status[1] = mat2 ^ tmat;

            for (var i=1; i<MIN_LOOP; i++)
            {
                status[i & 1] ^= (ulong)i + 6364136223846793005
                    * (status[(i-1)&1] ^ (status[(i-1)&1] >> 62));
            }

            periodCertification();
        }

        // original function: tinymt64_init_by_array
        void initByArray(ulong[] initKey)
        {
            const ulong lag = 1;
            const ulong mid = 1;
            const ulong size = 4;

            ulong keyLength = (ulong)initKey.Length;
            ulong[] st = new ulong[] { 0, mat1, mat2, tmat };

            var count = keyLength + 1;
            if (count < MIN_LOOP)
            {
                count = MIN_LOOP;
            }

            ulong r = iniFunc1(st[0] ^ st[mid % size] ^ st[(size-1) % size]);

            st[mid % size] += r;
            r += keyLength;
            st[(mid + lag) % size] += r;
            st[0] = r;
            count--;

            ulong i, j;
            for (i = 1, j = 0; (j < count) && (j < keyLength); j++) {
                r = iniFunc1(st[i] ^ st[(i + mid) % size] ^ st[(i + size - 1) % size]);
                st[(i + mid) % size] += r;
                r += initKey[j] + i;
                st[(i + mid + lag) % size] += r;
                st[i] = r;
                i = (i + 1) % size;
            }
            for (; j < count; j++) {
                r = iniFunc1(st[i] ^ st[(i + mid) % size] ^ st[(i + size - 1) % size]);
                st[(i + mid) % size] += r;
                r += i;
                st[(i + mid + lag) % size] += r;
                st[i] = r;
                i = (i + 1) % size;
            }
            for (j = 0; j < size; j++) {
                r = iniFunc2(st[i] + st[(i + mid) % size] + st[(i + size - 1) % size]);
                st[(i + mid) % size] ^= r;
                r -= i;
                st[(i + mid + lag) % size] ^= r;
                st[i] = r;
                i = (i + 1) % size;
            }

            status[0] = st[0] ^ st[1];
            status[1] = st[2] ^ st[3];
            periodCertification();
        }

        // original function: tinymt64_next_state
        void nextState()
        {
            ulong x;

            status[0] &= MASK;
            x = status[0] ^ status[1];
            x ^= x << SH0;
            x ^= x >> 32;
            x ^= x << 32;
            x ^= x << SH1;
            status[0] = status[1];
            status[1] = x;
            if ((x & 1) != 0)
            {
                status[0] ^= mat1;
                status[1] ^= (ulong)mat2 << 32;
            }
        }

        // original function: tinymt64_temper
        ulong temper()
        {
            var x = status[0] + status[1];
            x ^= status[0] >> SH8;
            if ((x & 1) != 0)
            {
                x ^= tmat;
            }

            return x;
        }

        // original function: tinymt64_temper_conv
        double temperConv()
        {
            var x = status[0] + status[1];
            x ^= status[0] >> SH8;
            long u;
            if ((x & 1) != 0)
            {
                u = (long)((x ^ tmat) >> 12) | 0x3ff0000000000000;
            }
            else
            {
                u = (long)(x >> 12) | 0x3ff0000000000000;
            }

            return BitConverter.Int64BitsToDouble(u);
        }

        // original function: tinymt64_temper_conv_open
        double temperConvOpen()
        {
            var x = status[0] + status[1];
            x ^= status[0] >> SH8;
            long u;
            if ((x & 1) != 0)
            {
                u = (long)((x ^ tmat) >> 12) | 0x3ff0000000000001;
            }
            else
            {
                u = (long)(x >> 12) | 0x3ff0000000000001;
            }

            return BitConverter.Int64BitsToDouble(u);
        }
    }
}
