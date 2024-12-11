/***************************************************************

•   File: ARC4CryptoTransform.cs

•   Description.

   ARC4CryptoTransform  implements and    provides methods  for
   encrypting   and  decrypting    data using  a   modified RC4
   algorithm. Used by default    to  generate  activation keys.

   Despite  the known vulnerabilities  of RC4, such  as leaking
   key information and the possibility of attacking  weak keys,
   it is sufficient for the purposes of this project, since the
   length of the encrypted  data,  as a rule, does not exceed a
   few    bytes. To    improve    cryptographic   strength,  an
   initialization vector and  skipping the first 512 bytes were
   implemented.

***************************************************************/

using static System.InternalTools;

namespace System.Security.Cryptography
{
    // Implements a modified version of the RC4+ encryption algorithm.
    internal sealed unsafe class ARC4CryptoTransform : ICryptoTransform
    {
        // All LCR multiplier values.
        private static readonly byte[] _A =
        {
            0x01, 0x05, 0x09, 0x0D, 0x11, 0x15, 0x19, 0x1D,
            0x21, 0x25, 0x29, 0x2D, 0x31, 0x35, 0x39, 0x3D,
            0x41, 0x45, 0x49, 0x4D, 0x51, 0x55, 0x59, 0x5D,
            0x61, 0x65, 0x69, 0x6D, 0x71, 0x75, 0x79, 0x7D,
            0x81, 0x85, 0x89, 0x8D, 0x91, 0x95, 0x99, 0x9D,
            0xA1, 0xA5, 0xA9, 0xAD, 0xB1, 0xB5, 0xB9, 0xBD,
            0xC1, 0xC5, 0xC9, 0xCD, 0xD1, 0xD5, 0xD9, 0xDD,
            0xE1, 0xE5, 0xE9, 0xED, 0xF1, 0xF5, 0xF9, 0xFD
        };

        // All LCR increment values.
        private static readonly byte[] _C =
        {
            0x01, 0x03, 0x05, 0x07, 0x09, 0x0B, 0x0D, 0x0F,
            0x11, 0x13, 0x15, 0x17, 0x19, 0x1B, 0x1D, 0x1F,
            0x21, 0x23, 0x25, 0x27, 0x29, 0x2B, 0x2D, 0x2F,
            0x31, 0x33, 0x35, 0x37, 0x39, 0x3B, 0x3D, 0x3F,
            0x41, 0x43, 0x45, 0x47, 0x49, 0x4B, 0x4D, 0x4F,
            0x51, 0x53, 0x55, 0x57, 0x59, 0x5B, 0x5D, 0x5F,
            0x61, 0x63, 0x65, 0x67, 0x69, 0x6B, 0x6D, 0x6F,
            0x71, 0x73, 0x75, 0x77, 0x79, 0x7B, 0x7D, 0x7F,
            0x81, 0x83, 0x85, 0x87, 0x89, 0x8B, 0x8D, 0x8F,
            0x91, 0x93, 0x95, 0x97, 0x99, 0x9B, 0x9D, 0x9F,
            0xA1, 0xA3, 0xA5, 0xA7, 0xA9, 0xAB, 0xAD, 0xAF,
            0xB1, 0xB3, 0xB5, 0xB7, 0xB9, 0xBB, 0xBD, 0xBF,
            0xC1, 0xC3, 0xC5, 0xC7, 0xC9, 0xCB, 0xCD, 0xCF,
            0xD1, 0xD3, 0xD5, 0xD7, 0xD9, 0xDB, 0xDD, 0xDF,
            0xE1, 0xE3, 0xE5, 0xE7, 0xE9, 0xEB, 0xED, 0xEF,
            0xF1, 0xF3, 0xF5, 0xF7, 0xF9, 0xFB, 0xFD, 0xFF
        };

        // Internal state arrays for RC4.
        private byte[] _s1 = new byte[256];
        private byte[] _s2 = new byte[256];

        // Indices for both state arrays.
        private int _x1, _y1, _x2, _y2;
        private bool _disposed = false;

        // Size of the input data block in bits.
        public int InputBlockSize => 1;

        // Size of the output data block in bits.
        public int OutputBlockSize => 1;

        // Indicates whether multiple data blocks can be converted.
        public bool CanTransformMultipleBlocks => true;

        // Indicates whether the transformation can be reused.
        public bool CanReuseTransform => false;

        public ARC4CryptoTransform(byte[] key, byte[] iv)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), GetResourceString("ArgumentNull_Array"));
            if (iv == null)
                throw new ArgumentNullException(nameof(iv), GetResourceString("ArgumentNull_Array"));
            if (iv.Length < 4)
                throw new ArgumentException(GetResourceString("Cryptography_InvalidIVSize"), nameof(iv));

            byte* ivPtr = stackalloc byte[iv.Length];
            for (int i = 0; i < iv.Length; i++)
            {
                ivPtr[i] = iv[i];
            }

            // Perform Linear Congruential Random (LCR) generating and Key Scheduling Algorithm (KSA) on both state arrays.
            fixed (byte* s1Ptr = _s1, s2Ptr = _s2, keyPtr = key)
            {
                /***** Apply the LCR operation. *****/
                LCR(s1Ptr, ivPtr);

                // Shift the IV for the second state array.
                for (int i = 0; i < 4; i++)
                    ivPtr[i] = (byte)((ivPtr[i] + 128) & 0xFF);

                // Rotate the IV for further modification.
                byte swap = ivPtr[0];
                for (int i = 0; i < 3; i++)
                    ivPtr[i] = ivPtr[i + 1];
                ivPtr[3] = swap;
                LCR(s2Ptr, ivPtr);

                /***** Apply the KSA operation. *****/
                int keyLength = key.Length;
                KSA(s1Ptr, keyPtr, keyLength, ref _x1, ref _y1);
                KSA(s2Ptr, keyPtr, keyLength, ref _x2, ref _y2);
            }

            // Initialize indices for the state arrays.
            _x1 = _y1 = _x2 = _y2 = 0;
        }

        public ARC4CryptoTransform(byte[] key, int seed) : this(key, BitConverter.GetBytes(seed))
        {
        }

        public ARC4CryptoTransform(byte[] key, uint seed) : this(key, BitConverter.GetBytes(seed))
        {
        }

        // Swap state array values.
        private static void Swap(byte* array, int x, int y)
        {
            if (x != y)
            {
                array[x] ^= array[y];
                array[y] ^= array[x];
                array[x] ^= array[y];
            }
        }

        // The Linear Congruential Generator (LCR) operation used in RC4.
        private static void LCR(byte* sblock, byte* iv)
        {
            // Extract the initialization vector (IV) values.
            int r = iv[0]; // Nolinear transformation value.
            int x = iv[1]; // First value.
            int a = _A[iv[2] & 0x3F]; // Multiplier.
            int c = _C[iv[3] & 0x7F]; // Increment.
            int s = (byte)(((iv[2] >> 6) & 0b11) | ((iv[3] >> 7) & 0b01)); // Shift.

            // Apply the Linear Congruential Transformation.
            for (int i = 0; i < 256; i++)
            {
                int b = (x = (a * x + c) & 0xFF) ^ r;
                sblock[i] = (byte)((b << s) | (b >> (8 - s)));
            }
        }

        // The Key Scheduling Algorithm (KSA) used in RC4 to initialize the state array.
        private static void KSA(byte* sblock, byte* key, int keyLength, ref int x, ref int y)
        {
            if (keyLength < 1)
                return;

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + sblock[i] + key[i % keyLength]) % 256;
                Swap(sblock, i, j);
            }

            // Skip the first 256 bytes to reduce correlation with the key.
            PRGA(sblock, ref x, ref y, 256);
        }

        // Performs PRGA operation.
        private static void PRGA(byte* sblock, ref int x, ref int y)
        {
            // Perform the swapping of state array values.
            x = (x + 1) & 0xFF;
            y = (y + sblock[x]) & 0xFF;
            Swap(sblock, x, y);
        }

        private static void PRGA(byte* sblock, ref int x, ref int y, int n)
        {
            for (int i = 0; i < n; i++)
            {
               x = (x + 1) & 0xFF;
               y = (y + sblock[x]) & 0xFF;
               Swap(sblock, x, y);
            }
        }

        // Converts a block of data using the RC4 algorithm.
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            CheckDisposed();
            CheckBufer(inputBuffer, inputOffset, inputCount);
            CheckBufer(outputBuffer, outputOffset);

            fixed (byte* s1Ptr = _s1, s2Ptr = _s2)
            {
                for (int i = 0; i < inputCount; i++)
                {
                    // Generate an intermediate key bytes using PRGA operation.
                    PRGA(s1Ptr, ref _x1, ref _y1);
                    byte k1 = s1Ptr[(s1Ptr[_x1] + s1Ptr[_y1]) & 0xFF];
                    PRGA(s2Ptr, ref _x2, ref _y2);
                    byte k2 = s2Ptr[(s2Ptr[_x2] + s2Ptr[_y2]) & 0xFF];

                    // Combine the two key bytes using additional nonlinear transformations.
                    byte k = (byte)((k1 + k2) ^ ((k1 << 5) | (k2 >> 3)));

                    // XOR the key byte with the input to produce the output.
                    outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ k);
                }
            }

            return inputCount;
        }

        // Converts the last block of data using the RC4 algorithm.
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            CheckDisposed();
            CheckBufer(inputBuffer, inputOffset, inputCount);

            byte[] finalBlock = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, finalBlock, 0);
            return finalBlock;
        }

        private void CheckBufer(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), GetResourceString("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    GetResourceString("Argument_InvalidValue"));
        }

        private void CheckBufer(byte[] buffer, int offset, int count)
        {
            CheckBufer(buffer, offset);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), offset,
                    GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count), count, GetResourceString("Argument_InvalidValue"));
            if (buffer.Length - count < offset)
                throw new ArgumentException(GetResourceString("Argument_InvalidOffLen"));
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ARC4CryptoTransform), GetResourceString("ObjectDisposed_Generic"));
        }

        // Releases resources used by the class.
        public void Dispose()
        {
            _s1.Clear();
            _s2.Clear();

            _s1 = null;
            _s2 = null;

            _x1 = _y1 = _x2 = _y2 = 0;

            _disposed = true;
        }
    }
}
