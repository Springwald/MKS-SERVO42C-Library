// Control MKSServo42c Servos via serial port
// https://github.com/Springwald/MKS-SERVO42C-Library
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Diagnostics;
using System.IO.Ports;

namespace Mks.Servo42c
{
    internal class MySerialPort
    {
        /// <summary>
        /// the pyhsical serial port
        /// </summary>
        private System.IO.Ports.SerialPort _serialPort;

        /// <summary>
        /// Is the serial port already open?
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// the last received bytes
        /// </summary>
        public byte[]? ResponseBytes { get; private set; }

        /// <summary>
        /// Contructor
        /// </summary>
        public MySerialPort()
        {
            this._serialPort = new System.IO.Ports.SerialPort();
            this._serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MySerialPort()
        {
            this.Close();
            this._serialPort.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
        }

        /// <summary>
        /// Open the physical serial port
        /// </summary>
        public bool Open(string comPort, MotorParameters.UartBauds baud = MotorParameters.UartBauds.b38400)
        {
            if (this.IsOpen) throw new ApplicationException("Port is already open.");

            try
            {
                _serialPort.PortName = comPort;
                _serialPort.BaudRate = (int)baud;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;
                _serialPort.Handshake = Handshake.None;
                _serialPort.Open();
                this.IsOpen = true;
                return true;
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached) throw;
            }
            return false;
        }

        /// <summary>
        /// Close the physical serial port
        /// </summary>
        public void Close()
        {
            if (this.IsOpen) _serialPort.Close();
            this.IsOpen = false;
        }

        /// <summary>
        /// Clear the last received bytes
        /// </summary>
        public void ClearResponse()
        {
            this.ResponseBytes = null;
        }


        /// <summary>
        /// Write command data to serial port and add checksum
        /// </summary>
        public async Task SendBytes(byte[] bytes, int waitTimeMs)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length == 0) throw new ArgumentException("bytes length == 0");

            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Write(bytes, 0, bytes.Length); //send data to serial port
                await Task.Delay(waitTimeMs);
            }
            else
            {
                throw new IOException("Serialport is not open！");
            }
        }

        /// <summary>
        /// Data is received from the serial port
        /// </summary>
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    var cmdReturn = SerialPortTools.Received(_serialPort);
                    if (cmdReturn != null)
                    {
                        this.ResponseBytes = cmdReturn;
                        // string? cmdReturnStrDebug = SerialPortTools.BytesToHexStr(cmdReturn);
                    }

                }
                catch (Exception ex)
                {

                    if (_serialPort != null)
                    {
                        _serialPort.Close();
                        this.IsOpen = false;
                        throw new Exception("Serial error: " + ex.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Serial port not ready");
            }
        }

    }
}
