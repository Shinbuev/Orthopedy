using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using JsonConfig;

namespace Stabilograph.Protocol.Configuration
{
    public class Configuration
    {
        public static Configuration Load()
        {
            return Load(Config.Global);
        }

        public static Configuration Load(dynamic config)
        {
            return new Configuration()
            {
                Platform = ReadPlatformConfig(config),
                Port = ReadSerialPortConfig(config),
                Version =  config.Version,
                Diagnostic = ReadDiagnostic(config),
                Sensors = ReadSensors(config)
            };
        }

        public Platform Platform { get; private set; }

        public static Platform ReadPlatformConfig(dynamic config)
        {
            var leftCorrection = new PointF((float)config.CenterCorrection.Left.X, (float)config.CenterCorrection.Left.Y);
            var rightCorrection = new PointF((float)config.CenterCorrection.Right.X, (float)config.CenterCorrection.Right.Y);
            var lenghtBetweenCenters = (float)config.LengthBetweenCenters;

            return new Platform
            {
                High = config.Platform.Height,
                Width = config.Platform.Width,
                LeftCorrection = leftCorrection,
                RightCorrection = rightCorrection,
                LengthBetweenCenters = lenghtBetweenCenters
            };
        }

        public SerialPort Port { get; private set; }

        public static SerialPort ReadSerialPortConfig(dynamic config)
        {
            string portName = config.Port.Name;
            int baudrate = config.Port.Baudrate;
            var parity = Parity.None;
            if (!Enum.TryParse(config.Port.Parity, true, out parity))
            {
                throw new InvalidEnumArgumentException(
                    String.Format("Cannot parse port parity from configured value: {0}. Default value {1} is used",
                        config.Port.Parity, parity.ToString()));
            }
            var openInterval = (int)config.Port.OpenInterval;
            return new SerialPort() { Name = portName, Baudrate = baudrate, Parity = parity, OpenInterval = openInterval};
        }

        public string Version { get; private set; }

        public Diagnostic Diagnostic { get; private set; }

        public static Diagnostic ReadDiagnostic(dynamic config)
        {
            var value = config.Diagnostic.Duration as string;
            TimeSpan duration = TimeSpan.Parse(value);
            return new Diagnostic() {Duration = duration};
        }

        public List<Sensor> Sensors { get; private set; }

        public static List<Sensor> ReadSensors(dynamic config)
        {
            var sensors = config.Sensors as ICollection<dynamic>;

            return sensors.Select(sensor => ReadSensor(sensor) as Sensor).ToList();
        }

        private static Sensor ReadSensor(dynamic config)
        {
            int index = config.Index;
            var position = new PointF((float)config.Position.X, (float)config.Position.Y);
            var points = config.Points as ICollection<dynamic>;
            var interpolation = points.Select(point => new PointF(point.Value, point.Weight)).ToList(); 

            return new Sensor()
            {
                Index = index,
                Position = position,
                Interpolation = interpolation
            };
        }
    }

    public class Platform
    {
        public float Width { get; set; }
        public float High { get; set; }
        public float LengthBetweenCenters { get; set; }
        public PointF LeftCorrection { get; set; }
        public PointF RightCorrection { get; set; }
        public SizeF Size {
            get
            {
                return new SizeF(Width, High);
            }
        }
    }

    public class SerialPort
    {
        public string Name { get; set; }
        public int Baudrate { get; set; }
        public Parity Parity { get; set; }
        public int OpenInterval { get; set; }
    }

    public class Diagnostic
    {
        public TimeSpan Duration { get; set; }
    }

    public class Sensor
    {
        public int Index { get; set; }
        public PointF Position { get; set; }
        public List<PointF> Interpolation { get; set; } 
    }

}

//    string portName = Config.Global.Port.Name;
//            int baudrate = Config.Global.Port.Baudrate;
//            var parity = Parity.None;
//            if (!Parity.TryParse(Config.Global.Port.Parity, true, out parity))
//            {
//                var message = String.Format("Cannot parse port parity from configured value: {0}. Default value {1} is used",
//                    Config.Global.Port.Parity, parity.ToString());
//                notifyIcon1.ShowBalloonTip(3000, "Configuration Error", message, ToolTipIcon.Error);
//
//                notifyIcon1.BalloonTipClosed += ShowPortSettings;
//            }
//            else
//            {
//                serialPort.PortName = portName;
//                serialPort.BaudRate = baudrate;
//                serialPort.Parity = parity;
//
//                ShowPortSettings(this, EventArgs.Empty);
//            }

