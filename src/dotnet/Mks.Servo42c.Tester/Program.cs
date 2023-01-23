// Control MKSServo42c Servos via serial port
// https://github.com/Springwald/MKS-SERVO42C-Library
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using Mks.Servo42c;

var control = new MksServo42cControl();
if (control.Open("COM6", baud: MotorParameters.UartBauds.b38400) == false)
    throw new Exception("Can't open port");

const byte motorIndex = 0;
const bool moveToZero = true;

await control.StopMovement(motorIndex);
await control.EnableDriver(motorIndex: motorIndex, enable: false);

if (moveToZero) // Move to zero pos 
{
    Console.WriteLine("return to zero...");
    await control.ReturnToZero(motorIndex);
    Console.WriteLine("ready.");
    await Task.Delay(500);
}


// ##############################################
// ## Uncomment to choose the wanted function: ##
// ##############################################

//await control.SetReturnToZeroModeNearest(motorIndex, speed: 4);
//await control.SetReturnToZeroPoint(motorIndex);

// await control.Calibrate(motorIndex); // Calibrate 

// await CheckSwitchBetweenDriverEnabledAndDisabled(control);
// await MoveByPressure();
// await MoveByPressure2();

await ShowMotorPositionForManualMovement();



// Shut down motor
await control.StopMovement(motorIndex);
await control.EnableDriver(motorIndex: motorIndex, enable: false);

/* -------------------------------------------------  */



/// <summary>
/// switch between motor movement and passive move by keypress
/// </summary>
async Task CheckSwitchBetweenDriverEnabledAndDisabled(MksServo42cControl? control)
{
    while (true)
    {
        await ShowDiagnostics(motorMoving: true);
        while (Console.KeyAvailable) Console.ReadKey();

        await ShowMotorPositionForManualMovement();
        while (Console.KeyAvailable) Console.ReadKey();
    }
}


/// <summary>
/// Move the motor by applying pressure by hand while observing end stop limits
/// </summary>
async Task MoveByPressure()
{
    long[] deltas = new long[10];
    int deltaRunner = 0;

    await control.StopMovement(motorIndex);
    await control.EnableDriver(motorIndex: motorIndex, enable: true);

    var startPos = (await control.GetMotorPosition(motorIndex: motorIndex)).Value;

    long actualSpeed = 0;

    while (!Console.KeyAvailable)
    {
        var pressureRaw = await control.GetAngleError(motorIndex: motorIndex);
        var posRaw = await control.GetMotorPosition(motorIndex: motorIndex);
        if (pressureRaw != null && posRaw != null)
        {
            var pos = posRaw - startPos;
            var pressure = pressureRaw.Value;

            deltaRunner++;
            if (deltaRunner >= deltas.Length) deltaRunner = 0;
            deltas[deltaRunner] = pressure - actualSpeed * 5; // compensate mass 
            var delta = deltas.Sum() / deltas.Length;

            if (pos > 16000 && delta < 0 || pos < -16000 && delta > 0)
            {
                await control.StopMovement(motorIndex);
                actualSpeed = (actualSpeed + 0) / 2;
            }
            else
            {
                if (Math.Abs(delta) > 60)
                {
                    var speed = Math.Max(1, Math.Min(3, Math.Abs(delta) / 5));
                    actualSpeed = (actualSpeed + speed) / 2;
                    await control.StartMovement(motorIndex, speed: (byte)speed, forward: delta > 0);
                }
                else
                {
                    await control.StopMovement(motorIndex);
                    actualSpeed = (actualSpeed + 0) / 2;
                }
            }
            Console.WriteLine(delta.ToString().PadLeft(10) + " " + pos);
        }
    }
}

/// <summary>
/// Move the motor by applying pressure by hand
/// </summary>
async Task MoveByPressure2()
{
    long[] deltas = new long[10];
    int deltaRunner = 0;

    await control.EnableDriver(motorIndex: motorIndex, enable: true);

    while (!Console.KeyAvailable)
    {
        var pressure = await control.GetAngleError(motorIndex: motorIndex);
        if (pressure != null)
        {
            deltaRunner++;
            if (deltaRunner >= deltas.Length) deltaRunner = 0;
            deltas[deltaRunner] = pressure.Value;
            var delta = deltas.Sum() / deltas.Length;

            if (Math.Abs(delta) > 50)
            {
                Console.WriteLine(delta.ToString().PadLeft(10));
                var speed = Math.Max(1, Math.Min(5, Math.Abs(delta) / 10));
                await control.MoveMotorByPulse(motorIndex, speed: (byte)speed, forward: delta > 0, pulse: (uint)Math.Abs(delta * 10));
                await Task.Delay(10); // Math.Abs(delta / 10) / speed);
            }
        }
    }
}


/// <summary>
/// set motor to passive mode and display manual movement
/// </summary>
async Task ShowMotorPositionForManualMovement()
{
    // these 2 lines are important to enable position reading when moving manual
    await control.EnableDriver(motorIndex: motorIndex, enable: false);
    await control.StopMovement(motorIndex);

    int max = int.MinValue;
    int min = int.MaxValue;

    while (!Console.KeyAvailable)
    {
        var motorPos = await control.GetMotorPosition(motorIndex: motorIndex);
        if (motorPos != null)
        {
            max = Math.Max(motorPos.Value, max);
            min = Math.Min(motorPos.Value, min);
            if (max != min)
            {
                const int charsPerLine = 80;
                Console.WriteLine(motorPos.Value.ToString().PadRight(12) + string.Empty.PadLeft((int)(charsPerLine * (motorPos.Value - min) / (max - min)), '#'));
            }
            else
            {
                Console.WriteLine("No movement detected yet.");
            }
        }
        await Task.Delay(10);
    }
}

/// <summary>
/// Show all values readable
/// </summary>

async Task ShowDiagnostics(bool motorMoving)
{
    if (motorMoving)
    {
        await control.EnableDriver(motorIndex: 0, enable: true);
        await control.StartMovement(motorIndex, forward: false, speed: 4);
    }
    else
    {
        await control.StopMovement(motorIndex); 
        await control.EnableDriver(motorIndex: 0, enable: false);
    }
    

    while (!Console.KeyAvailable)
    {
        Console.WriteLine(
            "Encoder: " + (await control.GetEncoderValue(motorIndex: motorIndex)).ToString().PadRight(12) +
            "Pulses: " + (await control.GetTotalPulse(motorIndex: motorIndex)).ToString().PadRight(12) +
            "Position: " + (await control.GetMotorPosition(motorIndex: motorIndex)).ToString().PadRight(12) +
            "AngleErr: " + (await control.GetAngleError(motorIndex: motorIndex)).ToString().PadRight(12) +
          "");

        await Task.Delay(1);
    }
}
