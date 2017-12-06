using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Ticks : Data
    {
        public override string Name { get; set; }

        public override int TimeFrame { get; set; }

        public override string ClassCod { get; set; }

        public override List<DateTime> Time { get; set; }

        public override List<double> Close { get; set; }

        public override List<double> Volume { get; set; }

        public override int Count { get; set; }
    }
}
