using System;

namespace LevelStrategy.Model
{
   public class Candle
    {
        public DateTime time;

        public double open;
        public double high;
        public double close;
        public double low;
        public double volume;



        public void SetCandleFromString(string In) // загрузить данные из файла
        {//20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000,1
            //<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOLUME>
            string[] lockal = In.Split(',');

            time = new DateTime(Convert.ToInt32(lockal[0][0].ToString() + lockal[0][1].ToString() + lockal[0][2].ToString() + lockal[0][3].ToString()),
                 Convert.ToInt32(lockal[0][4].ToString() + lockal[0][5].ToString()),
                 Convert.ToInt32(lockal[0][6].ToString() + lockal[0][7].ToString()),
                 Convert.ToInt32(lockal[1][0].ToString() + lockal[1][1].ToString()),
                 Convert.ToInt32(lockal[1][2].ToString() + lockal[1][3].ToString()),
                 Convert.ToInt32(lockal[1][4].ToString() + lockal[1][5].ToString()));

            string[] sIn = In.Split(',');

            string[] shit = sIn[2].Split('.');
            if (shit.Length == 2)
            {
                open = Convert.ToDouble(shit[0] + ',' + shit[1]);
            }
            else
            {
                open = Convert.ToDouble(shit[0]);
            }

            shit = sIn[3].Split('.');
            if (shit.Length == 2)
            {
                high = Convert.ToDouble(shit[0] + ',' + shit[1]);
            }
            else
            {
                high = Convert.ToDouble(shit[0]);
            }

            shit = sIn[4].Split('.');
            if (shit.Length == 2)
            {
                low = Convert.ToDouble(shit[0] + ',' + shit[1]);
            }
            else
            {
                low = Convert.ToDouble(shit[0]);
            }

            shit = sIn[5].Split('.');
            if (shit.Length == 2)
            {
                close = Convert.ToDouble(shit[0] + ',' + shit[1]);
            }
            else
            {
                close = Convert.ToDouble(shit[0]);
            }

            shit = sIn[6].Split('.');

            if (shit.Length == 2)
            {
                volume = Convert.ToDouble(shit[0] + ',' + shit[1]);
            }
            else
            {
                volume = Convert.ToDouble(shit[0]);
            }

        }

        public string GetString() // взять строку эквивалент сохранённой в файле
        { //20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000
            string result = string.Empty;

            result += time.Year.ToString();
            if (time.Month > 9)
            {
                result += time.Month.ToString();
            }
            else
            {
                result += "0" + time.Month.ToString();
            }
            if (time.Day > 9)
            {
                result += time.Day.ToString() + ",";
            }
            else
            {
                result += "0" + time.Day.ToString() + ",";
            }

            if (time.Hour > 9)
            {
                result += time.Hour.ToString();
            }
            else
            {
                result += "0" + time.Hour.ToString();
            }
            if (time.Minute > 9)
            {
                result += time.Minute.ToString();
            }
            else
            {
                result += "0" + time.Minute.ToString();
            }
            if (time.Second > 9)
            {
                result += time.Second.ToString() + ",";
            }
            else
            {
                result += "0" + time.Second.ToString() + ",";
            }



            result += open.ToString() + ",";
            result += high.ToString() + ",";
            result += low.ToString() + ",";
            result += close.ToString() + ",";
            return result;
        }

        public string GetBeautifulString() // взять строку с подписями
        {//Date - 20131001 Time - 100000 Open - 97.8000000 High - 97.9900000 Low - 97.7500000 Close - 97.9000000

            string result = string.Empty;
            result += "Date: ";
            result += time.Year.ToString();
            if (time.Month > 9)
            {
                result += time.Month.ToString();
            }
            else
            {
                result += "0" + time.Month.ToString();
            }

            if (time.Day > 9)
            {
                result += time.Day.ToString();
            }
            else
            {
                result += "0" + time.Day.ToString();
            }

            result += " Time: ";

            if (time.Hour > 9)
            {
                result += time.Hour.ToString();
            }
            else
            {
                result += "0" + time.Hour.ToString();
            }
            if (time.Minute > 9)
            {
                result += time.Minute.ToString();
            }
            else
            {
                result += "0" + time.Minute.ToString();
            }
            if (time.Second > 9)
            {
                result += time.Second.ToString();
            }
            else
            {
                result += "0" + time.Second.ToString();
            }


            result += " Open: ";
            result += open.ToString();
            result += " High: ";
            result += high.ToString();
            result += " Low: ";
            result += low.ToString();
            result += " Close: ";
            result += close.ToString();

            return result;
        }
    }
}
