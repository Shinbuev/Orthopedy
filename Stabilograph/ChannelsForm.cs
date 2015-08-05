using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace Stabilograph
{
    public partial class ChannelsForm : Form
    {
        private readonly PlotModel _model = new PlotModel();
        private readonly int _numberOfPoints = 300;
        private readonly IObservable<List<float>> _observable;
        private readonly PlotView _plot = new PlotView();
        private IDisposable _disposable;
        
        public ChannelsForm(IObservable<List<float>> observable)
        {
            _observable = observable;
            this.SuspendLayout();
            InitializeComponent();
            try
            {
                InitializePlot();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Channel form creation error: " + e.ToString());
            }
            this.ResumeLayout(true);
        }

        private void InitializePlot()
        {
            _model.PlotType = PlotType.XY;

            for (var index = 0; index < 7; index++)
            {
                var s = new LineSeries();
                s.Title = "C" + index;
                s.Smooth = true;
                s.LineLegendPosition = LineLegendPosition.Start;
                
                _model.Series.Add(s);
            }

            var axisY = new LinearAxis();
            axisY.Position = AxisPosition.Left;
            _model.Axes.Add(axisY);
            axisY.MinimumPadding = 0.1;
            axisY.Position = AxisPosition.Right;
            axisY.MajorGridlineStyle = LineStyle.Solid;
            axisY.MinorGridlineStyle = LineStyle.Dot;
            
            var axisX = new LinearAxis();
            axisX.Position = AxisPosition.Bottom;
            axisX.IsAxisVisible = false;
            axisX.MinimumPadding = 0.1;
            _model.Axes.Add(axisX);

            _model.LegendPlacement = LegendPlacement.Outside;
            _model.LegendPosition = LegendPosition.TopLeft;
            _model.LegendOrientation = LegendOrientation.Horizontal;

            _plot.Model = _model;
            _plot.Dock = DockStyle.Fill;

            Controls.Add(_plot);
        }

        private void ChannelsForm_Activated(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref _disposable, _observable.ObserveOn(this).Subscribe(UpdatePlot, CloseOnError));
            Debug.WriteLine("Subscribing to channels on activation of ChannelForm");
        }

        private void UpdatePlot(IList<float> values)
        {
            var ts = DateTime.Now.TimeOfDay.TotalMilliseconds * 0.001d;
            for (var index = 0; index < 7; index++)
            {
                var s = (LineSeries) _model.Series[index];
                s.Points.Add(new DataPoint(ts, values[index]));
                if (s.Points.Count > _numberOfPoints)
                    s.Points.RemoveAt(0);
            }

            _plot.InvalidatePlot(true);
        }

        private void CloseOnError(Exception e)
        {
            Unsubscribe();
            Close();
        }

        private void Unsubscribe()
        {
            var disposable = Interlocked.Exchange(ref _disposable, null);
            if (disposable != null)
            {
                Debug.WriteLine("Closing subscription on closing ChannelForm");
                disposable.Dispose();
            }
        }

        private void ChannelsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Unsubscribe();
        }
    }
}