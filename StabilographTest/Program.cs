using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using Stabilograph.Core;
using Stabilograph.Core.Configuration;
using Stabilograph.Core.Diagnostic;
using Stabilograph.Core.Filters;
using Stabilograph.Core.Processing;
using Stabilograph.Protocol;
using SerialPort = System.IO.Ports.SerialPort;

namespace StabilographTest
{
    internal class Program
    {
        private static Root _conf = Loader.Load();

        //TODO: remove RX
        private static void Main(string[] args)
        {
//            var serialPort = new SerialPort("COM3", 9600, Parity.None);
//            serialPort.Open();
//
//            var protocol = new Protocol(serialPort);
//            var weightPublisher = new ChannelReader(protocol);
//
//            var observer = weightPublisher.ObserveWeights(TimeSpan.FromMilliseconds(1)).Publish();
//            var disposable = observer.Connect();
//
//            var stdout = observer.Subscribe(weights => Console.WriteLine(String.Join(", ", weights)));
//
//            var channels = new List<IObservable<float>>(8);
//            for (var index = 0; index < 8; index++)
//            {
//                var chIndex = index;
//                var channel = observer.Select(values => values[chIndex])
//                    .Buffer(TimeSpan.FromSeconds(1))
//                    .Select(buf =>
//                    {
//                        if (buf.Count != 0)
//                            return buf.Average(f => f);
//                        return 0;
//                    });
//                channels.Add(channel);
//            }
//
//            var common = channels.Zip();
//
//            common.Subscribe(
//                    weights => Console.WriteLine(string.Join(", ", weights.Select(f => f.ToString("0.00")))));
//
//            var interpolators = _conf.Sensors.Select(s => new Interpolator.ChannelInterpolator(s.Interpolation)).ToList();
//            var interpolator = new Interpolator(interpolators);
//            var platformSize = _conf.Platform.Size;
//            var leftPlatform = new PlatformDiagnostic(platformSize, _conf.Sensors.Take(4).ToList(), _conf.Platform.LeftCorrection);
//            var rightPlatform = new PlatformDiagnostic(platformSize, _conf.Sensors.Skip(4).ToList(), _conf.Platform.RightCorrection);
//
//            var filtered = SortSkipTakeAvgFilter.Filter(observer, 8, 2, 4);
//            var weightObserver = interpolator.Interpolate(filtered);
//            var leftWeightObserver = weightObserver.Select(list => list.Take(4).ToList());
//            var rightWeightObserver = weightObserver.Select(list => list.Skip(4).ToList());
//
//            var leftCenterObserver = leftPlatform.Center(leftWeightObserver);
//            var rightCenterObserver = rightPlatform.Center(rightWeightObserver);
//
//            var plotDataObserver = leftCenterObserver.Zip(rightCenterObserver, Tuple.Create);
//
//            plotDataObserver.Subscribe(data => Console.WriteLine("Left: {0}, Right: {1}", data.Item1, data.Item2));
//
//            Console.ReadKey(false);
//            stdout.Dispose();
//            disposable.Dispose();
//            Console.ReadKey();
        }

        private static string BytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }
    }
}