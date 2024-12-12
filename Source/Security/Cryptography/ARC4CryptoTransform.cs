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
            int a = ((iv[2] & 0x3F) << 2) | 1; // Multiplier.
            int c = ((iv[3] & 0x7F) << 1) | 1; // Increment.
            int s = (byte)(((iv[2] & 0xC0) >> 5) | ((iv[3] & 0x80) >> 7)); // Shift.

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
