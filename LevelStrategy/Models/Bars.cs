using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    //public enum TimeFrame
    //{
    //    INTERVAL_TICK = 0,      //   Тиковые данные
    //    INTERVAL_M1 = 1,        //  1 минута
    //    INTERVAL_M2 = 2,        //  2 минуты
    //    INTERVAL_M3 = 3,        //  3 минуты
    //    INTERVAL_M4 = 4,        //  4 минуты
    //    INTERVAL_M5 = 5,        //  5 минут
    //    INTERVAL_M6 = 6,        //  6 минут
    //    INTERVAL_M10 = 10,      //  10 минут
    //    INTERVAL_M15 = 15,      //  15 минут
    //    INTERVAL_M20 = 20,      //  20 минут
    //    INTERVAL_M30 = 30,      //   30 минут
    //    INTERVAL_H1 = 60,       //   1 час
    //    INTERVAL_H2 = 120,      //   2 часа
    //    INTERVAL_H4 = 240,      // 4 часа
    //    INTERVAL_D1 = 1440,     // 1 день
    //    INTERVAL_W1 = 10080,    //  1 неделя
    //    INTERVAL_MN1 = 23200,   //   1 месяц
    //}

    public enum Class
    {
        SPBFUT = 1,
        TQBR = 2
    }

    public class Bars : Data
    {
        public override string Name { get; set; }

        public override int TimeFrame { get; set; }

        public override string ClassCod { get; set; }

        public List<double> Open { get; set; }

        public override List<double> Close { get; set; }

        public List<double> High { get; set; }

        public List<double> Low { get; set; }

        public override List<double> Volume { get; set; }

        public override List<DateTime> Time { get; set; }

        public override int Count{ get{ return Close.Count(); }set { } }

        public List<double> MovingAverage { get; set; }

        public int periodMoving = 4;

        public string movingType = "Close";

        public string movingCalculateType = "EMA";

        public int periodStrategy = 200;

        public double fractalPeriod = 6;

        public int sdvig = 3;

        public List<int> indexFractalHigh { get; set; }

        public List<int> indexFractalsLow { get; set; }

        public int temp = 100;

        public DateTime LastTime
        {
            get
            {
                return Time[Count - 1];
            }
        }
        public DateTime From
        {
            get
            {
                return new DateTime(2017, 11, 17, 10, 00, 00);
            }
        }

        public double StepPrice { get; set; }

        public int StepCount { get; set; } = 0;

        public int SecondsCycle { get; set; } = 120;

       // public override bool FlagAccess { get; set; } = false;

        public string ProcessType { get; set; } = "Accept";
    }
}
