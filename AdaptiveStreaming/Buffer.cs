using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Buffer
    {
        public const double sizeUpperBound  = 30.0;
        public const double sizeLowerBound  = 0.0;
        public const double reservoir       = 11.0;
        public const double cushion         = 16.0;

        public double Occupancy { get; set; }

        private readonly List<Segment> bufferedChunks = new List<Segment>();

        public void AddChunk(Segment chunk)
        {
            bufferedChunks.Add(chunk);
            bufferedChunks.Sort();

            Occupancy += Segment.length;
        }

        public void RemoveFirstChunk()
        {
            var itemToRemove = bufferedChunks.First();
            if (itemToRemove != null)
                bufferedChunks.Remove(itemToRemove);
        }
    }
}
