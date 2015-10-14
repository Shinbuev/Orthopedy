using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JsonConfig;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Stabilograph
{
    public partial class PlatformControl : UserControl
    {
        private double PlatformHeight = Config.Global.Platform.Height;
        private double PlatformWidth = Config.Global.Platform.Width;
        private LineSeries _series;
        private int _numberOfPoints = 1000;

        private OxyPlot.WindowsForms.PlotView plotView;

        public PlatformControl()
        {
            InitializeComponent();
            this.plotView = new OxyPlot.WindowsForms.PlotView();

            this.plotView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plotView.Location = new System.Drawing.Point(0, 0);
            this.plotView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;

            this.Controls.Add(this.plotView);

            var greyColor = OxyColor.FromRgb(0, 0, 0);
            var top = PlatformHeight / 2;
            var bottom = -top;
            var right = PlatformWidth / 2;
            var left = -right;

            var plotModel = new PlotModel();

            plotModel.PlotType = PlotType.Cartesian;

            var axisY = new LinearAxis();
            axisY.Key = "y";
            axisY.MinimumPadding = 0.1;
            axisY.PositionAtZeroCrossing = true;
            axisY.Position = AxisPosition.Left;
            axisY.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            axisY.MajorGridlineStyle = LineStyle.Solid;
            axisY.MinorGridlineStyle = LineStyle.Dot;
            plotModel.Axes.Add(axisY);


            var axisX = new LinearAxis();
            axisX.Key = "x";
            axisX.AbsoluteMaximum = right;
            axisX.Position = AxisPosition.Bottom;
            axisX.MinimumPadding = 0.1;
            axisX.PositionAtZeroCrossing = true;
            axisX.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            axisX.MajorGridlineStyle = LineStyle.Solid;
            axisX.MinorGridlineStyle = LineStyle.Dot;

            plotModel.Axes.Add(axisX);

            var series = new LineSeries() { MinimumSegmentLength = 2 };
            series.XAxisKey = "x";
            series.YAxisKey = "y";
            series.Color = OxyColor.FromRgb(255, 0, 0);
            series.MarkerFill = OxyColors.Red;
            //series.Smooth = true;
            plotModel.Series.Add(series);
            _series = series;

            this.plotView.Model = plotModel;
        }



        public void DisplayPlotData(PointF point)
        {
            _series.Points.Add(new DataPoint(point.X, point.Y));
            if (_series.Points.Count > _numberOfPoints)
                _series.Points.RemoveAt(0);

            plotView.InvalidatePlot(true);
        }

        public void ResetSeries()
        {
            _series.Points.Clear();
            plotView.Invalidate(true);
        }

    }
}
