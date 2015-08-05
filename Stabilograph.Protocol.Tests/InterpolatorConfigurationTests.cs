using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JsonConfig;
using NUnit.Framework;
using Stabilograph.Core;
using Stabilograph.Core.Configuration;
using Stabilograph.Core.Processing;

namespace Stabilograph.Protocol.Tests
{
    [TestFixture]
    class InterpolatorConfigurationTests
    {
        [Test]
        public void InterpolatorFromConfigurationTest()
        {
            var scope = ParseConfigurationFromResources();

            Assert.IsInstanceOf<ConfigObject>(scope);
            Assert.IsInstanceOf<ConfigObject[]>(scope.Sensors);
            
            var interpolation = scope.Sensors as ICollection<dynamic>;
            Assert.IsNotNull(interpolation);

            var channelInterpolations = interpolation
                .Select(entry => ((ICollection<dynamic>)entry.Points).Select(p => new PointF(p.Value, p.Weight)).ToList()).ToList();
                
            channelInterpolations.Count.Should().Be(8);
            channelInterpolations[0][0].ShouldBeEquivalentTo(new PointF(265f, 0));
            channelInterpolations[4][1].ShouldBeEquivalentTo(new PointF(-823, 6));
           
            var interpolator = new Interpolator(channelInterpolations.Select(_ => new Interpolator.ChannelInterpolator(_)).ToArray());
            Assert.IsNotNull(interpolator);
        }

        private static dynamic ParseConfigurationFromResources()
        {
            var jsonTests =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("Stabilograph.Protocol.Tests.settings.conf");

            var sReader = new StreamReader(jsonTests);
            var jReader = new JsonFx.Json.JsonReader();
            dynamic parsed = jReader.Read(sReader.ReadToEnd());

            dynamic scope = ConfigObject.FromExpando(parsed);
            return scope;
        }

        [Test]
        public void TimeSpanParseTest()
        {
            var ts = TimeSpan.Parse("00:00:30");
            ts.Should().Be(TimeSpan.FromSeconds(30));
        }

        [Test]
        public void ConfigurationTest()
        {
            var config = ParseConfigurationFromResources();
            var configuration = Loader.Load(config);

            var sensors = configuration.Sensors;
            sensors.Count.Should().Be(8);

            configuration.Platform.LeftCorrection.Should().Be(new PointF());
            configuration.Diagnostic.Duration.Should().Be(TimeSpan.FromSeconds(30));
        }
    }
}
