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
    internal enum MotorCommands
    {
        // Resetting motor parameters
        FactoryReset = 0x3F,

        // Motor programming commands
        Calibrate = 0x80, // Calibrating the encoder

        /* 
         * the following programming parameters have not yet been implemented in this library: 
         
            MotorType = 0x81,   // Set the motor type
            CtrMode = 0x82,     // Set the working mode
            Ma = 0x83,          // Set current level
            MStep = 0x84,       // Set the subdivision (microsteps)
            En = 0x85,          // Set the active level of the En pin
            Direction = 0x86,       // Set the positive direction of motor rotation
            AutoScreenOff = 0x87,   // Set auto screen off function
            Protect = 0x88,         // Set blocking protection function
            MPlyer = 0x89,      // Set interpolation function
            UartBuad = 0x8A,    // Set UART serial port baud rate
            UartAddr = 0x8B,    // Set UART serial port address

        */

        // Read parameter command
        ReadEncoderVal = 0x30, // Reads the encoder value (calibrated und interpolated)
        ReadTotalPulse = 0x33, // Reads the total number of input pulses
        ReadPosition = 0x36, // Read the real-time position of the closed-loop motor
        ReadAngleError = 0x39, // Reads the position angle error, when torque is applied to the motor
        ReadDriverBoard = 0x3A, // Read the enable status of the closed-loop driver board
        ReadBlockFlag = 0x3E, // Read the blocking flag (is blocking: return e0 01; no blocking: return e0 02; error command: return e0 00)

        // Serial direct control commands
        EnableDisableDriver = 0xF3, // Modify the enable status of the driver board in serial port control mode         
        Start = 0xF6, // Starts the motor forward/reverse at a given speed
        Stop = 0xF7, // Stop the motor in forward/reverse rotation
        Save = 0xFF, // 保存/清除保存上面(2)中所设置的正/反转速度                
        MoveMotorByPulses = 0xFD, // Serial port direct position control rotation

        /* 
         * the following programming parameters to set PID/acceleration/torque parameter 
         * have not yet been implemented in this library: 
         
            PID_Kp = 0xA1,  // Set the position Kp parameter
            PID_Ki = 0xA2,  // Set the location Ki parameter
            PID_Kd = 0xA3,  // Set the position Kd parameter
            ACC = 0xA4,     // Sets the acceleration ACC parameter
            MaxT = 0xA5,    // Set the MaxT parameter

        */

        // Commands to program and activate "Auto Return to Zero"
        Return2Zero_Mode = 0x90, // Set the auto-zero mode: 00=Disable, 01=directional mode, 02=near mode
        Return2Zero_Set_O = 0x91, // Set the auto-zero  point 
        Return2Zero_Speed = 0x92, // Set the auto-zero speed 0-4
        Return2Zero_Dir = 0x93, // Set the auto-zero odirection 
        Return2Zero_Goto_O = 0x94 // Return to zero point (if one is stored)
    }
}
