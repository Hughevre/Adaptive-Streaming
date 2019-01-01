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
            {"IIS-1", 0.35 },
            {"IIS-2", 0.47 },
            {"IIS-3", 0.63 },
            {"IIS-4", 0.85 },
            {"IIS-5", 1.13 },
            {"IIS-6", 1.52 },
            {"IIS-7", 2.04 },
            {"IIS-8", 2.75 }
        };

        public static double FindMinRateWhere(Func<double, bool> predicate) =>
            rates.Values.Where(predicate).Min();

        public static double FindMaxRateWhere(Func<double, bool> predicate) =>
            rates.Values.Where(predicate).Max();
    }
}
