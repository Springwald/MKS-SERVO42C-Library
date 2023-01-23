// Control MKSServo42c Servos via serial port
// https://github.com/Springwald/MKS-SERVO42C-Library
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace Mks.Servo42c
{
    internal static class HelperExtensions
    {
        /// <summary>
        /// Move unsigned values in the middle of their number range to provide negative, not jumping values
        /// </summary>
        /// <param name="valueUint"></param>
        /// <returns></returns>
        public static int FixOverflow(this UInt32 valueUint)
        {
            Int64 value = (Int64)(valueUint);
            return (int)(value > UInt32.MaxValue / 2 ? value -= UInt32.MaxValue : value);
        }

        /// <summary>
        /// Only take some bytes of the array
        /// </summary>
        public static byte[] SkipAddressByte(this byte[] bytes, byte length)
        {
            if (bytes.Length <= length)// to short byte array - seems to be a transport error
            {
                var lengthNew = bytes.Length - 1;
                if (lengthNew < 1) return bytes; // to short byte array - seems to be a transport error, return original array
                length = (byte)lengthNew;
            }

            var resultBytes = new byte[length];
            Array.Copy(bytes, 1, resultBytes, 0, length);
            return resultBytes;
        }


        /// <summary>
        /// Convert byte-array to integer 16
        /// </summary>
        public static Int16 ToInt16(this byte[] bytes)
        {
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Convert byte-array to integer 32
        /// </summary>
        public static Int32 ToInt32(this byte[] bytes)
        {
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Convert byte-array to unsigned integer 16
        /// </summary>
        public static UInt16 ToUInt16(this byte[] bytes)
        {
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Convert byte-array to unsigned integer 32
        /// </summary>
        public static UInt32 ToUInt32(this byte[] bytes)
        {
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

    }


}
