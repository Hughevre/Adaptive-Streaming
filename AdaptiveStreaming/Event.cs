using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveStreaming
{
    class Event : IComparable
    {
        public double Time { get; }
        public Action Handler { get; }

        public Event(double time, Action handler)
        {
            Time = time;
            Handler = handler;
        }

        #region IComparable implementation
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            Event otherEvent = obj as Event;
            if (otherEvent != null)
                return this.Time.CompareTo(otherEvent.Time);
            else
                throw new ArgumentException("Object is not a Event");
        }
        #endregion
    }
}
