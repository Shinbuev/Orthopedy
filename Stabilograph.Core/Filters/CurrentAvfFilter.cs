using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Stabilograph.Core.Filters
{
    public static class CurrentAvfFilter
    {
        public static IObservable<float> Filter(IObservable<float> src, int bufferSize)
        {
            return src.Scan(new Queue<float>(),
                (queue, f) =>
                {
                    queue.Enqueue(f);
                    if (queue.Count > bufferSize)
                        queue.Dequeue();
                    return queue;
                }).Select(q => q.Average());
        }

        public static IObservable<List<float>> Filter(IObservable<List<float>> src, int bufferSize)
        {
            return UnwildZipFilter.Filter(src, s => Filter(s, bufferSize));
        } 
    }
}
