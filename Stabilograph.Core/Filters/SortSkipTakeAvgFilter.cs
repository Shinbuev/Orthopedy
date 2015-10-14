using System;
using System.Collections.Generic;
using System.Linq;

namespace Stabilograph.Core.Filters
{
    public static class SortSkipTakeAvgFilter
    {
//        public static IObservable<float> Filter(IObservable<float> src, int bufferSize, int dropSize, int takeSize)
//        {
//            return src.Buffer(bufferSize).Select((buffered, index) => ApplyFilter(buffered, index, dropSize, takeSize));
//        }

//        public static float ApplyFilter(IList<float> buffered, int indexOfSource, int dropSize, int takeSize)
//        {
//            var list = buffered.ToList();
//            list.Sort();
//
//            return list.Skip(dropSize).Take(takeSize).Average();
//        }
//
//        public static IObservable<List<float>> Filter(IObservable<List<float>> src, 
//            int bufferSize, int dropSize, int takeSize)
//        {
//            return UnwildZipFilter.Filter(src, observable => Filter(observable, bufferSize, dropSize, takeSize));
//        } 
    }
}
