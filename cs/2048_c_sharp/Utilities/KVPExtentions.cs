using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Utilities
{
    public static class KVPExtentions
    {
        public static void Deconstruct<TKey, TValue>(
                    this KeyValuePair<TKey, TValue> entry,
                    out TKey key,
                    out TValue value)
        {
            key = entry.Key;
            value = entry.Value;
        }
    }
}
