using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Drawing;
using FluentAssertions;


namespace Stabilograph.Protocol.Tests
{
    [TestFixture]
    class AnalizeLogicTests
    {
        [Test]
        public void DateTimeProviderTest() 
        {
            var dt = DateTime.Now;
            int sequence = 0;
            Func<DateTime> dft = () => dt + TimeSpan.FromTicks(TimeSpan.FromMilliseconds(100).Ticks * sequence++);

            var dt1 = dft();
            var dt2 = dft();
            (dt2 - dt1).Should().Be(TimeSpan.FromMilliseconds(100));
            (dft() - dt2).Should().Be(TimeSpan.FromMilliseconds(100));
        }

        [Test]
        public void ProcessZeros()
        {
            var dt = DateTime.Now;
            var sequence = 0;
            Func<DateTime> dtf = () => dt + TimeSpan.FromTicks(TimeSpan.FromMilliseconds(100).Ticks * sequence++);
            
            var state = new Platform.State(dtf);
            var s1 = state.ProcessNext(new PointF());
            var s2 = s1.ProcessNext(new PointF());
            var s3 = s2.ProcessNext(new PointF());

            s3.Indicators.AvgAmplitude.Should().Be(new PointF());
            s3.Indicators.Length.Should().Be(0);
            s3.Indicators.MaxAmplitude.Should().Be(new PointF());
            s3.Indicators.Frequency.Should().Be(new PointF());
            s3.Indicators.Period.Should().Be(new PointF());
            s3.Duration.Should().Be(TimeSpan.FromMilliseconds(200));
        }

        [Test]
        public void ProcessSamePoint()
        {
            var dt = DateTime.Now;
            var sequence = 0;
            Func<DateTime> dtf = () => dt + TimeSpan.FromTicks(TimeSpan.FromMilliseconds(100).Ticks * sequence++);

            var state = new Platform.State(dtf);
            var s1 = state.ProcessNext(new PointF(1, 1));
            var s2 = s1.ProcessNext(new PointF(1, 1));
            var s3 = s2.ProcessNext(new PointF(1, 1));

            s3.Indicators.AvgAmplitude.Should().Be(new PointF());
            s3.Indicators.Length.Should().Be(0);
            s3.Indicators.MaxAmplitude.Should().Be(new PointF());
            s3.Indicators.Frequency.Should().Be(new PointF());
            s3.Indicators.Period.Should().Be(new PointF());
            s3.Duration.Should().Be(TimeSpan.FromMilliseconds(200));
        }

        [Test]
        public void ProcessRectanglePoints()
        {
            var dt = DateTime.Now;
            var sequence = 0;
            Func<DateTime> dtf = () => dt + TimeSpan.FromTicks(TimeSpan.FromMilliseconds(100).Ticks * sequence++);

            var state = new Platform.State(dtf);
            var s1 = state.ProcessNext(new PointF(1, 1));
            var s2 = s1.ProcessNext(new PointF(1, -1));
            var s3 = s2.ProcessNext(new PointF(-1, -1));
            var s4 = s3.ProcessNext(new PointF(-1, 1));
            var s5 = s4.ProcessNext(new PointF(1, 1));

            //s5.Indicators.AvgAmplitude.Should().Be(new PointF(2, 2));
            s5.Indicators.Length.Should().Be(8);
            //s5.Indicators.MaxAmplitude.Should().Be(new PointF(2, 2));
            s5.Indicators.Frequency.Should().Be(new PointF(
                2/(float)TimeSpan.FromMilliseconds(400).TotalSeconds, 
                2/(float)TimeSpan.FromMilliseconds(400).TotalSeconds));
            s5.Indicators.Period.Should().Be(new PointF(
                (float)TimeSpan.FromMilliseconds(400).TotalSeconds / 2f,
                (float)TimeSpan.FromMilliseconds(400).TotalSeconds / 2f));
            s5.Duration.Should().Be(TimeSpan.FromMilliseconds(400));

            var s6 =
                s5.ProcessNext(new PointF(1, -1))
                    .ProcessNext(new PointF(-1, -1))
                    .ProcessNext(new PointF(-1, 1))
                    .ProcessNext(new PointF(1, 1));

            //s6.Indicators.AvgAmplitude.Should().Be(new PointF(2, 2));
            s6.Indicators.Length.Should().Be(16);
            //s6.Indicators.MaxAmplitude.Should().Be(new PointF(2, 2));
            s6.Indicators.Frequency.Should().Be(new PointF(
                4 / (float)TimeSpan.FromMilliseconds(800).TotalSeconds,
                4 / (float)TimeSpan.FromMilliseconds(800).TotalSeconds));
            s6.Indicators.Period.Should().Be(new PointF(
                (float)TimeSpan.FromMilliseconds(800).TotalSeconds / 4f,
                (float)TimeSpan.FromMilliseconds(800).TotalSeconds / 4f));
            s6.Duration.Should().Be(TimeSpan.FromMilliseconds(800));
        }

        [Test]
        public void ProcessRombPoints()
        {
            var dt = DateTime.Now;
            var sequence = 0;
            Func<DateTime> dtf = () => dt + TimeSpan.FromTicks(TimeSpan.FromMilliseconds(100).Ticks * sequence++);

            var state = new Platform.State(dtf);
            var s1 = state.ProcessNext(new PointF(1, 0));
            var s2 = s1.ProcessNext(new PointF(0, -1));
            var s3 = s2.ProcessNext(new PointF(-1, 0));
            var s4 = s3.ProcessNext(new PointF(0, 1));
            var s5 = s4.ProcessNext(new PointF(1, 0));

            s5.Indicators.Length.Should().Be(4 * Math.Sqrt(2));
            //s5.Indicators.MaxAmplitude.Should().Be(new PointF(2, 2));
//            s5.Indicators.Frequency.Should().Be(new PointF(
//                2 / (float)TimeSpan.FromMilliseconds(400).TotalSeconds,
//                2 / (float)TimeSpan.FromMilliseconds(400).TotalSeconds));
//            s5.Indicators.Period.Should().Be(new PointF(
//                (float)TimeSpan.FromMilliseconds(400).TotalSeconds / 2f,
//                (float)TimeSpan.FromMilliseconds(400).TotalSeconds / 2f));
            s5.Duration.Should().Be(TimeSpan.FromMilliseconds(400));

            var s6 =
                s5.ProcessNext(new PointF(0, -1))
                    .ProcessNext(new PointF(-1, 0))
                    .ProcessNext(new PointF(0, 1))
                    .ProcessNext(new PointF(1, 0));

            //s6.Indicators.AvgAmplitude.Should().Be(new PointF(1, 1));
            //s6.Indicators.Length.Should().Be(8 * Math.Sqrt(2));
            //s6.Indicators.MaxAmplitude.Should().Be(new PointF(1, 1));
//            s6.Indicators.Frequency.Should().Be(new PointF(
//                4 / (float)TimeSpan.FromMilliseconds(800).TotalSeconds,
//                4 / (float)TimeSpan.FromMilliseconds(800).TotalSeconds));
//            s6.Indicators.Period.Should().Be(new PointF(
//                (float)TimeSpan.FromMilliseconds(800).TotalSeconds / 4f,
//                (float)TimeSpan.FromMilliseconds(800).TotalSeconds / 4f));
            s6.Duration.Should().Be(TimeSpan.FromMilliseconds(800));
        }

    }
}
