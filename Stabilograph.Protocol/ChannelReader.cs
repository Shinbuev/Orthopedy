using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Stabilograph.Protocol
{
    public interface IChannelReader
    {
        List<float> ReadWeights();
    }

    public static class ObserwableChannelReader
    {
        public static IObservable<List<float>> AsObservable(this IChannelReader reader, TimeSpan interval)
        {
            return Observable.Interval(interval)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(tick => reader.ReadWeights());
        } 
    }

    public class ChannelReaderStub: IChannelReader
    {
        private float _step = 5f;
        private int _sensorIndexL = 0;
        private int _sensorIndexR = 4;
        private List<float> _weights;
        private List<float> _vectors;
        private float[] _max;
        private float[] _min;

        public ChannelReaderStub(Configuration.Configuration config)
        {
            _weights = config.Sensors.Select(s => s.Interpolation[0].X).ToList();
            _vectors = config.Sensors.Select(s => s.Interpolation[1].X - s.Interpolation[0].X)
                .Select(v => v / Math.Abs(v))
                .ToList();
            _max = config.Sensors.Select(s => s.Interpolation[1].X).ToArray();
            _min = config.Sensors.Select(s => s.Interpolation[0].X).ToArray();
        }

        Random _rnd = new Random(123);
            
        public List<float> ReadWeights()
        {
            _sensorIndexL = _rnd.Next(0, 7);
            _sensorIndexR = _rnd.Next(0, 7);

            var valueL = _vectors[_sensorIndexL]*_step + _weights[_sensorIndexL];
            _weights[_sensorIndexL] = valueL;
            if (valueL > _max[_sensorIndexL])
            {
                _vectors[_sensorIndexL] *= -1;
            }
            if (valueL < _min[_sensorIndexL])
            {
                _vectors[_sensorIndexL] *= -1;
                _sensorIndexL = (_sensorIndexL + 1) % 8;
            }

            var valueR = _vectors[_sensorIndexR] * _step + _weights[_sensorIndexR];
            _weights[_sensorIndexR] = valueR;
            if (valueR > _max[_sensorIndexR])
            {
                _vectors[_sensorIndexR] *= -1;
            }
            if (valueR < _min[_sensorIndexR])
            {
                _vectors[_sensorIndexR] *= -1;
                _sensorIndexR = (_sensorIndexR + 1) % 8;
            }

            return _weights;
        }
    }

    public class ChannelReader: IChannelReader
    {
        private readonly Protocol _protocol;
        private readonly TimeSpan _delay;

        public ChannelReader(Protocol protocol)
        {
            _protocol = protocol;
            protocol.Initialize();
        }

        public List<float> ReadWeights()
        {
            return ReadWeights(_protocol.ReadChannels());
        } 

        public List<float> ReadWeights(int[] channels)
        {
            var values = new List<float>();

            for (int index = 0; index < channels.Length/2; index ++)
            {
                values.Add(channels[index * 2] - channels[index * 2 + 1]);
            }

            Debug.WriteLine("Channels combined " + String.Join(", ", values));
            
            return values;
        }

        public IObservable<List<float>> ObserveWeights(TimeSpan delay)
        {
            return Observable.Interval(delay)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(tick => ReadWeights());
        }
    }
}
