using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.Core.Extensions
{
    public static class ObservableRangeCollectionEx
    {
        /// <summary>
        /// To be used in collection initializer
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Add<T>(this ObservableRangeCollection<T> source, IEnumerable<T> collection)
        {
            source.AddRange(collection);
        }
    }
}
