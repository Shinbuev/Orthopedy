using System.Collections.Generic;
using System.Diagnostics;

namespace Stabilograph.Core.Configuration
{
    public class Root
    {
        public bool Debug { get; internal set; }
        public Platform Platform { get; internal set; }
        public SerialPort Port { get; internal set; }
        public string Version { get; internal set; }
        public Diagnostic Diagnostic { get; internal set; }
        public List<Sensor> Sensors { get; internal set; }
    }
}