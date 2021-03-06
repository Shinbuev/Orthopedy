﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stabilograph.Core;
using Stabilograph.Core.Diagnostic;

namespace Stabilograph
{
    public partial class IndicatorsForm : Form
    {
        private IObservable<PointF> _leftCenterObserver;
        private IObservable<PointF> _rightCenterObserver;
        private PlatformDiagnostic _leftPlatformDiagnostic;
        private PlatformDiagnostic _rightPlatformDiagnostic;
        private IDisposable _disposable;

        public IndicatorsForm()
        {
            InitializeComponent();
        }

        public void ResetIndicators()
        {
            Unsubscribe();

            var leftIndicators = _leftPlatformDiagnostic.Analize(_leftCenterObserver);
            var rightIndicators = _rightPlatformDiagnostic.Analize(_rightCenterObserver);

            var allIndicators = leftIndicators.Zip(rightIndicators, (indicators, indicators1) => Tuple.Create(indicators, indicators1));
            _disposable = allIndicators.ObserveOn(this).Subscribe(UpdateView);
        }

        private void UpdateView(Tuple<PlatformDiagnostic.Indicators, PlatformDiagnostic.Indicators> indicators)
        {
            var left = indicators.Item1;
            var right = indicators.Item2;

            listView1.Items[0].SubItems[1].Text = left.Length.ToString("F2");
            listView1.Items[1].SubItems[1].Text = left.AvgAmplitude.ToString();
            listView1.Items[2].SubItems[1].Text = left.MaxAmplitude.ToString();
            listView1.Items[3].SubItems[1].Text = left.Frequency.ToString();
            listView1.Items[4].SubItems[1].Text = left.Period.ToString();

            listView1.Items[0].SubItems[3].Text = right.Length.ToString("F2");
            listView1.Items[1].SubItems[3].Text = right.AvgAmplitude.ToString();
            listView1.Items[2].SubItems[3].Text = right.MaxAmplitude.ToString();
            listView1.Items[3].SubItems[3].Text = right.Frequency.ToString();
            listView1.Items[4].SubItems[3].Text = right.Period.ToString();
        }

        private void Unsubscribe()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }

        public void Prepare(PlatformDiagnostic leftPlatformDiagnostic, IObservable<PointF> leftCenterObserver, PlatformDiagnostic rightPlatformDiagnostic, IObservable<PointF> rightCenterObserver)
        {
            _leftPlatformDiagnostic = leftPlatformDiagnostic;
            _leftCenterObserver = leftCenterObserver;
            _rightPlatformDiagnostic = rightPlatformDiagnostic;
            _rightCenterObserver = rightCenterObserver;
        }

        public void CompleteDiagnostic()
        {
            if (this.InvokeRequired)
            {
                Unsubscribe();
                
            }
        }
    }
}
