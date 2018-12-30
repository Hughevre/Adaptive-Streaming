using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Buffer
    {
        public double SizeUpperBound { get; }
        public double Size { get; set; }
        public double SizeLowerBound { get; }

        private readonly List<Segment> cachedSegments;

        public ulong GetLastSegmentIndex() => cachedSegments.Count > 0 ? cachedSegments.Last().Index : 0;

        public Buffer(double sizeUpperBound = 30.0, double sizeLowerBound = 10.0)
        {
            SizeUpperBound          = sizeUpperBound;
            Size                    = 0.0;
            SizeLowerBound          = sizeLowerBound;

            cachedSegments          = new List<Segment>();
        }

        public void AddSegment(Segment segment)
        {
            cachedSegments.Add(segment);
            cachedSegments.Sort();
        }

        public Segment GetFirstSegment() => cachedSegments.FirstOrDefault();
    }
}
