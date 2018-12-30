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
        public Buffer GenericBuffer { get; }
        private double playingTime;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Player()
        {
            GenericBuffer = new Buffer();
            playingTime = 0.0;
        }

        public void RenderBackBuffer(double currentTime, double previousTime)
        {
            double nextBufferSize = GenericBuffer.Size - (currentTime - previousTime);

            if (nextBufferSize >= 0)
                playingTime += (currentTime - previousTime);
            else
                logger.Info("Buffering at: {0}", currentTime);

            GenericBuffer.Size = nextBufferSize > 0.0 ? nextBufferSize : 0.0;
        }
    }
}
