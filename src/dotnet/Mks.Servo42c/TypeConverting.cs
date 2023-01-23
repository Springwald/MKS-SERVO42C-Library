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
    internal static class TypeConverting
    {
        /// <summary>
        /// Converts an unsigned integer 16 to byte array
        /// </summary>
        public static byte[] Uint16ToByteArray(UInt16 intData)
        {
            var bytes = BitConverter.GetBytes(intData);
            Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts an unsigned integer 32 to byte array
        /// </summary>
        public static byte[] Uint32ToByteArray(UInt32 intData)
        {
            var bytes = BitConverter.GetBytes(intData);
            Array.Reverse(bytes);
            return bytes;
        }
    }
}
