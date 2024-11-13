using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task2_taskmngr
{
    class ClassTimeMetrics
    {
        public double Value { get; set; }
        public string Time { get; set; }
        public ClassTimeMetrics() { }
        public ClassTimeMetrics(double Value, string Time)
        {
            this.Value = Value;
            this.Time = Time;
        }
    }
}
