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
    internal static class MotorValueHelper
    {
        // motor speed reversal bit mask
        private const byte SpeedReversalBitMask = 0x80;

        /// <summary>
        /// Calculate the motor speed and direction value as a single byte
        /// </summary>
        public static byte Speed(byte speed, bool forward)
        {
            if (speed > 127 || speed < 0) throw new ArgumentOutOfRangeException("speed");
            return forward ? speed : Convert.ToByte(speed | SpeedReversalBitMask);
        }

      
        /// <summary>
        /// Add the UART communication checksum to the byte array to send
        /// </summary>
        public static byte[] AppendSendChecksumToCommandBytes(byte[] command)
        {
            var len = command.Length;
            var cmd = new byte[len + 1];
            var checksum = CalcChecksum(command, 0, command.Length);
            command.CopyTo(cmd, 0);
            cmd[len] = checksum;
            return cmd;
        }

        /// <summary>
        /// Check the UART communication checksum of the byte array received
        /// </summary>
        public static bool CheckChecksumResponse(byte[] response)
        {
            var checksum = CalcChecksum(response, 0, response.Length-1);
            return response[response.Length-1] == checksum ; 
        }

        /// <summary>
        /// Calculate the UART communication checksum 
        /// </summary>
        private static byte CalcChecksum(byte[] bytes, int index, int length)
        {
            uint num = 0;
            for (int i = index; i < (index + length); i++)
            {
                num += bytes[i];
            }
            return (byte)num;
        }
    }
}
