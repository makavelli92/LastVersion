using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Data
    {
        public virtual string Name { get; set; }

        public virtual string ClassCod { get; set; }

        public virtual int TimeFrame { get; set; }

        public virtual List<DateTime> Time { get; set; }

        public virtual List<double> Close { get; set; }

        public virtual List<double> Volume { get; set; }

        public virtual int Count { get; set; }

    //    public virtual bool FlagAccess { get; set; } = false;
    }
}
