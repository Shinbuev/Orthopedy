using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stabilograph.Core;
using Stabilograph.Core.Diagnostic;

namespace Stabilograph
{
    public partial class IndicatorsForm : Form
    {
        public IndicatorsForm()
        {
            InitializeComponent();
        }

        public void UpdateView(PlatformDiagnostic.Indicators left, PlatformDiagnostic.Indicators right)
        {
            if (InvokeRequired)
            {
                Delegate d = new MethodInvoker(() => UpdateView(left, right));
                Invoke(d, new object[] { left, right });
            }
            else
            {
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
        }
    }
}
