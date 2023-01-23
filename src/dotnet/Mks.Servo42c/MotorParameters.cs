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
    public class MotorParameters
    {
        /// <summary>
        /// the UART address of the first motor 
        /// </summary>
        internal const byte FirstServoAdress = 224; // = 0xE0 OR Convert.ToInt32("0XE0", 16);

        /// <summary>
        /// Connection speed for UART communinection
        /// </summary>
        public enum UartBauds
        {
            b9600 = 9600,
            b19200 = 19200,
            b25000 = 25000,
            b38400 = 38400,
            b57600 = 57600,
            b115200 = 115200
        }

        public const byte ENABLE_STATE_ON = 0x01;   // Motor driver is enabled
        public const byte ENABLE_STATE_OFF = 0x02;  // Motor driver is disabled

        public const byte BLOCKED_STATE_YES = 0x01;     // Motor is blockes
        public const byte BLOCKES_STATE_NO = 0x02;      // Motor is not blocked

        public const byte ENABLE_ON = 0x01;         // Enable motor driver
        public const byte ENABLE_OFF = 0x00;        // Disable motor driver

        public enum ReturnToZeroModes
        {
            Disable = 0,
            DirectionalMode = 1,
            NearMode = 2
        }

    }
}
