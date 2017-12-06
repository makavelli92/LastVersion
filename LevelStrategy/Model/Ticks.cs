using System;
using System.Collections.Generic;

namespace LevelStrategy.Model
{
    public class Ticks : Data
    {
        public Ticks()
        {
            timeToAction = new List<DateTime>();
        }
        public override string Name { get; set; }

        public override int TimeFrame { get; set; }

        public override string ClassCod { get; set; }

        public override List<DateTime> Time { get; set; }

        public override List<double> Close { get; set; }

        public override List<double> Volume { get; set; }

        public override int Count { get; set; }

        public override string ProcessType { get; set; } = "Accept";

        public List<DateTime> timeToAction { get; set; }

        public int SecondsCycle { get; set; } = 20;

        public void CalculateListMinuts()
        {
            if (this.timeToAction.Count == 0)
            {

                if (ClassCod == "TQBR")
                {
                    DateTime time = DateTime.Now.Date.AddHours(10);
                    DateTime fine = DateTime.Now.Date.AddHours(18).AddMinutes(45);

                    while (time.AddSeconds(SecondsCycle) <= fine)
                    {
                        timeToAction.Add(time.AddSeconds(SecondsCycle));

                        time = time.AddSeconds(SecondsCycle);
                    }
                }
                else if (ClassCod == "SPBFUT")
                {
                    DateTime time = DateTime.Now.Date.AddHours(10);
                    DateTime fine = DateTime.Now.Date.AddHours(18).AddMinutes(45);

                    while (time.AddSeconds(SecondsCycle) <= fine)
                    {
                        timeToAction.Add(time.AddSeconds(SecondsCycle));

                        time = time.AddSeconds(SecondsCycle);
                    }

                    time = DateTime.Now.Date.AddHours(19);
                    fine = DateTime.Now.Date.AddHours(23).AddMinutes(50);

                    while (time.AddSeconds(SecondsCycle) <= fine)
                    {
                        timeToAction.Add(time.AddSeconds(SecondsCycle));

                        time = time.AddSeconds(SecondsCycle);
                    }
                }
            }
            timeToAction.RemoveAll(x => x < DateTime.Now);
        }
    }
}