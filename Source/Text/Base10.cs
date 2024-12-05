/***************************************************************

•   File: Base16Encoding.cs

•   Description.

    Base16Encoding is designed to work with hexadecimal data and
    implements methods for encoding and decoding data.

***************************************************************/

using static System.InternalTools;

namespace System.Text
{
    internal sealed class Base10Encoding : InternalBaseEncoding
    {
        public override string EncodingName => "base-10";

        public Base10Encoding()
          : base(0)
        {
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Validate(chars, charIndex, charCount, bytes, byteIndex);
            charCount = GetCharCount(chars, charIndex, charCount, bytes, byteIndex);
            int startByteIndex = byteIndex;
            int endCharIndex = charIndex + charCount;
            while (charIndex < endCharIndex)
            {
                byte value = (byte)((GetValue(chars[charIndex++]) << 4) + GetValue(chars[charIndex++]));
                bytes[byteIndex++] = value;
            }

            return byteIndex - startByteIndex;
        }

        static string BytesToString(byte[] bytes)
        {
            // Minimum length 1.
            if (bytes.Length == 0) return "0";

            // length <= digits.Length.
            var chars = new byte[(bytes.Length * 0x00026882 + 0xFFFF) >> 16];
            int length = 1;

            // For each byte:
            for (int j = 0; j != bytes.Length; ++j)
            {
                // digits = digits * 256 + data[j].
                int i, carry = bytes[j];
                for (i = 0; i < length || carry != 0; ++i)
                {
                    int value = chars[i] * 256 + carry;
                    carry = value / 10;
                    value %= 10;
                    chars[i] = (byte)value;
                }
                // digits got longer.
                if (i > length) length = i;
            }

            // Return string.
            var result = new StringBuilder(length);
            while (0 != length) result.Append((char)('0' + chars[--length]));
            return result.ToString();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            Validate(bytes, byteIndex, byteCount, chars, charIndex);
            byteCount = GetByteCount(bytes, byteIndex, byteCount, chars, charIndex);
            int startCharIndex = charIndex;
            int endByteIndex = byteIndex + byteCount;
            while (byteIndex < endByteIndex)
            {
                int i;
                int carry = bytes[byteIndex++];
                int length = 1;
                for (i = 0; i < length || carry != 0; ++i)
                {
                    int value = chars[i] * 256 + carry;
                    carry = value / 10;
                    value %= 10;
                    chars[i + charIndex] = (byte)value;
                }

            }
            return charIndex - startCharIndex;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return ((charCount << 16) - 0xFFFF) / 0x00026882;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return (byteCount * 0x00026882 + 0xFFFF) >> 16;
        }

        private static int GetValue(char digit)
        {
            if (digit > 0x2F && digit < 0x3A)
                return digit - 48;
            throw new ArgumentOutOfRangeException(nameof(digit), digit, GetResourceString("Format_BadBase"));
        }

        private static char GeDigit(int value)
        {
            return value + 0x30;
        }

        public override object Clone()
        {
            return new Base16Encoding();
        }

    }
}