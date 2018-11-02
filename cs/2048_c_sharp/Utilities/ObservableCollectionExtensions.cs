using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Utilities
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> ts, IEnumerable<T> range)
        {
            if (range == null || !range.Any()) return;
            foreach (var t in range) ts.Add(t);
        }
    }
}
