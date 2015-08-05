using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonConfig;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Stabilograph.Protocol;
using Stabilograph.Protocol.Configuration;
using SerialPort = System.IO.Ports.SerialPort;

namespace Stabilograph
{
    public partial class MainForm : Form
    {
        private Configuration _configuration;
        private PatientForm _patientForm;
        private ChannelsForm _channelsForm;
        
        public MainForm()
        {
            InitializeComponent();
            InitializePlot();

            _configuration = LoadConfiguration();
            InitializeSerialPort();
            
            this.SerialPortOpened +=SerialPortOpenedHandler;

            SuspendLayout();

            notifyIcon1.Icon = SystemIcons.Application;
            
            InitializePatientForm();
            InitializeIndicatorsForm();
            this.Load += ShowPatientForm;
            this.Load += ShowIndicatorForm;

            ResumeLayout(true);
        }

        private void InitializeIndicatorsForm()
        {
            var form = new IndicatorsForm();
            form.TopLevel = false;
            form.Parent = this;
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Controls.Add(form);

            form.Closed += OnChildFormClosed;
            Interlocked.Exchange(ref _indicatorsForm, form);
        
        }

        private void SerialPortOpenedHandler(object sender, EventArgs e)
        {
            var weightPublisher = sender as IObservable<List<float>>;

            if (weightPublisher != null)
            {
                Interlocked.Exchange(ref _weightObserver, weightPublisher.Publish());
                Interlocked.Exchange(ref _observerDisposable, _weightObserver.Connect());

                InitializeChannelForm(_weightObserver);
                ShowChannelForm(this, EventArgs.Empty);

                InitializeProcessing(_weightObserver);
            }
        }

        private void InitializeProcessing(IConnectableObservable<List<float>> channelObserver)
        {
            var interpolators = _configuration.Sensors.Select(s => new Interpolator.ChannelInterpolator(s.Interpolation)).ToList();
            var interpolator = new Interpolator(interpolators);
            var platformSize = _configuration.Platform.Size;
            var leftPlatform = new Protocol.Platform(platformSize, _configuration.Sensors.Take(4).ToList(), _configuration.Platform.LeftCorrection);
            var rightPlatform = new Protocol.Platform(platformSize, _configuration.Sensors.Skip(4).ToList(), _configuration.Platform.RightCorrection);

            var filtered = Protocol.Filters.CurrentAvfFilter.Filter(channelObserver, 10);
            var weightObserver = interpolator.Interpolate(filtered);
            var leftWeightObserver = weightObserver.Select(list => list.Take(4).ToList());
            var rightWeightObserver = weightObserver.Select(list => list.Skip(4).ToList());

            var leftCenterObserver = leftPlatform.Center(leftWeightObserver);
            var rightCenterObserver = rightPlatform.Center(rightWeightObserver);

            _indicatorsForm.Prepare(leftPlatform, leftCenterObserver, rightPlatform, rightCenterObserver);

            var plotDataObserver = leftCenterObserver.ObserveOn(Scheduler.Default).Zip(rightCenterObserver,
                (l, r) => new PlotData() {LeftPoint = l, RightPoint = r});

            StartTracking(plotDataObserver);
            
        }

        private void InitializeSerialPort()
        {
            serialPort.PortName = _configuration.Port.Name;
            serialPort.BaudRate = _configuration.Port.Baudrate;
            serialPort.Parity = _configuration.Port.Parity;
        }

        private Configuration LoadConfiguration()
        {
            try
            {
                return Configuration.Load();

            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot load configuration. Reason: " + e.ToString() + "Application will be closed", "Configuration Error", MessageBoxButtons.OK);
                Close();
                return null;
            }
        }

        private void InitializePatientForm()
        {
            var form = new PatientForm();
            form.TopLevel = false;
            form.Parent = this;
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Controls.Add(form);

            form.Closed += OnChildFormClosed;
            Interlocked.Exchange(ref _patientForm, form);
        }

        private void ShowPatientForm(object sender, EventArgs e)
        {
            var form = _patientForm;
            if (form != null)
            {
                try
                {
                    form.Top = this.ClientRectangle.Top + 30;
                    form.Left = this.ClientRectangle.Right - form.Width - 5;

                    form.Show();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(@"Cannot show PatientForm: " + exception.Message);
                }
            }
        }

        private void ShowIndicatorForm(object sender, EventArgs e)
        {
            var form = _indicatorsForm;
            if (form != null)
            {
                try
                {
                    form.Top = this.ClientRectangle.Bottom - form.Height - 30;
                    form.Left = this.ClientRectangle.Right - form.Width - 5;

                    form.Show();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(@"Cannot show PatientForm: " + exception.Message);
                }
            }
        }
        

        private void InitializeChannelForm(IObservable<List<float>> observable)
        {
            //mock
            //IObservable<List<float>> mock = Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(tick => NextValues(tick));
            
            var form = new ChannelsForm(observable);
            form.TopLevel = false;
            form.Parent = this;
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Controls.Add(form);
            
            form.Closed += OnChildFormClosed;
            Interlocked.Exchange(ref _channelsForm, form);
        }

        void OnChildFormClosed(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (form != null)
            {
                Debug.WriteLine("{0} form is closed", form.Name);
                if (Controls.Contains(form))
                {
                    Controls.Remove(form);
                    this.Load -= ShowChannelForm;
                }
            }
        }

        void ShowChannelForm(object sender, EventArgs e)
        {
            var form = _channelsForm;
            if (form != null)
            {
                try
                {
                    form.Top = ClientRectangle.Bottom - form.Height;
                    form.Show();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(@"Cannot show ChannelForm: " + exception.Message);
                }
            }
        }

        private int mockValue = 0;
        private IDisposable _observerDisposable;
        private IConnectableObservable<List<float>> _weightObserver;
        private IndicatorsForm _indicatorsForm;

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.serialPortTimer.Interval = _configuration.Port.OpenInterval;
            this.serialPortTimer.Enabled = true;
        }

        void ShowPortSettings(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(3000, "Port Setting",
                String.Format("Setting are applied to the serial port. Name: {0}, Baudrate: {1}, Parity: {2}",
                    serialPort.PortName, serialPort.BaudRate, serialPort.Parity),
                ToolTipIcon.Info);

            notifyIcon1.BalloonTipClosed -= ShowPortSettings;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //channels
            if (_channelsForm.IsDisposed)
            {
                InitializeChannelForm(_weightObserver);
                ShowChannelForm(this, EventArgs.Empty);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //patient
            if (_patientForm.IsDisposed)
            {
                InitializePatientForm();
                ShowPatientForm(this, EventArgs.Empty);
            }
        }

        private void OpenSerialPort(object sender, EventArgs e)
        {
            try
            {
                //var protocol = new Protocol.Protocol(serialPort);
                //var reader = new ChannelReader(protocol);
                //var channels = reader.ReadWeights();
                var reader = new ChannelReaderStub(_configuration);

                ShowPortSettings(this, EventArgs.Empty);
                serialPortTimer.Enabled = false;

                OnSerialPortOpened(reader.AsObservable(TimeSpan.FromMilliseconds(100)));
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Cannot open serial port {0}: {1}", serialPort.PortName, exception.ToString());
            }
        }

        private event EventHandler SerialPortOpened;
        
        protected void OnSerialPortOpened(IObservable<List<float>> readerObservable)
        {
            var handler = SerialPortOpened;
            if (handler != null) handler(readerObservable, EventArgs.Empty);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripButton3.Enabled = false;
                ResetSeries();
                _indicatorsForm.ResetIndicators();

                _diagnosticTimer.Tick += CompleteDiagnostic;
                _diagnosticTimer.Interval = (int) _configuration.Diagnostic.Duration.TotalMilliseconds;
                _diagnosticTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Run Test");
            }
        }

        private void CompleteDiagnostic(object sender, EventArgs e)
        {
            try
            {
                toolStripButton3.Enabled = true;
                _diagnosticTimer.Enabled = false;
                _diagnosticTimer.Tick -= CompleteDiagnostic;
                _indicatorsForm.CompleteDiagnostic();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Stop Test");
            }

        }
    }
}
