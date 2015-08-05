using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Stabilograph.Core.IO
{
    public static class ObserwableProtocol
    {
        public static IObservable<List<float>> AsObservable(this IProtocol protocol, TimeSpan interval)
        {
            return Observable.Interval(interval)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(tick => protocol.ReadWeights().ToList());
        }
    }
}