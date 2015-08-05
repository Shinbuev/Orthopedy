using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using Stabilograph.Core.Configuration;
using Stabilograph.Core.Utils;

namespace Stabilograph.Core.Diagnostic
{
    public class PlatformDiagnostic
    {
        private readonly List<Sensor> _sensors;
        private readonly PointF _correction;

        public class State
        {
            private readonly Func<DateTime> _currentTime;

            public State()
                : this(() => { return DateTime.Now; })
            {
            }

            public State(Func<DateTime> currentTime)
            {
                _currentTime = currentTime;
            }

            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public TimeSpan Duration
            {
                get { return EndTime - StartTime; }
            }

            public PointF AvgCenter { get; set; }
            public PointF MaxCenter { get; set; }
            public PointF MinCenter { get; set; }
            public long Count { get; set; }
            public PointF CrossingCount { get; set; }
            public PointF? LastCenter { get; set; }
            public Indicators Indicators { get; set; }
            public readonly List<double> XAmplitudes = new List<double>();
            public readonly List<double> YAmplitudes = new List<double>();
            public State ProcessNext(PointF next)
            {
                var currentDateTime = _currentTime();
                if (StartTime == default(DateTime))
                    StartTime = currentDateTime;
                EndTime = currentDateTime;

                AvgCenter = new PointF(
                    AvgCenter.X * Count / (Count + 1) + next.X / (Count + 1),
                    AvgCenter.Y * Count / (Count + 1) + next.Y / (Count + 1)
                    );
                MaxCenter = new PointF(Math.Max(MaxCenter.X, next.X), Math.Max(MaxCenter.Y, next.Y));
                MinCenter = new PointF(Math.Min(MaxCenter.X, next.X), Math.Min(MaxCenter.Y, next.Y));

                if (LastCenter.HasValue)
                {
                    if (LastCenter.Value.X < AvgCenter.X && AvgCenter.X <= next.X)
                    {
                        XAmplitudes.Add(Math.Abs(MinCenter.X - -AvgCenter.X)* 2);
                        MinCenter = new PointF(AvgCenter.X, MinCenter.Y);
                    }

                    if (LastCenter.Value.X > AvgCenter.X && AvgCenter.X >= next.X)
                    {
                        XAmplitudes.Add(Math.Abs(MaxCenter.X - AvgCenter.X) *2);
                        MaxCenter = new PointF(AvgCenter.X, MaxCenter.Y);
                    }

                    if (LastCenter.Value.Y < AvgCenter.Y && AvgCenter.Y <= next.Y)
                    {
                        YAmplitudes.Add(Math.Abs(MinCenter.Y - AvgCenter.Y)*2);
                        MinCenter = new PointF(MinCenter.X, AvgCenter.Y);
                    }

                    if (LastCenter.Value.Y > AvgCenter.Y && AvgCenter.Y >= next.Y)
                    {
                        YAmplitudes.Add(Math.Abs(MaxCenter.Y - AvgCenter.Y)*2);
                        MaxCenter = new PointF(MaxCenter.X, AvgCenter.Y);
                    }
                }
                
                var length = LastCenter.HasValue ? Indicators.Length + next.DistanceTo(LastCenter.Value) : Indicators.Length;
                var frequencyX = XAmplitudes.Count / (EndTime - StartTime).TotalSeconds;
                var frequencyY = YAmplitudes.Count / (EndTime - StartTime).TotalSeconds;
                var periodX = XAmplitudes.Count == 0 ? 0 : 1 / frequencyX;
                var periodY = YAmplitudes.Count == 0 ? 0 : 1 / frequencyY;

                Indicators = new Indicators
                {
                    Length = length,
                    Frequency = new PointF((float)frequencyX, (float)frequencyY),
                    Period = new PointF((float)periodX, (float)periodY),
                    AvgAmplitude = new PointF(
                        XAmplitudes.Count == 0 ? 0f : (float)XAmplitudes.Average(),
                        YAmplitudes.Count == 0 ? 0f : (float)YAmplitudes.Average()),
                    MaxAmplitude = new PointF(
                        XAmplitudes.Count == 0 ? 0f : (float)XAmplitudes.Max(),
                        YAmplitudes.Count == 0 ? 0f : (float)YAmplitudes.Max())
                };

                Count = Count + 1;
                LastCenter = next;

                return this;
            }
        }

        public struct Indicators
        {
            //L
            public double Length;
            //Acp
            public PointF AvgAmplitude;
            //Amax
            public PointF MaxAmplitude;
            //f
            public PointF Frequency;
            //t
            public PointF Period;
        }

        public readonly PointF GeometricalCenter;

        public PlatformDiagnostic(SizeF size, List<Sensor> sensors, PointF correction)
        {
            _sensors = sensors;
            _correction = correction;
            Size = size;
            GeometricalCenter = new PointF(Size.Width / 2, Size.Height / 2);
        }

        public SizeF Size { get; private set; }

        public PointF CalculateCenterOf(List<float> weights)
        {
            var totalWeight = weights.Sum();

            if ((int) totalWeight*100 == 0)
            {
                return _correction;
            }

            var ws = _sensors.Zip(weights, (s, w) => new PointF(s.Position.X*w/totalWeight, s.Position.Y*w/totalWeight)).ToList();

            var centerX = ws.Select(p => p.X).Sum() + _correction.X;
            var centerY = ws.Select(p => p.Y).Sum() + _correction.Y;

            return new PointF(centerX, centerY);
        }

        public IObservable<PointF> Center(IObservable<List<float>> channelsObservable)
        {
            return channelsObservable.Select(channels =>
            {
                if (channels != null && channels.Count == 4)
                {
                    return CalculateCenterOf(channels);
                }

                return GeometricalCenter;
            });
        }

        public IObservable<Indicators> Analize(IObservable<PointF> centerObservable)
        {
            return centerObservable.Scan(new State(), ProcessNextPoint)
                .Select(state => state.Indicators);
        }

        private State ProcessNextPoint(State state, PointF next)
        {
            return state.ProcessNext(next);
        }

        public IObservable<double> PathLength(IObservable<PointF> centerObservable)
        {
            return
                centerObservable.Scan(new Tuple<double, PointF>(0, PointF.Empty), CalculateLength)
                    .Select(tuple => tuple.Item1);
        }

        private Tuple<double, PointF> CalculateLength(Tuple<double, PointF> acc, PointF nextPoint)
        {
            if (nextPoint == PointF.Empty)
            {
                return acc;
            }
            if (acc.Item2 == PointF.Empty)
            {
                return new Tuple<double, PointF>(0, nextPoint);
            }
            var distanse = acc.Item2.DistanceTo(nextPoint);
            return new Tuple<double, PointF>(acc.Item1 + distanse, nextPoint);
        }



        public PointF CalculateMiddleOf(PointF left, PointF right)
        {
            return new PointF((left.X + right.X) / 2, (left.Y + right.Y) / 2);
        }

        public IObservable<PointF> Middle(IObservable<PointF> left, IObservable<PointF> right)
        {
            var centers = Observable.Zip(left, right);
            return centers.Select(points => CalculateMiddleOf(points[0], points[1]));
        }

        public Indicators Process(State state, PointF nextPoint)
        {
            return new Indicators();
        }
    }
}