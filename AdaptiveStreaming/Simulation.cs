using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace AdaptiveStreaming
{
    class Simulation
    {
        private double simulationClock;
        private readonly double simulationLength;
        private double previousSegmentTimeStamp;

        private readonly Player genericPlayer;

        private double bitRate;

        private readonly List<Event> eventsQueue;

        private Segment downloadingSegment;

        private readonly double lambda;

        private readonly double minimumBitRate;
        private readonly double maximumBitRate;

        private readonly Random rnd;

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ICommand ReadCommand { get; set; }

        public Simulation(double _simulationLength)
        {
            simulationClock          = 0.0;
            simulationLength         = _simulationLength;
            previousSegmentTimeStamp = 0.0;

            genericPlayer            = new Player();

            bitRate                  = 10.0;

            eventsQueue              = new List<Event>();

            lambda                   = 0.25;

            minimumBitRate           = 10.0;
            maximumBitRate           = 0.5;

            rnd                      = new Random();
        }

        public Simulation() : this(300.0)
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title  = "Band width",
                    Values = new ChartValues<ObservablePoint>(),
                    Stroke = System.Windows.Media.Brushes.Khaki
                },

                new LineSeries
                {
                    Title  = "Buffer length",
                    Values = new ChartValues<ObservablePoint>(),
                    Stroke = System.Windows.Media.Brushes.Lime
                }
            };

            YFormatter = value => value.ToString("");

            ReadCommand = new RelayCommand<object>(Simulate);
        }

        public void Simulate(object arg)
        {
            Task.Run(() =>
            {
                Event currentEvent = null;
                eventsQueue.Add(new Event(simulationClock, BeginGettingSegment));
                eventsQueue.Add(new Event(simulationClock + GenerateExpDistribiution(), ChangeBitRate));
                while (simulationClock < simulationLength)
                {
                    currentEvent = eventsQueue.First();
                    eventsQueue.Remove(eventsQueue.First());

                    simulationClock = currentEvent.Time;

                    currentEvent.Handler();
                    eventsQueue.Sort(); //Sort by time
                }
            });
        }

        private double ComputeSegmentQA()
        {
            double bufferWNewSegment = genericPlayer.GenericBuffer.Size + Segment.Length;
            if (bitRate > 0 && bufferWNewSegment - (Segment.Quality.FHD / bitRate) >= genericPlayer.GenericBuffer.SizeLowerBound)
                return Segment.Quality.FHD;
            else if (bitRate > 0 && bufferWNewSegment - (Segment.Quality.HD / bitRate) >= genericPlayer.GenericBuffer.SizeLowerBound)
                return Segment.Quality.HD;
            else
                return Segment.Quality.SD;
        }

        private void BeginGettingSegment()
        {
            downloadingSegment = new Segment(ComputeSegmentQA(), genericPlayer.GenericBuffer.GetLastSegmentIndex() + 1);
            double nextTime = (bitRate > 0) ?
                simulationClock + (downloadingSegment.Size / bitRate) :
                Double.PositiveInfinity;
            eventsQueue.Add(new Event(nextTime, EndGettingSegment));

            genericPlayer.RenderBackBuffer(simulationClock - previousSegmentTimeStamp);

            SeriesCollection[1].Values.Add(new ObservablePoint(simulationClock, genericPlayer.GenericBuffer.Size));

            previousSegmentTimeStamp = simulationClock;
        }

        private void EndGettingSegment()
        {
            genericPlayer.GenericBuffer.AddSegment(downloadingSegment);

            double delay = 0.0;
            if (genericPlayer.GenericBuffer.Size > genericPlayer.GenericBuffer.SizeUpperBound)
                delay = genericPlayer.GenericBuffer.Size - genericPlayer.GenericBuffer.SizeUpperBound;

            double nextTime = simulationClock + delay;
            eventsQueue.Add(new Event(nextTime, BeginGettingSegment));
        }

        private void ChangeBitRate()
        {


            bitRate = rnd.NextDouble() * (maximumBitRate - minimumBitRate) + minimumBitRate;
            eventsQueue.Add(new Event(simulationClock + GenerateExpDistribiution(), ChangeBitRate));

            SeriesCollection[0].Values.Add(new ObservablePoint(simulationClock, bitRate));
        }

        private double GenerateExpDistribiution() => (-1 / lambda) * Math.Log(rnd.NextDouble());
    }
}
