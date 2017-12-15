using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelStrategy.Model
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
        public void CalculateListMinuts()
        {
            if (this.timeToAction.Count == 0)
            {
                if (ClassCod == "TQBR")
                {
                    if (TimeFrame < 60)
                    {
                        DateTime time = DateTime.Now.Date.AddHours(10);
                        DateTime fine = DateTime.Now.Date.AddHours(18).AddMinutes(45);

                        while (time <= fine)
                        {
                            if (time > DateTime.Now.Date.AddHours(10))
                                timeToAction.Add(time.AddSeconds(-SecondsCycle));

                            time = time.AddMinutes(TimeFrame);

                            if (time > DateTime.Now.Date.AddHours(18).AddMinutes(45) && !timeToAction.Contains(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle)))
                                timeToAction.Add(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle));
                        }
                    }
                    else if (TimeFrame <= 240)
                    {
                        DateTime time = DateTime.Now.Date.AddHours(10);
                        DateTime fine = DateTime.Now.Date.AddHours(18).AddMinutes(45);

                        while (time <= fine)
                        {
                            time = time.AddMinutes(TimeFrame);

                            if (time >= DateTime.Now.Date.AddHours(18).AddMinutes(45) && !timeToAction.Contains(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle)))
                            {
                                timeToAction.Add(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle));
                            }
                            else
                                timeToAction.Add(time.AddSeconds(-SecondsCycle));
                        }
                    }
                }
                else if (ClassCod == "SPBFUT")
                {
                    if (TimeFrame < 60)
                    {
                        DateTime time = DateTime.Now.Date.AddHours(10);
                        DateTime fine = DateTime.Now.Date.AddHours(23).AddMinutes(50);

                        while (time <= fine)
                        {
                            time = time.AddMinutes(TimeFrame);

                            if (time >= DateTime.Now.Date.AddHours(18).AddMinutes(45) && !timeToAction.Contains(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle)))
                            {
                                timeToAction.Add(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle));
                            }
                            else if (time < DateTime.Now.Date.AddHours(18).AddMinutes(45))
                                timeToAction.Add(time.AddSeconds(-SecondsCycle));
                        }

                        time = DateTime.Now.Date.AddHours(19);
                        fine = DateTime.Now.Date.AddHours(23).AddMinutes(50);

                        while (time <= fine)
                        {
                            time = time.AddMinutes(TimeFrame);

                            if (time >= DateTime.Now.Date.AddHours(23).AddMinutes(50) && !timeToAction.Contains(DateTime.Now.Date.AddHours(23).AddMinutes(50).AddSeconds(-SecondsCycle)))
                                timeToAction.Add(DateTime.Now.Date.AddHours(23).AddMinutes(50).AddSeconds(-SecondsCycle));
                            else if (time < DateTime.Now.Date.AddHours(23).AddMinutes(50))
                                timeToAction.Add(time.AddSeconds(-SecondsCycle));
                        }
                    }
                    else if (TimeFrame <= 240)
                    {
                        DateTime time = DateTime.Now.Date.AddHours(10);
                        DateTime fine = DateTime.Now.Date.AddHours(23).AddMinutes(50);

                        while (time <= fine)
                        {
                            time = time.AddMinutes(TimeFrame);

                            if (time >= DateTime.Now.Date.AddHours(18).AddMinutes(45) && time <= DateTime.Now.Date.AddHours(19))
                            {
                                timeToAction.Add(DateTime.Now.Date.AddHours(18).AddMinutes(45).AddSeconds(-SecondsCycle));
                            }
                            else if (time >= DateTime.Now.Date.AddHours(23).AddMinutes(50) && !timeToAction.Contains(DateTime.Now.Date.AddHours(23).AddMinutes(50).AddSeconds(-SecondsCycle)))
                                timeToAction.Add(DateTime.Now.Date.AddHours(23).AddMinutes(50).AddSeconds(-SecondsCycle));
                            else
                                timeToAction.Add(time.AddSeconds(-SecondsCycle));
                        }
                    }
                }
            }
            timeToAction.RemoveAll(x => x < DateTime.Now);
        }
        public Bars()
        {
            timeToAction = new List<DateTime>();
        }
        public override string Name { get; set; }

        public override int TimeFrame { get; set; }

        public override string ClassCod { get; set; }

        public List<double> Open { get; set; }

        public override List<double> Close { get; set; }

        public List<double> High { get; set; }

        public List<double> Low { get; set; }

        public override List<double> Volume { get; set; }

        public override List<DateTime> Time { get; set; }

        public override DateTime LastGet { get; set; }

        public override int Count { get { return Close.Count; } set { } }

        public List<double> MovingAverage { get; set; }

        public int periodMoving = 4;

        public string movingType = "Close";

        public string movingCalculateType = "EMA";

        public int periodStrategy = 200;

        public double fractalPeriod = 21;

        public int sdvig = 3;

        public List<int> indexFractalHigh { get; set; }

        public List<int> indexFractalsLow { get; set; }

        public int temp = 0;

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
                return new DateTime(2017, 12, 01, 10, 00, 00);
            }
        }

        public double StepPrice { get; set; }

        public int StepCount { get; set; } = 1;

        public int SecondsCycle { get; set; } = 120;

        public override string ProcessType { get; set; } = "Accept";
        
        public List<SignalData> listSignal { get; set; }

        public List<DateTime> timeToAction { get; set; }

        public int CountSigns { get; set; }

        public string Account
        {
            get
            {
                return ClassCod == "SPBFUT" ? "41104ES" : "L01-00000F00";
            }
        }

        public override List<Order> Orders { get; set; }

        public override bool CheckOrder => Orders.Count > 0 && Orders.Any(x => /*x.transactionId > 0 && x.StopProfitId > 0 &&*/ DateTime.Now - LastGet > new TimeSpan(0, 1, 0));

        public DateTime LastCheckTable { get; set; }

        public bool CheckTableTime => DateTime.Now - LastCheckTable > new TimeSpan(0, 2, 0);

        public bool SendCheckTable = false;

        public  string first = "first";

        public  bool history = false;
    }
}
