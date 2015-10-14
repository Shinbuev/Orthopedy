using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonConfig;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Stabilograph.Core;
using Stabilograph.Core.Configuration;
using Stabilograph.Core.Diagnostic;
using Stabilograph.Core.Filters;
using Stabilograph.Core.IO;
using Stabilograph.Core.Processing;
using SerialPort = System.IO.Ports.SerialPort;

namespace Stabilograph
{
    public partial class MainForm : Form
    {
        private Root _config;
        private PatientForm _patientForm;
        private List<Interpolator.ChannelInterpolator> _channellInterpolators;
        private Interpolator _interpolator;
        SizeF _platformSize;
        PlatformDiagnostic _leftPlatformDiagnostic;
        PlatformDiagnostic _rightPlatformDiagnostic;
        PlatformDiagnostic.State _leftState = new PlatformDiagnostic.State();
        PlatformDiagnostic.State _rightState = new PlatformDiagnostic.State();

        public MainForm()
        {
            InitializeComponent();
            InitializePlot();

            _config = LoadConfiguration();
            _channellInterpolators = _config
                .Sensors.Select(s => new Interpolator.ChannelInterpolator(s.Interpolation)).ToList();
            _interpolator = new Interpolator(_channellInterpolators);
            _platformSize = _config.Platform.Size;
            _leftPlatformDiagnostic = new PlatformDiagnostic(_platformSize, _config.Sensors.Take(4).ToList(), _config.Platform.LeftCorrection);
            _rightPlatformDiagnostic = new PlatformDiagnostic(_platformSize, _config.Sensors.Skip(4).ToList(), _config.Platform.RightCorrection);

            InitializeSerialPort();

            this.SerialPortOpened += SerialPortOpenedHandler;

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
            var protocol = sender as IProtocol;
            if (protocol != null)
            {
                InitializeProcessing(protocol);
            }
        }


        private void InitializeProcessing(IProtocol protocol)
        {
            readerTimer.Tag = protocol;
            readerTimer.Enabled = true;
        }

        private void InitializeSerialPort()
        {
            serialPort.PortName = _config.Port.Name;
            serialPort.BaudRate = _config.Port.Baudrate;
            serialPort.Parity = _config.Port.Parity;
        }

        private Root LoadConfiguration()
        {
            try
            {
                return Loader.Load();

            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot load configuration. Reason: " + e.ToString() + "Application will be closed", "Loader Error", MessageBoxButtons.OK);
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

        void OnChildFormClosed(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (form != null)
            {
                Debug.WriteLine("{0} form is closed", form.Name);
                if (Controls.Contains(form))
                {
                    Controls.Remove(form);
                    Refresh();
                }
            }
        }



        private int mockValue = 0;
        private IDisposable _observerDisposable;
        private IndicatorsForm _indicatorsForm;

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.serialPortTimer.Interval = _config.Port.OpenInterval;
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
                IProtocol reader;
                if (_config.Debug)
                {
                    reader = new ProtocolStub(_config);
                }
                else
                {
                    reader = new Protocol(serialPort);
                }
                ShowPortSettings(this, EventArgs.Empty);
                serialPortTimer.Enabled = false;

                OnSerialPortOpened(reader);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Cannot open serial port {0}: {1}", serialPort.PortName, exception.ToString());
            }
        }

        private event EventHandler SerialPortOpened;

        protected void OnSerialPortOpened(IProtocol reader)
        {
            var handler = SerialPortOpened;
            if (handler != null) handler(reader, EventArgs.Empty);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripButton3.Enabled = false;
                ResetSeries();
                //_indicatorsForm.ResetIndicators();

                _diagnosticTimer.Tick += CompleteDiagnostic;
                _diagnosticTimer.Interval = (int)_config.Diagnostic.Duration.TotalMilliseconds;
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
                //_indicatorsForm.CompleteDiagnostic();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Stop Test");
            }

        }

        //private LinkedList<float[]> _weightsBuffer = new LinkedList<float[]>();
        private void ReadWeightsFromProtocolAndUpdateModel(object sender, EventArgs e)
        {
            var reader = readerTimer.Tag as IProtocol;

            if (reader != null)
            {
                var weights = reader.ReadWeights();
                UpdateModel(weights.ToList());
            }
        }

        public PlotData _plotData = new PlotData();
        private void UpdateModel(List<float> weights)
        {
            var interpolatedWeights = _interpolator.Interpolate(weights);
            var leftWeights = interpolatedWeights.Take(4).ToList();
            var rightWeights = interpolatedWeights.Skip(4).ToList();
            var leftCenter = _leftPlatformDiagnostic.CalculateCenterOf(leftWeights);
            var rightCenter = _rightPlatformDiagnostic.CalculateCenterOf(rightWeights);
            _leftState.ProcessNext(leftCenter);
            _rightState.ProcessNext(rightCenter);
            
            _indicatorsForm.UpdateView(_leftState.Indicators, _rightState.Indicators);
            var data = new PlotData() {LeftPoint = leftCenter, RightPoint = rightCenter};
            Interlocked.Exchange(ref _plotData, data);
        }


        private void updatePlotTimer_Tick(object sender, EventArgs e)
        {
            if (updatePlotTimer.Tag != _plotData)
            {
                updatePlotTimer.Tag = _plotData;
                this.DisplayPlotData(_plotData);
            }
        }
    }
}
