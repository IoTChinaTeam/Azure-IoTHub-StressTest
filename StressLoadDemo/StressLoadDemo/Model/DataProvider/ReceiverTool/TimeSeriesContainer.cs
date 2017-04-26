using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.DataProvider.ReceiverTool
{
    class TimeSeriesContianer
    {
        private TimeSpan window;
        private double weight;
        private double windowSum;
        private LinkedList<KeyValuePair<DateTime, double>> values = new LinkedList<KeyValuePair<DateTime, double>>();

        public int Count
        {
            get
            {
                return values.Count;
            }
        }

        public double StreamAvg { get; private set; }

        public double WindowAvg
        {
            get
            {
                int count = values.Count;
                return count == 0 ? double.NaN : windowSum / count;
            }
        }

        public TimeSeriesContianer(TimeSpan window, double weight)
        {
            this.window = window;
            this.weight = weight;

            Reset();
        }

        public void Reset()
        {
            windowSum = 0;
            values.Clear();

            StreamAvg = double.NaN;
        }

        public void Push(DateTime time, double value)
        {
            lock (values)
            {
                if (double.IsNaN(StreamAvg))
                {
                    StreamAvg = value;
                }
                else
                {
                    StreamAvg = (StreamAvg * weight + value) / (weight + 1);
                }

                // Add new value
                var node = values.Last;
                while (node != null)
                {
                    if (node.Value.Key < time)
                    {
                        values.AddAfter(node, new KeyValuePair<DateTime, double>(time, value));
                        break;
                    }

                    node = node.Previous;
                }

                if (node == null)
                {
                    values.AddFirst(new KeyValuePair<DateTime, double>(time, value));
                }

                windowSum += value;

                // Remove old values
                var start = values.Last().Key - window;

                while (values.Any())
                {
                    var pair = values.First();

                    if (pair.Key >= start)
                    {
                        return;
                    }
                    else
                    {
                        windowSum -= pair.Value;
                        values.RemoveFirst();
                    }
                }
            }
        }
    }
}
