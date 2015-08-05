using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Stabilograph.Core.Filters
{
    public static class UnwildZipFilter
    {
        public static IObservable<List<float>> Filter(IObservable<List<float>> src, Func<IObservable<float>, IObservable<float>> filter)
        {
            var channels = new List<IObservable<float>>(8);
            for (var index = 0; index < 8; index++)
            {
                var chIndex = index;
                var channel = filter(src.Select(values => values[chIndex]));
                channels.Add(channel);
            }

            return channels.Zip(ilist => ilist.ToList());
        } 
    }
}