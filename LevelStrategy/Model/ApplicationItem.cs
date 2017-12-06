using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelStrategy.DAL;

namespace LevelStrategy.Model
{
    public class ApplicationItem
    {
        public string classCod;

        public string security;

        public TimeFrame timeFrame;

        public ApplicationItem(ClassCod classCod, Futures security, TimeFrame timeFrame)
        {
            this.classCod = classCod.ToString();

            this.security = security.ToString();

            this.timeFrame = timeFrame;
        }

        public ApplicationItem(ClassCod classCod, Security security, TimeFrame timeFrame)
        {
            this.classCod = classCod.ToString();

            this.security = security.ToString();

            this.timeFrame = timeFrame;
        }
    }
}
