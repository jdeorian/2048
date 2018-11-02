using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Utilities
{
    public static class ListExtensions
    {
        public static void AddMany<T>(this List<T> list, params T[] items) => list.AddRange(items);
    }
}
