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
    internal class MotorCommandSender
    {
        private const int TIMEOUT_DEFAULT_MS = 10;

        private MySerialPort _serialPort;
        private const int MaxMotorCount = 16;
        private MotorCommands?[] _commandSent = new MotorCommands?[MaxMotorCount];

        public MotorCommandSender(MySerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public async Task<byte[]?> SendCommand(byte motorIndex, MotorCommands command, int timeoutMs = TIMEOUT_DEFAULT_MS)
        {
            byte[] cmdBytes = Build2ByteCommand((byte)(MotorParameters.FirstServoAdress + motorIndex), (byte)command);
            _commandSent[motorIndex] = command;
            await SendCommand(cmdBytes);
            return await GetRawResult(motorIndex: motorIndex, timeoutMs: timeoutMs);
        }

        public async Task<byte[]?> SendCommand(byte motorIndex, MotorCommands command, byte value, int timeoutMs = TIMEOUT_DEFAULT_MS)
        {
            byte[] cmdBytes = Build3ByteCommand((byte)(MotorParameters.FirstServoAdress + motorIndex), motorCmd: (byte)command, value : value );
            _commandSent[motorIndex] = command;
            await SendCommand(cmdBytes);
            return await GetRawResult(motorIndex: motorIndex, timeoutMs: timeoutMs);
        }

        public async Task<byte[]?> SendCommand(byte motorIndex, MotorCommands command, UInt16 value, int timeoutMs = TIMEOUT_DEFAULT_MS)
        {
            byte[] cmdBytes = Build4ByteCommand((byte)(MotorParameters.FirstServoAdress + motorIndex), motorCmd: (byte)command, value: value );
            _commandSent[motorIndex] = command;
            await SendCommand(cmdBytes);
            return await GetRawResult(motorIndex: motorIndex, timeoutMs: timeoutMs);
        }

        public async Task<byte[]?> SendSpeedPulseCommand(byte motorIndex, MotorCommands command, byte speed, UInt32 pulse, int timeoutMs = TIMEOUT_DEFAULT_MS)
        {
            byte[] cmdBytes = BuildSpeedPulseCommand((byte)(MotorParameters.FirstServoAdress + motorIndex), (byte)command, speed: speed, pulse: pulse);
            _commandSent[motorIndex] = command;
            await SendCommand(cmdBytes);
            return await GetRawResult(motorIndex: motorIndex, timeoutMs: timeoutMs);
        }

        /// <summary>
        /// Write command data to serial port and add checksum
        /// </summary>
        private async Task SendCommand(byte[] command, int waitTimeMs = 2)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (command.Length == 0) throw new ArgumentException("command length == 0");

            if (_serialPort.IsOpen)
            {
                var cmd = MotorValueHelper.AppendSendChecksumToCommandBytes(command); // append checksum
                await this._serialPort.SendBytes(cmd, waitTimeMs);
            }
            else
            {
                throw new IOException("Serialport is not open！");
            }
        }

        private async Task<byte[]?> GetRawResult(int motorIndex, int timeoutMs = TIMEOUT_DEFAULT_MS)
        {
            var timeoutCountdown = timeoutMs;

            while (timeoutCountdown-- > 0)
            {
                if (_serialPort.ResponseBytes != null)
                {
                    if (_serialPort.ResponseBytes.Length > 1)
                    {
                        if (motorIndex + MotorParameters.FirstServoAdress == _serialPort.ResponseBytes[0])
                        {
                            // this is an answer from the correct motor
                            break;
                        }
                    }
                    else
                    {
                        // to short length of byte array?!?
                        _serialPort.ClearResponse();
                    }
                }
                await Task.Delay(1);
            }

            if (_serialPort.ResponseBytes == null)
            {
                Console.WriteLine($"Timeout! Receive {_commandSent[motorIndex].ToString()}");
                return null;
            }

            var responseBytes = (byte[])_serialPort.ResponseBytes.Clone();
            _commandSent[motorIndex] = null;
            _serialPort.ClearResponse();

            if (MotorValueHelper.CheckChecksumResponse(responseBytes))
            {
                return responseBytes;
            } 
            else
            {
                Console.WriteLine($"Wrong checksum! Receive {_commandSent[motorIndex].ToString()}");
                return null;
            }
        }

        private static byte[] Build2ByteCommand(byte address, byte motorCmd)
          => new byte[2] { address, motorCmd };

        private static byte[] Build3ByteCommand(byte address, byte motorCmd, byte value)
            => new byte[3] { address, motorCmd, value };

        private static byte[] Build4ByteCommand(byte address, byte motorCmd, UInt16 value)
        {
            var command = new byte[4] { address, motorCmd, 0, 0 };
            TypeConverting.Uint16ToByteArray(value).CopyTo(command, 2);
            return command;
        }

        private static byte[] BuildSpeedPulseCommand(byte address, byte motorCmd, byte speed, UInt32 pulse)
        {
            var command = new byte[7];
            command[0] = address;
            command[1] = motorCmd;
            command[2] = speed;
            TypeConverting.Uint32ToByteArray(pulse).CopyTo(command, 3);
            return command;
        }

        //private static string CalcAdress(int servoNo)
        //{
        //    var rootAdress = MotorParameters.FirstServoAdress;
        //    var adress = $"0x{rootAdress + servoNo:X}";
        //    return adress;
        //}

    }
}
