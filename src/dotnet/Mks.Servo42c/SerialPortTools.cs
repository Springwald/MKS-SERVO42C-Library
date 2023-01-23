// Control MKSServo42c Servos via serial port
// https://github.com/Springwald/MKS-SERVO42C-Library
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.IO.Ports;

namespace Mks.Servo42c
{
    internal class SerialPortTools
    {
        /// <summary>
        /// Read data from the serial port
        /// </summary>
        public static byte[] Received(SerialPort comPort, int readIntervalTime = 10)
        {
            if (comPort.IsOpen)
            {
                // Wait for serial data till completed
                int len = 0;
                do
                {
                    len = comPort.BytesToRead;
                    Thread.Sleep(readIntervalTime);

                } while ((len < comPort.BytesToRead) && (comPort.BytesToRead < 4800));

                //  Read data
                byte[] buffer = new byte[len];
                comPort.Read(buffer, 0, len);
                return buffer;
            }
            else
            {
                throw new IOException("Serial port is not open！");
            }
        }
    }
}
