using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Segment : IComparable
    {
        public const double length = 2.0; //In s
        public double Rate { get; } //In MB
        public ulong Index { get; }
        public double SizeToDownload { get; set; }

        public Segment(double rate, ulong index)
        {
            Rate            = rate;
            Index           = index;
            SizeToDownload  = rate;
        }

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is Segment otherSegment)
                return Index.CompareTo(otherSegment.Index);
            else
                throw new ArgumentException("Object is not a Segment");
        }
        #endregion
    }
}
