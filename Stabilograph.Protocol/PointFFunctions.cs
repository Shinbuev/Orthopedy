using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stabilograph.Protocol
{
    public static class PointFunctions
    {
        public static double DistanceTo(this PointF point1, PointF point2)
        {
            var a = (double)(point2.X - point1.X);
            var b = (double)(point2.Y - point1.Y);

            return Math.Sqrt(a * a + b * b);
        }
    }
}
