using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Stabilograph.Core.Configuration;

namespace Stabilograph.Core.IO
{
    public class ProtocolStub : IProtocol
    {
        private readonly float[] _max;
        private readonly float[] _min;
        private readonly Random _rnd = new Random(123);
        private int _sensorIndexL;
        private int _sensorIndexR = 4;
        private readonly float _step = 5f;
        private readonly float[] _vectors;
        private readonly float[] _weights;

        public ProtocolStub(Root config)
        {
            _weights = config.Sensors.Select(s => s.Interpolation[0].X).ToArray();
            _vectors = config.Sensors.Select(s => s.Interpolation[1].X - s.Interpolation[0].X)
                .Select(v => v/Math.Abs(v))
                .ToArray();
            _max = config.Sensors.Select(s => s.Interpolation[1].X).ToArray();
            _min = config.Sensors.Select(s => s.Interpolation[0].X).ToArray();
        }

        public float[] ReadWeights()
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
                _sensorIndexL = (_sensorIndexL + 1)%8;
            }

            var valueR = _vectors[_sensorIndexR]*_step + _weights[_sensorIndexR];
            _weights[_sensorIndexR] = valueR;
            if (valueR > _max[_sensorIndexR])
            {
                _vectors[_sensorIndexR] *= -1;
            }
            if (valueR < _min[_sensorIndexR])
            {
                _vectors[_sensorIndexR] *= -1;
                _sensorIndexR = (_sensorIndexR + 1)%8;
            }

            return _weights;
        }
    }
}