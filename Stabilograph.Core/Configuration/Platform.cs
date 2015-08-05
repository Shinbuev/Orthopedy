using System.Drawing;

namespace Stabilograph.Core.Configuration
{
    public class Platform
    {
        public float Width { get; set; }
        public float High { get; set; }
        public float LengthBetweenCenters { get; set; }
        public PointF LeftCorrection { get; set; }
        public PointF RightCorrection { get; set; }

        public SizeF Size
        {
            get { return new SizeF(Width, High); }
        }
    }
}