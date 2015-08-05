using System.IO.Ports;

namespace Stabilograph.Core.Configuration
{
    public class SerialPort
    {
        public string Name { get; set; }
        public int Baudrate { get; set; }
        public Parity Parity { get; set; }
        public int OpenInterval { get; set; }
    }
}