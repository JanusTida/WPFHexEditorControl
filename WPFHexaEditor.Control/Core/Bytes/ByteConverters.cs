//////////////////////////////////////////////
// MIT License  - 2016-2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WPFHexaEditor.Core.Bytes
{
    /// <summary>
    /// ByteCharConverter for convert data
    /// </summary>
    public class ByteConverters
    {
        /// <summary>
        /// Convert long to hex value
        /// </summary>
        public static string LongToHex(long l)
        {
            return l.ToString(ConstantReadOnly.HexLineInfoStringFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert Byte to Char (can be used as visible text)
        /// </summary>
        /// <remarks>
        /// Code from : https://github.com/pleonex/tinke/blob/master/Be.Windows.Forms.HexBox/ByteCharConverters.cs
        /// </remarks>
        public static char ByteToChar(byte b)
        {
            return b > 0x1F && !(b > 0x7E && b < 0xA0) ? (char)b : '.';
        }

        /// <summary>
        /// Convert Char to Byte
        /// </summary>
        public static byte CharToByte(char c)
        {
            return (byte)c;
        }

        /// <summary>
        /// Converts a byte array to a hex string. For example: {10,11} = "0A 0B"
        /// </summary>
        public static string ByteToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in data)
            {
                string hex = ByteToHex(b);
                sb.Append(hex);
                sb.Append(" ");
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }


        /// <summary>
        /// Convert a byte to a hex char arrary,This method may not be called multitimes in a moment.
        /// cuz the hexbyteArray is static.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static readonly char[] hexbyteArray = new char[2];
        public static char[] ByteToHexCharArray(byte b) {
            var hexbyteArray = new char[2];
            hexbyteArray[0] = ByteToHexChar(b >> 4);
            hexbyteArray[1] = ByteToHexChar(b - ((b >> 4) << 4));
            return hexbyteArray;
        }
        //Convert a byte to Hex char,i.e,10 = 'A'
        public static char ByteToHexChar(int b) {
            if (b < 10) {
                return (char)(48 + b);
            }
            switch (b) {
                case 10:
                    return 'A';
                case 11:
                    return 'B';
                case 12:
                    return 'C';
                case 13:
                    return 'D';
                case 14:
                    return 'E';
                case 15:
                    return 'F';
                default:
                    return 's';
            }
        }


        /// <summary>
        /// Converts the byte to a hex string. For example: "10" = "0A";
        /// </summary>
        public static string ByteToHex(byte b)
        {
            string sB = b.ToString(ConstantReadOnly.HexStringFormat, CultureInfo.InvariantCulture);

            if (sB.Length == 1)
                sB = "0" + sB;

            return sB;
        }

        /// <summary>
        /// Converts the byte to a hex string. For example: "10" = "0A";
        /// </summary>
        public static string ByteToHex2(byte b) {
            var chArr = ByteToHexCharArray(b);
            return new string(chArr);
        }
        
        

        /// <summary>
        /// Convert byte to ASCII string
        /// </summary>
        public static string BytesToString(byte[] buffer, ByteToString converter = ByteToString.ByteToCharProcess)
        {
            switch (converter)
            {
                case ByteToString.ASCIIEncoding:
                    return Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                case ByteToString.ByteToCharProcess:
                    StringBuilder builder = new StringBuilder();

                    foreach (byte @byte in buffer)
                        builder.Append(ByteToChar(@byte));

                    return builder.ToString();
            }

            return "";
        }

        /// <summary>
        /// Converts the hex string to an byte array. The hex string must be separated by a space char ' '. If there is any invalid hex information in the string the result will be null.
        /// </summary>
        public static byte[] HexToByte(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;
            hex = hex.Trim();
            var hexArray = hex.Split(' ');
            var byteArray = new byte[hexArray.Length];

            for (int i = 0; i < hexArray.Length; i++)
            {
                var hexValue = hexArray[i];

                byte b;
                var isByte = HexToByte(hexValue, out b);
                if (!isByte)
                    return null;
                byteArray[i] = b;
            }

            return byteArray;
        }

        public static bool HexToByte(string hex, out byte b)
        {
            return byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
        }

        public static long HexLiteralToLong(string hex)
        {
            if (string.IsNullOrEmpty(hex)) throw new ArgumentException("hex");

            int i = hex.Length > 1 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X') ? 2 : 0;
            long value = 0;

            while (i < hex.Length)
            {
                int x = hex[i++];

                if
                    (x >= '0' && x <= '9') x = x - '0';
                else if
                    (x >= 'A' && x <= 'F') x = (x - 'A') + 10;
                else if
                    (x >= 'a' && x <= 'f') x = (x - 'a') + 10;
                else
                    throw new ArgumentOutOfRangeException("hex");

                value = 16 * value + x;
            }

            return value;
        }

        public static long DecimalLiteralToLong(string hex) {
            long value = 0;
            if (long.TryParse(hex, out value)) {
                return value;
            }
            else {
                throw new ArgumentException("hex");
            }
        }

        /// <summary>
        /// Check if is an hexa string
        /// </summary>
        /// <param name="hexastring"></param>
        /// <returns></returns>
        public static bool IsHexaValue(string hexastring)
        {
            long position = 0;
            try
            {
                position = HexLiteralToLong(hexastring);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert string to byte array
        /// </summary>
        public static byte[] StringToByte(string str)
        {
            List<byte> byteList = new List<byte>();

            foreach (char c in str)
                byteList.Add(CharToByte(c));

            return byteList.ToArray();
        }

        /// <summary>
        /// Convert String to hex string For example: "barn" = "62 61 72 6e"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToHex(string str)
        {
            return ByteToHex(StringToByte(str));
        }
    }
}