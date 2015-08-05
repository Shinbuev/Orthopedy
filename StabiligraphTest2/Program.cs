using System;
using System.IO.Ports;
using System.Timers;
using Stabilograph.Protocol;

namespace StabiligraphTest2
{
    internal class Program
    {
        private static SerialPort _serialPort;
        private static Protocol _protocol;
        private static ChannelReader _reader;
        private static readonly Timer _timer = new Timer {Interval = 1000, AutoReset = true};

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var portName = args[0];
            Console.WriteLine("Tryingn to open {0}", portName);

            _serialPort = new SerialPort(portName, 9600, Parity.None);
            _serialPort.Open();

            _protocol = new Protocol(_serialPort);
            _reader = new ChannelReader(_protocol);

            _timer.Elapsed += DisplaySensors;
            _timer.Enabled = true;

            Console.WriteLine("Press any key for exit");
            Console.ReadKey(true);
            _timer.Enabled = false;
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: StabilographTest2 COM3");
        }

        private static void DisplaySensors(object sender, ElapsedEventArgs e)
        {
            var channels = _reader.ReadWeights();
            Console.WriteLine(string.Join(", ", channels));
        }
    }
}