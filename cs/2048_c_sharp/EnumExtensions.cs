using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp
{
    public static class XT
    {
        public static IEnumerable<T> EnumVals<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();

        public static T GetRandom<T>(this T[] array, Random rnd = null)
        {
            var len = array.Count();
            if (len == 0) return default;
            if (len == 1) return array[0];

            if (rnd == null) rnd = new Random();
            return array[rnd.Next(len)];
        }
    }
}
