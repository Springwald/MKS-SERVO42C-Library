// Control MKSServo42c Servos via serial port
// https://github.com/Springwald/MKS-SERVO42C-Library
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using static Mks.Servo42c.MotorParameters;

namespace Mks.Servo42c
{
    /// <summary>
    /// Controls a MskServo42c stepper motor via a given serial port
    /// </summary>
    public class MksServo42cControl
    {
        /// <summary>
        /// When waiting on return-to-zero command to execute the motor path takes some time...
        /// </summary>
        private const int TIMEOUT_RETURN_TO_ZERO_MS = 3000;

        /// <summary>
        /// Storing the return-to-zero point takes some time...
        /// </summary>
        private const int TIMEOUT_SET_RETURN_TO_ZERO_POS__MS = 1000;

        /// <summary>
        /// When waiting on calibrate command to execute the motor path takes some time...
        /// </summary>
        private const int TIMEOUT_RETURN_CALIBRATE_MS = 5000;

        /// <summary>
        /// When turning off and on the motor driver, the position value seems to be reset to 0. 
        /// This int should fix this a little bit by sorting the last value when changing driver enable/disable.
        /// </summary>
        private int positionCorrection = 0;

        /// <summary>
        /// Is the motor driver enabled or in passive mode?
        /// </summary>
        private bool _isEnabled = false;

        /// <summary>
        /// the serial port to connect the motor driver via UART
        /// </summary>
        private MySerialPort _serialPort;

        /// <summary>
        /// manages the detail-communication to the motor
        /// </summary>
        private MotorCommandSender _motorCommandSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public MksServo42cControl()
        {
            this._serialPort = new MySerialPort();
            this._motorCommandSender = new MotorCommandSender(_serialPort);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MksServo42cControl()
        {
            this._serialPort.Close();
        }

        /// <summary>
        /// Open the serial port
        /// </summary>
        /// <param name="portName">e.g. "COM6"</param>
        /// <returns></returns>
        public bool Open(string portName, MotorParameters.UartBauds baud) =>
            _serialPort.Open(portName, baud);


        /* --- read motor values ---- */

        /// <summary>
        /// Reads the value of the magnetic encoder
        /// </summary>
        public async Task<UInt16?> GetEncoderValue(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadEncoderVal))?.SkipAddressByte(length: 2).ToUInt16();

        /// <summary>
        /// Reads the total number of input pulses
        /// </summary>
        public async Task<int?> GetTotalPulse(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadTotalPulse))?.SkipAddressByte(length: 4).ToUInt32().FixOverflow();

        /// <summary>
        /// Read the real-time position of the closed-loop motor
        /// </summary>
        public async Task<int?> GetMotorPosition(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadPosition))?.SkipAddressByte(length: 4).ToUInt32().FixOverflow()
            + positionCorrection;

        /// <summary>
        /// Reads the position angle error, when torque is applied to the motor
        /// </summary>
        public async Task<Int16?> GetAngleError(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadAngleError))?.SkipAddressByte(length: 2).ToInt16();

        /// <summary>
        /// Read the enable status of the closed-loop driver board
        /// </summary>
        public async Task<bool?> GetIsDriverEnabled(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadDriverBoard))?.SkipAddressByte(length: 2).ToUInt16() switch
            {
                MotorParameters.ENABLE_STATE_ON => true,
                MotorParameters.ENABLE_STATE_OFF => false,
                _ => null,
            };

        /// <summary>
        /// Read the blocking flag, whether the motor no longer has enough power and is blocked
        /// </summary>
        public async Task<bool?> GetIsBlocked(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.ReadBlockFlag))?.SkipAddressByte(length: 2).ToUInt16() switch
            {
                MotorParameters.BLOCKED_STATE_YES => true,
                MotorParameters.BLOCKES_STATE_NO => false,
                _ => null,
            };

        /* --- return to zero ---- */

        /// <summary>
        /// Return to zero point (if one is stored)
        /// </summary>
        public async Task<bool> ReturnToZero(byte motorIndex)
        {
            if ((await _motorCommandSender.SendCommand(
                motorIndex: motorIndex,
                command: MotorCommands.Return2Zero_Goto_O,
                value: 0,
                timeoutMs: TIMEOUT_RETURN_TO_ZERO_MS))?[1] == 1)
            {
                positionCorrection = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the return to zero point to the actual position
        /// </summary>
        public async Task<bool> SetReturnToZeroPoint(byte motorIndex)
            => (await _motorCommandSender.SendCommand(
               motorIndex, MotorCommands.Return2Zero_Set_O,
               value: (byte)0, 
               timeoutMs: TIMEOUT_SET_RETURN_TO_ZERO_POS__MS))?[1] == 1;

        /// <summary>
        /// Sets the return to zero mode to "direction" mode
        /// </summary>
        public async Task<bool> SetReturnToZeroModeDirectional(byte motorIndex, byte speed, bool forward)
        {
            if ((await SetReturnToZeroSpeed(motorIndex, speed))== false) return false;

            // Set mode
            if ((await _motorCommandSender.SendCommand(
               motorIndex, MotorCommands.Return2Zero_Mode,
               value: (byte)ReturnToZeroModes.DirectionalMode))?[1] != 1) return false;

            // set direction
            if ((await _motorCommandSender.SendCommand(
               motorIndex, MotorCommands.Return2Zero_Dir,
               value: forward ?  (byte)1 : (byte)0))?[1] != 1) return false;

            return true;
        }

        /// <summary>
        /// Sets the return to zero mode to "nearest" mode
        /// </summary>
        /// <param name="speed">0=slowest 4=max</param>
        public async Task<bool> SetReturnToZeroModeNearest(byte motorIndex, byte speed)
        {
            if ((await SetReturnToZeroSpeed(motorIndex, speed)) == false) return false;

            // Set mode
            if ((await _motorCommandSender.SendCommand(
               motorIndex, MotorCommands.Return2Zero_Mode,
               value: (byte)ReturnToZeroModes.NearMode))?[1] != 1) return false;

            return true;
        }

        /// <param name="speed">0=slowest 4=max</param>
        private async Task<bool> SetReturnToZeroSpeed(byte motorIndex, byte speed)
        {
            // calc speed
            speed = (byte)(4 - Math.Max(0, Math.Min(4, (int)speed)));

            // set speed
            return ((await _motorCommandSender.SendCommand(
               motorIndex, MotorCommands.Return2Zero_Speed,
               value: speed))?[1] == 1);
        }

        public async Task<bool> DisableReturnToZero(byte motorIndex)
            => (await _motorCommandSender.SendCommand(
                motorIndex, MotorCommands.Return2Zero_Mode,
                value: (byte)ReturnToZeroModes.Disable))?[1] == 1;

        /* --- move motor manual ---- */

        /// <summary>
        /// Modify the enable status of the driver board in serial port control mode     
        /// </summary>
        public async Task<bool> EnableDriver(byte motorIndex, bool enable)
        {
            if (enable && _isEnabled == false)
            {
                int? pos = null;
                int count = 10;
                while (pos == null && count-- > 0)
                {
                    pos = await this.GetMotorPosition(motorIndex);
                }
                if (pos == null) throw new ApplicationException("can't read position!");
                positionCorrection = pos.Value;
            }

            var success = (await _motorCommandSender.SendCommand(
                motorIndex: motorIndex,
                command: MotorCommands.EnableDisableDriver,
                value: enable ? MotorParameters.ENABLE_ON : MotorParameters.ENABLE_OFF))?[1] == 1;

            if (success)
            {
                _isEnabled = enable;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stop the motor in forward/reverse rotation
        /// </summary>
        public async Task<bool> StopMovement(byte motorIndex)
            => (await _motorCommandSender.SendCommand(motorIndex, MotorCommands.Stop))?[1] == 1;

        /// <summary>
        /// Starts the motor forward/reverse at a given speed
        /// </summary>
        public async Task<bool> StartMovement(byte motorIndex, bool forward, byte speed)
            => (await _motorCommandSender.SendCommand(
                motorIndex, MotorCommands.Start,
                value: MotorValueHelper.Speed(speed, forward)))?[1] == 1;

        /// <summary>
        /// Serial port direct position control rotation
        /// </summary>
        public async Task<bool> MoveMotorByPulse(byte motorIndex, byte speed, bool forward, UInt32 pulse)
            => (await _motorCommandSender.SendSpeedPulseCommand(
                motorIndex: motorIndex,
                command: MotorCommands.MoveMotorByPulses,
                speed: MotorValueHelper.Speed(speed, forward),
                pulse: pulse))?[1] == 1;


        /* --- various motor functions ---- */

        /// <summary>
        /// Calibrate the encoder
        /// </summary>
        public async Task<bool> Calibrate(byte motorIndex)
            => (await _motorCommandSender.SendCommand(
                motorIndex: motorIndex,
                command: MotorCommands.Calibrate,
                value: 0,
                timeoutMs: TIMEOUT_RETURN_CALIBRATE_MS))?[1] == 1;

    }
}