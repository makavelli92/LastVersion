using System;
using System.Collections.Generic;

namespace LevelStrategy.Model
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
        public virtual string ProcessType { get; set; } = "Accept";
        
    }
}
