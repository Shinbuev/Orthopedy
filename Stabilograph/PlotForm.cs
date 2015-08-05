using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonConfig;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Stabilograph
{
    public partial class MainForm
    {
        public struct PlotData
        {
            public PointF LeftPoint;
            public PointF RightPoint;
            public PointF Center;
            public PointF LeftPointAverage;
            public PointF RightPointAverage;
            public PointF CenterAverage;
        }

        private double PlatformHeight = Config.Global.Platform.Height;
        private double PlatformWidth = Config.Global.Platform.Width;
        private LineSeries _leftSeries;
        private LineSeries _rightSeries;
        private int _numberOfPoints = 1000;

        private OxyPlot.WindowsForms.PlotView plotView;

        public void InitializePlot()
        {
            this.plotView = new OxyPlot.WindowsForms.PlotView();
            // 
            // plotView
            // 
            this.plotView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plotView.Location = new System.Drawing.Point(0, 0);
            this.plotView.Name = "plotView";
            this.plotView.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView.Size = new System.Drawing.Size(434, 315);
            this.plotView.TabIndex = 0;
            this.plotView.Text = "plotView";
            this.plotView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // PlotForm
            // 
            this.Controls.Add(this.plotView);

            var greyColor = OxyColor.FromRgb(0, 0, 0);
            var top = PlatformHeight / 2;
            var bottom = -top;
            var right = PlatformWidth / 2;
            var left = -right;

            var myModel = new PlotModel();
            myModel.PlotType = PlotType.Cartesian;
            //myModel.Series.Add(new FunctionSeries(d => PlatformHeight * Math.Cos(d) / 2, left, right, 0.1, "cos(x)"));

            var leftFootPlatformSeries = new LineSeries();
            leftFootPlatformSeries.Points.Add(new DataPoint(left, bottom));
            leftFootPlatformSeries.Points.Add(new DataPoint(left, top));
            leftFootPlatformSeries.Points.Add(new DataPoint(right, top));
            leftFootPlatformSeries.Points.Add(new DataPoint(right, bottom));
            leftFootPlatformSeries.Points.Add(new DataPoint(left, bottom));
            leftFootPlatformSeries.LineJoin = LineJoin.Round;
            leftFootPlatformSeries.Color = greyColor;

            leftFootPlatformSeries.XAxisKey = "x1";
            leftFootPlatformSeries.YAxisKey = "y1";            
            myModel.Series.Add(leftFootPlatformSeries);

            var rightFootPlatformSeries = new LineSeries();
            rightFootPlatformSeries.Points.Add(new DataPoint(left, bottom));
            rightFootPlatformSeries.Points.Add(new DataPoint(left, top));
            rightFootPlatformSeries.Points.Add(new DataPoint(right, top));
            rightFootPlatformSeries.Points.Add(new DataPoint(right, bottom));
            rightFootPlatformSeries.Points.Add(new DataPoint(left, bottom));
            rightFootPlatformSeries.Color = greyColor;

            rightFootPlatformSeries.XAxisKey = "x2";
            rightFootPlatformSeries.YAxisKey = "y2";
            myModel.Series.Add(rightFootPlatformSeries);


            var axisY = new LinearAxis();
            axisY.Key = "y1";
            //axisY.Maximum = top+ 10;
            //axisY.Minimum = bottom - 10;
            //axisY.AbsoluteMaximum = top + 10;
            //axisY.AbsoluteMinimum = bottom - 10;
            axisY.MinimumPadding = 0.1;
            //axisY.PositionAtZeroCrossing = true;
            axisY.Position = AxisPosition.Left;
            //axisY.TickStyle = OxyPlot.Axes.TickStyle.Crossing;      
            //axisY.AbsoluteMinimum = bottom -1;
            //axisY.AbsoluteMaximum = top + 1;
            axisY.MajorGridlineStyle = LineStyle.Solid;
            axisY.MinorGridlineStyle = LineStyle.Dot;
            myModel.Axes.Add(axisY);


            var leftX = new LinearAxis();
            leftX.EndPosition = 0.5;
            leftX.Key = "x1";
            //leftX.Maximum = right;
            //leftX.Minimum = left;
            leftX.AbsoluteMaximum = right;
            //leftX.AbsoluteMinimum = left - 1;
            leftX.Position = AxisPosition.Bottom;
            leftX.MinimumPadding = 0.1;
            //leftX.PositionAtZeroCrossing = true;
            //leftX.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            leftX.MajorGridlineStyle = LineStyle.Solid;
            leftX.MinorGridlineStyle = LineStyle.Dot;
            
            myModel.Axes.Add(leftX);

            var rightX = new LinearAxis();
            rightX.Key = "x2";
            //rightX.Maximum = right;
            //rightX.Minimum = left;
            //rightX.AbsoluteMaximum = right + 1;
            rightX.AbsoluteMinimum = left;
            rightX.Position = AxisPosition.Bottom;
            rightX.MinimumPadding = 0.1;
            //rightX.PositionAtZeroCrossing = true;
            //rightX.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            rightX.StartPosition = 0.5;
            rightX.MajorGridlineStyle = LineStyle.Solid;
            rightX.MinorGridlineStyle = LineStyle.Dot;
            
            myModel.Axes.Add(rightX);

            var topX = new LinearAxis();
            topX.Key = "x";
            topX.Maximum = right;
            topX.Minimum = left;
            //rightX.AbsoluteMaximum = 2* right + 10;
            //rightX.AbsoluteMinimum = 2* left - 10;
            topX.Position = AxisPosition.Top;
            topX.MinimumPadding = 0.1;
            //rightX.PositionAtZeroCrossing = true;
            //rightX.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            //rightX.StartPosition = 0.5;
            topX.MajorGridlineStyle = LineStyle.Solid;
            topX.MinorGridlineStyle = LineStyle.Dot;

            myModel.Axes.Add(topX);


            var axisY2 = new LinearAxis();
            axisY2.Key = "y2";
            //axisY2.Maximum = top;
            //axisY2.Minimum = bottom;
            axisY2.Position = AxisPosition.Right;
            axisY2.MinimumPadding = 0.1;
            //axisY2.PositionAtZeroCrossing = true;
            //axisY2.TickStyle = OxyPlot.Axes.TickStyle.Crossing;
            //axisY2.AbsoluteMinimum = bottom - 10;
            //axisY2.AbsoluteMaximum = top + 10;
            axisY2.MajorGridlineStyle = LineStyle.Solid;
            axisY2.MinorGridlineStyle = LineStyle.Dot;
            
            myModel.Axes.Add(axisY2);

            //var secondSeries = new FunctionSeries(d => PlatformHeight * Math.Sin(d) / 2, left, right, 0.1, "sin(x)");
            //secondSeries.XAxisKey = "x2";
            //secondSeries.YAxisKey = "y2";
            //myModel.Series.Add(secondSeries);

            var leftSeries = new LineSeries();
            leftSeries.XAxisKey = "x1";
            leftSeries.YAxisKey = "y1";
            leftSeries.Color = OxyColor.FromRgb(255, 0, 0);
            leftSeries.MarkerFill = OxyColors.Red;
//            leftSeries.MarkerSize = 1;
//            leftSeries.MarkerStroke = OxyColors.Red;
//            leftSeries.MarkerType = MarkerType.Circle;
            leftSeries.Smooth = true;
            myModel.Series.Add(leftSeries);
            _leftSeries = leftSeries;

            var rightSeries = new LineSeries();
            rightSeries.XAxisKey = "x2";
            rightSeries.YAxisKey = "y2";
            rightSeries.Color = OxyColor.FromRgb(255, 0, 0);
            rightSeries.MarkerFill = OxyColors.Red;
//            rightSeries.MarkerSize = 1;
//            rightSeries.MarkerStroke = OxyColors.Red;
//            rightSeries.MarkerType = MarkerType.Circle;
            rightSeries.Smooth = true;
            myModel.Series.Add(rightSeries);
            _rightSeries = rightSeries;
            //plotView.Enabled = false;

            this.plotView.Model = myModel;
            

        }

        public void StartTracking(IObservable<PlotData> data)
        {
            var disposable = data.ObserveOn(this).Subscribe(DisplayPlotData);
        }

        public void DisplayPlotData(PlotData data)
        {
            //_leftSeries.Points.Clear();
            //_rightSeries.Points.Clear();

            _leftSeries.Points.Add(new DataPoint(data.LeftPoint.X, data.LeftPoint.Y));
            if (_leftSeries.Points.Count > _numberOfPoints)
                _leftSeries.Points.RemoveAt(0);
            
            _rightSeries.Points.Add(new DataPoint(data.RightPoint.X, data.RightPoint.Y));
            if (_rightSeries.Points.Count > _numberOfPoints)
                _rightSeries.Points.RemoveAt(0);

            Debug.WriteLine("Centers: {0}, {1}", data.LeftPoint, data.RightPoint);

            this.plotView.InvalidatePlot(true);
            
        }

        public void ResetSeries()
        {
            _leftSeries.Points.Clear();
            _rightSeries.Points.Clear();
            plotView.Invalidate(true);
        }
    }
}
