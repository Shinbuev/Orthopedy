using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;

namespace Stabilograph.Core.IO
{
    public class IndicatorReaderWorker : IDisposable
    {
        private readonly BlockingCollection<float[]> _output;
        private readonly IProtocol _protocol;
        private readonly TimeSpan _timeout;
        private readonly Timer _timer;

        public IndicatorReaderWorker(IProtocol protocol, BlockingCollection<float[]> output)
            : this(protocol, output, TimeSpan.FromMilliseconds(100))
        {
        }

        public IndicatorReaderWorker(IProtocol protocol, BlockingCollection<float[]> output, TimeSpan timeout)
        {
            _protocol = protocol;
            _output = output;
            _timeout = timeout;
            _timer = new Timer {AutoReset = true, Enabled = false, Interval = timeout.TotalMilliseconds};
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Dispose()
        {
            _timer.Enabled = false;
            _timer.Dispose();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var indicators = _protocol.ReadWeights();
            if (_output.TryAdd(indicators, _timeout))
                Debug.WriteLine("Buffer is full: {0}", _output.BoundedCapacity);
        }

        public void Start()
        {
            _timer.Enabled = true;
        }
    }
}