using System.Collections.Generic;
using System.Drawing;

namespace Stabilograph.Core.Configuration
{
    public class Sensor
    {
        public int Index { get; set; }
        public PointF Position { get; set; }
        public List<PointF> Interpolation { get; set; }
    }
}