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
        private const double simulationLength       = 150.0;
        private double downloadStartTime;

        private readonly Player genericPlayer       = new Player();

        private double bitRate                      = 5.0;

        private readonly List<Event> eventsQueue    = new List<Event>();

        private Segment downloadingSegment;
        private bool isDownloading;

        private const double lambda                 = 0.25;
        private const double minimumBitRate         = 0.0;
        private const double maximumBitRate         = 10.0;

        private readonly Random rnd                 = new Random();

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public ICommand ReadCommand { get; set; }

        public Simulation()
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
                },

                new LineSeries
                {
                    Title  = "Segment quality",
                    Values = new ChartValues<ObservablePoint>(),
                    Stroke = System.Windows.Media.Brushes.Indigo
                }
            };

            YFormatter = value => value.ToString("");

            ReadCommand = new RelayCommand<object>(_ => Simulate());
        }

        public void Simulate()
        {
            Task.Run(() =>
            {
                Event currentEvent = null;
                //Make init download
                downloadingSegment = new Segment(VideoRate.FindMinRateWhere(_ => true), 0);
                eventsQueue.Add(new Event(simulationClock, BeginGettingSegment));
                eventsQueue.Add(new Event(simulationClock + GenerateExpDistribiution(), ChangeBitRate));
                while (simulationClock < simulationLength)
                {
                    currentEvent = eventsQueue.First();
                    eventsQueue.Remove(eventsQueue.First());

                    simulationClock = currentEvent.Time;

                    currentEvent.Handler();
                    eventsQueue.Sort(); //Sort by time

                    genericPlayer.RenderBuffer(simulationClock);

                    PopulateChartData();
                }
            });
        }

        private double DecodeRateMap(double bufferOccupancy)
        {
            if (bufferOccupancy <= Buffer.reservoir)
            {
                return minimumBitRate;
            }
            else if (bufferOccupancy > Buffer.reservoir && bufferOccupancy <= Buffer.cushion + Buffer.reservoir)
            {
                const double coefficient = (maximumBitRate - minimumBitRate) / (Buffer.cushion);
                return (coefficient * (bufferOccupancy - Buffer.reservoir)) + minimumBitRate;
            }
            else
            {
                return maximumBitRate;
            }
        }

        private double GetNextSegmentRate()
        {
            //Adaptive bit rate selection algorithm: BBA-0
            double rateMinus, ratePlus;
            if (downloadingSegment.Rate == VideoRate.FindMaxRateWhere(_ => true))
                ratePlus = VideoRate.FindMaxRateWhere(_ => true);
            else
                ratePlus = VideoRate.FindMinRateWhere(x => x > downloadingSegment.Rate);

            if (downloadingSegment.Rate == VideoRate.FindMinRateWhere(_ => true))
                rateMinus = VideoRate.FindMinRateWhere(_ => true);
            else
                rateMinus = VideoRate.FindMaxRateWhere(x => x < downloadingSegment.Rate);

            if (genericPlayer.GenericBuffer.Occupancy <= Buffer.reservoir)
                return VideoRate.FindMinRateWhere(_ => true);
            else if (genericPlayer.GenericBuffer.Occupancy >= (Buffer.reservoir + Buffer.cushion))
                return VideoRate.FindMaxRateWhere(_ => true);
            else if (DecodeRateMap(genericPlayer.GenericBuffer.Occupancy) >= ratePlus)
                return VideoRate.FindMaxRateWhere(x => x < DecodeRateMap(genericPlayer.GenericBuffer.Occupancy));
            else if (DecodeRateMap(genericPlayer.GenericBuffer.Occupancy) <= rateMinus)
                return VideoRate.FindMinRateWhere(x => x > DecodeRateMap(genericPlayer.GenericBuffer.Occupancy));
            else
                return downloadingSegment.Rate;
        }

        private void BeginGettingSegment()
        {
            if (! isDownloading)
                downloadingSegment = new Segment(GetNextSegmentRate(), downloadingSegment.Index + 1);

            double nextTime = (bitRate > 0) ?
                simulationClock + (downloadingSegment.SizeToDownload / bitRate) :
                Double.PositiveInfinity;
            eventsQueue.Add(new Event(nextTime, EndGettingSegment));

            isDownloading = true;

            downloadStartTime = simulationClock;
        }

        private void EndGettingSegment()
        {
            genericPlayer.GenericBuffer.AddChunk(downloadingSegment);

            double delay = (genericPlayer.GenericBuffer.Occupancy > Buffer.sizeUpperBound) ?
                genericPlayer.GenericBuffer.Occupancy - Buffer.sizeUpperBound :
                0.0;

            double nextTime = simulationClock + delay;
            eventsQueue.Add(new Event(nextTime, BeginGettingSegment));

            isDownloading = false;
        }

        private void ChangeBitRate()
        {
            if (isDownloading)
            {
                downloadingSegment.SizeToDownload -= (bitRate * (simulationClock - downloadStartTime));

                eventsQueue.RemoveAll(bash => bash.Handler == EndGettingSegment);
                eventsQueue.Add(new Event(simulationClock, BeginGettingSegment));
            }
            bitRate = (rnd.NextDouble() * (maximumBitRate - minimumBitRate)) + minimumBitRate;

            eventsQueue.Add(new Event(simulationClock + GenerateExpDistribiution(), ChangeBitRate));
        }

        private double GenerateExpDistribiution() => (-1 / lambda) * Math.Log(rnd.NextDouble());

        private void PopulateChartData()
        {
            SeriesCollection[0].Values.Add(new ObservablePoint(simulationClock, bitRate));
            SeriesCollection[1].Values.Add(new ObservablePoint(simulationClock, genericPlayer.GenericBuffer.Occupancy));
            SeriesCollection[2].Values.Add(new ObservablePoint(simulationClock, downloadingSegment.Rate));
        }
    }
}
