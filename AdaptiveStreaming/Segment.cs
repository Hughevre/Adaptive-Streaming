using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Segment : IComparable
    {
        public static double Length { get; } = 2.0;//In s
        public double Size { get; } //In MB/s
        public ulong Index { get; }

        public Segment(double size, ulong index)
        {
            Size = size;
            Index = index;
        }

        public struct Quality
        {
            public const double SD  = 2.0;
            public const double HD  = 4.0;
            public const double FHD = 10.0;
            public const double QHD = 35.0; //For future development
        }

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is Segment otherSegment)
                return this.Index.CompareTo(otherSegment.Index);
            else
                throw new ArgumentException("Object is not a Event");
        }
        #endregion
    }
}
