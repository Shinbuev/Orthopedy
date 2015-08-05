using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stabilograph.Protocol
{
    public class Protocol
    {
        private readonly SerialPort _serialPort;
        private readonly byte[] _handShake = new byte[] { 0xAE };
        private readonly byte[] _packetStart = new byte[] { 0XAC };
        private readonly byte _packetEnd = 0xEF;

        public Protocol(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void Initialize()
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }

        public int[] ReadChannels()
        {
            _serialPort.Write(_handShake, 0, 1);
            var handShakeResponse = (byte)_serialPort.ReadByte();
            if (handShakeResponse != _handShake[0])
                throw new ProtocolException(String.Format("Handshake is wrong. Expected {0}, but {1} is found", _handShake[0], handShakeResponse));

            _serialPort.Write(_packetStart, 0, 1);
            var packetStartResponse = (byte)_serialPort.ReadByte();
            if (handShakeResponse != _handShake[0])
                throw new ProtocolException(String.Format("Packet start is wrong. Expected {0}, but {1} is found", _packetStart[0], packetStartResponse));

            var bytes = ReadBytes(32);
            var crcBytes = ReadBytes(2);
            var packetEnd = _serialPort.ReadByte();

            if (packetEnd != _packetEnd)
                throw new ProtocolException(String.Format("Packet end is wrong. Expected {0}, but {1} is found", _packetEnd, packetEnd));

            var crc = BytesToUint(crcBytes[0], crcBytes[1]);
            var channels = BytesToChannels(bytes);
            VerifyCs(channels, crc);

            return channels;
        }

        private int BytesToUint(int lsb, int msb)
        {
            return ((lsb & 0xFF) | ((msb & 0xFF) << 8));
        }

        private int[] BytesToChannels(int[] bytes)
        {
            var channels = new int[16];

            for (int index = 0; index < 16; index++)
            {
                channels[index] = BytesToUint(bytes[index * 2], bytes[index * 2 + 1]);
            }

            Debug.WriteLine("Channels: " + String.Join(", ", channels));

            return channels;
        }

        private void VerifyCs(int[] bytes, int crc)
        {
            var calculatedCrc = bytes.Select(i => (uint)i).Aggregate((uint)0, (acc, value) => acc += value) / 16;
            if(calculatedCrc != (uint)crc)
                Debug.WriteLine(String.Format("Crc {0}, {1}", calculatedCrc, crc));
                //throw new ProtocolException(String.Format("Check sum is wrong. Expected {0}, but {1} is found", firstCs, actualFirstCs));
//
//            if ((byte)actualSecondCs != secondCs)
//                throw new ProtocolException(String.Format("Check sum is wrong. Expected {0}, but {1} is found", secondCs, actualSecondCs));
        }

        private int[] ReadBytes(int size)
        {
            var bytes = new int[size];
            var index = 0;

            while (index < bytes.Length)
            {
                bytes[index++] = _serialPort.ReadByte();
            }

            Debug.WriteLine("Bytes " + String.Join("|", bytes));

            return bytes;
        }
    }

    public class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message) { }
    }
}