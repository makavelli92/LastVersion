using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelStrategy.DAL;
using LevelStrategy.BL;

namespace LevelStrategy.Model
{
    public class ApplicationItem
    {
        public string classCod;

        public string security;

        public TimeFrame timeFrame;

        public int fractalParam = 0;

        public FindPattern findPattern = null;

        public ApplicationItem(ClassCod classCod, Futures security, TimeFrame timeFrame, int fractalPeriod = 0, FindPattern pattern = null)
        {
            this.classCod = classCod.ToString();

            this.security = security.ToString();

            this.timeFrame = timeFrame;

            this.fractalParam = fractalPeriod;

            this.findPattern = pattern;
        }

        public ApplicationItem(ClassCod classCod, Security security, TimeFrame timeFrame, int fractalPeriod = 0)
        {
            this.classCod = classCod.ToString();

            this.security = security.ToString();

            this.timeFrame = timeFrame;

            this.fractalParam = fractalPeriod;
        }
    }
}
