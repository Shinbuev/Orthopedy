using System.Drawing;
using FluentAssertions;
using NUnit.Framework;
using Stabilograph.Core;
using Stabilograph.Core.Processing;

namespace Stabilograph.Protocol.Tests
{
    [TestFixture]
    public class InterpolatorLogicTests
    {
        [Test]
        public void InterpolateBetweenPointsTest()
        {
            var interpolator = new Interpolator.ChannelInterpolator(new[] {new PointF(0, 0), new PointF(1, 1)});
            interpolator.Interpolate(0.1f).ShouldBeEquivalentTo(0.1f);
            interpolator.Interpolate(0.5f).ShouldBeEquivalentTo(0.5f);
        }

        [Test]
        public void InterpolateInvertedTest()
        {
            var interpolator = new Interpolator.ChannelInterpolator(new[] {new PointF(-1000, 0), new PointF(-2000, 1)});
            interpolator.Interpolate(-1000f).ShouldBeEquivalentTo(0);
            interpolator.Interpolate(-1500f).ShouldBeEquivalentTo(0.5f);
        }

        [Test]
        public void InterpolateOnPointsTest()
        {
            var interpolator = new Interpolator.ChannelInterpolator(new[] {new PointF(0, 0), new PointF(1, 1)});
            interpolator.Interpolate(0).ShouldBeEquivalentTo(0f);
            interpolator.Interpolate(1).ShouldBeEquivalentTo(1f);
        }

        [Test]
        public void InterpolateOutOfPointsTest()
        {
            var interpolator = new Interpolator.ChannelInterpolator(new[] {new PointF(0, 0), new PointF(1, 1)});
            interpolator.Interpolate(-1f).ShouldBeEquivalentTo(0f);
            interpolator.Interpolate(5f).ShouldBeEquivalentTo(5f);
        }
    }
}