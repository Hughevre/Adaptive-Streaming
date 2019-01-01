using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace AdaptiveStreaming
{
    class Player
    {
        public Buffer GenericBuffer { get; } = new Buffer();
        private double lastRenderTime;
        private double timeToRemoveFirstSegment;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void RenderBuffer(double currentClock)
        {
            double nextBufferSize = GenericBuffer.Occupancy - (currentClock - lastRenderTime);

            if (nextBufferSize >= 0)
            {
                GenericBuffer.Occupancy = nextBufferSize;
                timeToRemoveFirstSegment += (currentClock - lastRenderTime);
            }
            else
            {
                GenericBuffer.Occupancy = 0.0;
                logger.Info("Buffering at: {0}", currentClock);
            }

            if(timeToRemoveFirstSegment >= Segment.length)
            {
                GenericBuffer.RemoveFirstChunk();
                timeToRemoveFirstSegment = 0.0;
            }

            lastRenderTime = currentClock;
        }
    }
}
