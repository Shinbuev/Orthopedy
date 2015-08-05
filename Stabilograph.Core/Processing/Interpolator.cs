using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;

namespace Stabilograph.Core.Processing
{
    public class Interpolator
    {
        private readonly ICollection<ChannelInterpolator> _interpolators;

        public Interpolator(ICollection<ChannelInterpolator> interpolators)
        {
            Contract.Assert(interpolators != null, "interpolators != null");
            Contract.Assert(interpolators.Count == 8, "interpolators.count == 8");
            _interpolators = interpolators;
        }

        public IObservable<List<float>> Interpolate(IObservable<List<float>> inputs)
        {
            var output = inputs.Select(values => _interpolators.Zip(values, (interpolator, value) => interpolator.Interpolate(value)).ToList());
            return output;
        } 

        /// <summary>
        ///     Each point in _points corresponds to (X, Y) => (channel, weight)
        /// </summary>
        public class ChannelInterpolator
        {
            private readonly IList<PointF> _points;

            public ChannelInterpolator(IList<PointF> points)
            {
                Contract.Assert(points != null, "points != null");
                Contract.Assert(points.Count >= 1, "points.Count >= 1");
                Contract.Assert(points.Select(point => point.X).Distinct().Count() == points.Count,
                    "points.X should be unique");
                _points = points.ToList();
            }

            public float Interpolate(float value)
            {
                if (value < _points.First().X)
                {
                    return Math.Max(0, Interpolate(value,
                        _points[0],
                        _points[1]));
                }
                if (value > _points.Last().X)
                {
                    return Interpolate(value,
                        _points[_points.Count - 2],
                        _points[_points.Count - 1]);
                }
                return Interpolate(value,
                    _points.First(point => point.X <= value),
                    _points.First(point => point.X >= value));
            }

            private float Interpolate(float value, PointF left, PointF right)
            {
                if (left.Equals(right))
                    return left.Y;
                //y = y0 + (y1 - y0)(x - x0)/(x1 - x0)
                //left == point0, right == point1
                var y = left.Y + (right.Y - left.Y)*(value - left.X)/(right.X - left.X);
                return y;
            }
        }
    }
}