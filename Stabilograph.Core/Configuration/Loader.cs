using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using JsonConfig;

namespace Stabilograph.Core.Configuration
{
    public class Loader
    {
        public static Root Load()
        {
            return Load(Config.Global);
        }

        public static Root Load(dynamic config)
        {
            return new Root
            {
                Platform = ReadPlatformConfig(config),
                Port = ReadSerialPortConfig(config),
                Version = config.Version,
                Diagnostic = ReadDiagnostic(config),
                Sensors = ReadSensors(config)
            };
        }

        public static Platform ReadPlatformConfig(dynamic config)
        {
            var leftCorrection = new PointF((float) config.CenterCorrection.Left.X,
                (float) config.CenterCorrection.Left.Y);
            var rightCorrection = new PointF((float) config.CenterCorrection.Right.X,
                (float) config.CenterCorrection.Right.Y);
            var lenghtBetweenCenters = (float) config.LengthBetweenCenters;

            return new Platform
            {
                High = config.Platform.Height,
                Width = config.Platform.Width,
                LeftCorrection = leftCorrection,
                RightCorrection = rightCorrection,
                LengthBetweenCenters = lenghtBetweenCenters
            };
        }

        public static SerialPort ReadSerialPortConfig(dynamic config)
        {
            string portName = config.Port.Name;
            int baudrate = config.Port.Baudrate;
            var parity = Parity.None;
            if (!Enum.TryParse(config.Port.Parity, true, out parity))
            {
                throw new InvalidEnumArgumentException(
                    string.Format("Cannot parse port parity from configured value: {0}. Default value {1} is used",
                        config.Port.Parity, parity.ToString()));
            }
            var openInterval = (int) config.Port.OpenInterval;
            return new SerialPort {Name = portName, Baudrate = baudrate, Parity = parity, OpenInterval = openInterval};
        }

        public static Diagnostic ReadDiagnostic(dynamic config)
        {
            var value = config.Diagnostic.Duration as string;
            var duration = TimeSpan.Parse(value);
            return new Diagnostic {Duration = duration};
        }

        public static List<Sensor> ReadSensors(dynamic config)
        {
            var sensors = config.Sensors as ICollection<dynamic>;

            return sensors.Select(sensor => ReadSensor(sensor) as Sensor).ToList();
        }

        private static Sensor ReadSensor(dynamic config)
        {
            int index = config.Index;
            var position = new PointF((float) config.Position.X, (float) config.Position.Y);
            var points = config.Points as ICollection<dynamic>;
            var interpolation = points.Select(point => new PointF(point.Value, point.Weight)).ToList();

            return new Sensor
            {
                Index = index,
                Position = position,
                Interpolation = interpolation
            };
        }
    }
}