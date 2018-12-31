using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    static class VideoRate
    {
        private static readonly Dictionary<string, double> rates = new Dictionary<string, double>
        {
            {"SD",  2.0 },
            {"HD",  6.0 },
            {"FHD", 10.0 }
            //{"QHD", 20.0 }
        };

        public static double FindMinRateWhere(Func<double, bool> predicate) =>
            rates.Values.Where(predicate).Min();

        public static double FindMaxRateWhere(Func<double, bool> predicate) =>
            rates.Values.Where(predicate).Max();
    }
}
