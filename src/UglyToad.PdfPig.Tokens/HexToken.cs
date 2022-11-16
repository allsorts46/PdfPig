namespace UglyToad.PdfPig.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A token containing string data where the string is encoded as hexadecimal.
    /// </summary>
    public class HexToken : IDataToken<string>
    {
        private static readonly Dictionary<char, byte> HexMap = new Dictionary<char, byte>
        {
            {'0', 0x00 },
            {'1', 0x01 },
            {'2', 0x02 },
            {'3', 0x03 },
            {'4', 0x04 },
            {'5', 0x05 },
            {'6', 0x06 },
            {'7', 0x07 },
            {'8', 0x08 },
            {'9', 0x09 },

            {'A', 0x0A },
            {'a', 0x0A },
            {'B', 0x0B },
            {'b', 0x0B },
            {'C', 0x0C },
            {'c', 0x0C },
            {'D', 0x0D },
            {'d', 0x0D },
            {'E', 0x0E },
            {'e', 0x0E },
            {'F', 0x0F },
            {'f', 0x0F }
        };

        /// <summary>
        /// The string contained in the hex data.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// The bytes of the hex data.
        /// </summary>
        public IReadOnlyList<byte> Bytes { get; }

        /// <summary>
        /// Create a new <see cref="HexToken"/> from the provided bytes.
        /// </summary>
        /// <param name="bytes">Byte array data.</param>
        public HexToken(Byte[] bytes)
            : this(String.Concat(bytes.Select(b => b.ToString("X2")).ToArray()).ToCharArray())
            { 
            }

        /// <summary>
        /// Create a new <see cref="HexToken"/> from the provided hex characters.
        /// </summary>
        /// <param name="characters">A set of hex characters 0-9, A - F, a - f representing a string.</param>
        public HexToken(IReadOnlyList<char> characters)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            var bytes = new List<byte>();

            for (var i = 0; i < characters.Count; i += 2)
            {
                char high = characters[i];
                char low;
                if (i == characters.Count - 1)
                {
                    low = '0';
                }
                else
                {
                    low = characters[i + 1];
                }

                var b = Convert(high, low);
                bytes.Add(b);
            }

            // Handle UTF-16BE format strings.
            if (bytes.Count >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                Data = Encoding.BigEndianUnicode.GetString(bytes.ToArray(), 2, bytes.Count - 2);
            }
            else
            {
                var builder = new StringBuilder();

                foreach (var b in bytes)
                {
                    if (b != '\0')
                    {
                        builder.Append((char)b);
                    }
                }

                Data = builder.ToString();
            }

            Bytes = bytes;
        }

        /// <summary>
        /// Convert two hex characters to a byte.
        /// </summary>
        /// <param name="high">The high nibble.</param>
        /// <param name="low">The low nibble.</param>
        /// <returns>The byte.</returns>
        public static byte Convert(char high, char low)
        {
            var highByte = HexMap[high];
            var lowByte = HexMap[low];

            return (byte)(highByte << 4 | lowByte);
        }

        /// <summary>
        /// Convert the bytes in this hex token to an integer.
        /// </summary>
        /// <param name="token">The token containing the data to convert.</param>
        /// <returns>The integer corresponding to the bytes.</returns>
        public static int ConvertHexBytesToInt(HexToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var bytes = token.Bytes;

            var value = bytes[0] & 0xFF;
            if (bytes.Count == 2)
            {
                value <<= 8;
                value += bytes[1] & 0xFF;
            }

            return value;
        }

        /// <inheritdoc />
        public bool Equals(IToken obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is HexToken other))
            {
                return false;
            }

            return Data == other.Data;
        }

        /// <summary>
        /// Converts the binary data back to a hex string representation.
        /// </summary>
        public string GetHexString()
        {
            return BitConverter.ToString(Bytes.ToArray()).Replace("-", string.Empty);
        }
    }
}