using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Player
    {
        public Buffer GenericBuffer { get; }

        public Segment RenderingSegment { get; private set; }

        public Player() => GenericBuffer = new Buffer();

        public void RenderBackBuffer(double elapsedTime)
        {
            //RenderingSegment = GenericBuffer.GetFirstSegment();
            double nextValue = GenericBuffer.Size - elapsedTime + Segment.Length;
            GenericBuffer.Size = nextValue > 0.0 ? nextValue : 0.0;
        }
    }
}
