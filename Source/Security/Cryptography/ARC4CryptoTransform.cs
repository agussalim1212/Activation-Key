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
    // Implements a modified version of the RC4 encryption algorithm.
    internal sealed unsafe class ARC4CryptoTransform : ICryptoTransform
    {
        // All LCR multiplier values.
        private static readonly byte[] _A =
        {
            0x09, 0x0D, 0x11, 0x15, 0x19, 0x1d, 0x21, 0x25,
            0x29, 0x2d, 0x31, 0x35, 0x39, 0x3d, 0x41, 0x45,
            0x49, 0x4d, 0x51, 0x55, 0x59, 0x5d, 0x61, 0x65,
            0x69, 0x6d, 0x71, 0x75, 0x79, 0x7d, 0x81, 0x85,
            0x89, 0x8d, 0x91, 0x95, 0x99, 0x9d, 0xa1, 0xa5,
            0xa9, 0xad, 0xb1, 0xb5, 0xb9, 0xbd, 0xc1, 0xc5,
            0xc9, 0xcd, 0xd1, 0xd5, 0xd9, 0xdd, 0xe1, 0xe5,
            0xe9, 0xed, 0xf1, 0xf5, 0xf9
        };

        // All LCR increment values.
        private static readonly byte[] _C =
        {
            0x05, 0x07, 0x0B, 0xD, 0x11, 0x13, 0x17, 0x1d,
            0x1f, 0x25, 0x29, 0x2b, 0x2f, 0x35, 0x3b, 0x3d,
            0x43, 0x47, 0x49, 0x4f, 0x53, 0x59, 0x61, 0x65,
            0x67, 0x6b, 0x6d, 0x71, 0x7f, 0x83, 0x89, 0x8b,
            0x95, 0x97, 0x9d, 0xa3, 0xa7, 0xad, 0xb3, 0xb5,
            0xbf, 0xc1, 0xc5, 0xc7, 0xd3, 0xdf, 0xe3, 0xe5,
            0xe9, 0xef, 0xf1, 0xfb
        };

        // Internal state arrays for RC4.
        private byte[] _s1 = new byte[256];
        private byte[] _s2 = new byte[256];

        // Indices for both state arrays.
        private int _x1, _y1, _x2, _y2;
        private bool _disposed = false;

        // Size of the input data block in bytes.
        public int InputBlockSize => 1;

        // Size of the output data block in bytes.
        public int OutputBlockSize => 1;

        // Indicates whether multiple data blocks can be converted.
        public bool CanTransformMultipleBlocks => true;

        // Indicates whether the transformation cannot be reused.
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
            int a = _A[iv[2] % _A.Length]; // Multiplier.
            int c = _C[iv[3] % _C.Length]; // Increment.

            // Apply the Linear Congruential Transformation.
            for (int i = 0; i < 256; i++)
            {
                sblock[i] = (byte)(r ^ (x = (a * x + c) & 0xFF));
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
            for (int i = 0; i < 256; i++)
            {
                // Performs PRGA operation.
                x = (x + 1) & 0xFF;
                y = (y + sblock[x]) & 0xFF;
                Swap(sblock, x, y);
            }
        }

        // Performs PRGA operation.
        private static void PRGA(byte* sblock, ref int x, ref int y)
        {
            // Perform the swapping of state array values.
            x = (x + 1) & 0xFF;
            y = (y + sblock[x]) & 0xFF;
            Swap(sblock, x, y);
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
