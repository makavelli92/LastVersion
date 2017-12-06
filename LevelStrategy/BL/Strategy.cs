using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelStrategy.Model;
using NLog;

namespace LevelStrategy.BL
{
    public class Strategy
    {
        private static readonly ILogger Logger = LogManager.GetLogger("info");
        public static void FindSignal(Bars bars, EventHandler<SignalData> eventHandler)
        {
            if (bars.LastTime == new DateTime(2017, 11, 17, 23, 25, 00))
            {
            }
            int bar = bars.Count - 1;
            int bsy;
            if (FindModelRepeatLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Short")//&& DefenitionAreaNearLevel(bar) == "Short"  && shortTrade)
            {
                bars.listSignal.Add(new SignalData("Short - Повторяющийся уровень", new char[] {'h','h','h'}, bars.Time[bsy], bars.Time[(bar - 1)], bars.Time[bar], bars.High[bsy], Math.Round(bars.High[bsy] * 0.9996, bars.CountSigns), Math.Round(((bars.High[bsy] * 0.9996) * 0.996), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                eventHandler(new object(), bars.listSignal.Last());
            }
            if (FindModelMirrorLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Short")// && DefenitionAreaNearLevel(bar) == "Short"  && shortTrade)
            {
                bars.listSignal.Add(new SignalData("Short - Зеркальный уровень", new char[] { 'l', 'h', 'h' }, bars.Time[bsy], bars.Time[(bar - 1)], bars.Time[bar], bars.Low[bsy], Math.Round(bars.Low[bsy] * 0.9996, bars.CountSigns), Math.Round(((bars.Low[bsy] * 0.9996) * 0.996), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                eventHandler(new object(), bars.listSignal.Last());
            }
            if (AirLevel(bar, bars, out bsy) == "Short")//&& DefenitionAreaNearLevel(bar) == "Nothing" && shortTrade)
            {
                bars.listSignal.Add(new SignalData("Short - Воздушный уровень", new char[] { 'h', 'h', 'h' }, bars.Time[bsy], bars.Time[(bar - 1)], bars.Time[bar], bars.High[bsy], Math.Round(bars.High[bsy] * 0.9996, bars.CountSigns), Math.Round(((bars.High[bsy] * 0.9996) * 0.996), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                ChangeColorConsole(true);
                eventHandler(new object(), bars.listSignal.Last());
                ChangeColorConsole(false);
            }
            if (FindModelRepeatLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Long")   // && DefenitionAreaNearLevel(bar) == "Long"   && longTrade)
            {
                bars.listSignal.Add(new SignalData("Long - Повторяющийся уровень", new char[] { 'l', 'l', 'l' }, bars.Time[bsy],  bars.Time[(bar - 1)], bars.Time[bar], bars.Low[bsy], Math.Round(bars.Low[bsy] * 1.0004, bars.CountSigns), Math.Round(((bars.Low[bsy] * 1.0004) * 1.004), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                eventHandler(new object(), bars.listSignal.Last());
            }
            if (FindModelMirrorLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Long")// && DefenitionAreaNearLevel(bar) == "Long" && longTrade)
            {
                bars.listSignal.Add(new SignalData("Long - Зеркальный уровень", new char[] { 'h', 'l', 'l' }, bars.Time[bsy],  bars.Time[(bar - 1)], bars.Time[bar], bars.High[bsy], Math.Round(bars.High[bsy] * 1.0004, bars.CountSigns), Math.Round(((bars.High[bsy] * 1.0004) * 1.004), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                eventHandler(new object(), bars.listSignal.Last());
            }
            if (AirLevel(bar, bars, out bsy) == "Long")// && DefenitionAreaNearLevel(bar) == "Nothing" && longTrade)
            {
                bars.listSignal.Add(new SignalData("Long - Воздушный уровень", new char[] { 'l', 'l', 'l' },  bars.Time[bsy],  bars.Time[(bar - 1)],  bars.Time[bar], bars.Low[bsy], Math.Round(bars.Low[bsy] * 1.0004, bars.CountSigns), Math.Round(((bars.Low[bsy] * 1.0004) * 1.004), bars.CountSigns), DateTime.Now, bars.Name + " " + bars.TimeFrame));
                ChangeColorConsole(true);
                eventHandler(new object(), bars.listSignal.Last());
                ChangeColorConsole(false);
            }
        }

        public static void ChangeColorConsole(bool change, ConsoleColor color = ConsoleColor.Red)
        {
            if (change)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ResetColor();
        }

        /// <summary>
        /// Метод для определния с какого бара искать БСУ, если в графике меньше 540 баров, то с 0, если больше, то текущий бар - 540 баров назад
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="periodStrategy"> Как глубоко будем искать БСУ</param>
        /// <returns></returns>
        public static int StartBarForFindBsy(int bar, int periodStrategy)             
        {
            if (bar - periodStrategy > 0)
                return bar - periodStrategy;
            return 0;
        }
        //(bar > 2 && Math.Abs(bars.High[bar - 2] - bars.High[bar - 3]) < TOLERANCE &&
        public static string AirLevel(int bar, Bars bars, out int bsy)
        {
            if (bar > 2 && bars.High[bar - 2] == bars.High[bar - 3] &&
                (bars.High[bar - 1] >= (bars.High[bar - 2] - (bars.High[bar - 2] / 2500)) && bars.High[bar - 1] <= bars.High[bar - 2]) &&
                (bars.High[bar] >= (bars.High[bar - 2] - (bars.High[bar - 2] / 2500)) && bars.High[bar] <= bars.High[bar - 2])
                )
            {
                bsy = bar - 2;
                return "Short";
            }
            else if (bar > 2 && bars.Low[bar - 2] == bars.Low[bar - 3] && 
                (bars.Low[bar - 1] <= (bars.Low[bar - 2] + (bars.Low[bar - 2] / 2500)) && bars.Low[bar - 1] >= bars.Low[bar - 2]) &&
                (bars.Low[bar] <= (bars.Low[bar - 2] + (bars.Low[bar - 2] / 2500)) && bars.Low[bar] >= bars.Low[bar - 2])
                )
            {
                bsy = bar - 2;
                return "Long";
            }
            bsy = 0;
            return "Nothing";
        }
        public static bool BsyAndPby1MirrorForLong(int bar, List<int> listHighFractal, Bars bars, out int bsy) // Для зеркального уровня, позиция в лонг
        {
            int fine = StartBarForFindBsy(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.High[temp] == bars.Low[bar]*/ (bars.High[temp] <= bars.Low[bar] && (bars.High[temp] + bars.StepPrice * bars.StepCount) >= bars.Low[bar]) && listHighFractal.Contains(temp))     // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BsyAndPby1MirrorForShort(int bar, List<int> listLowFractal, Bars bars, out int bsy) // Для зеркального уровня, позиция в шорт
        {
            int fine = StartBarForFindBsy(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.Low[temp] == bars.High[bar]*/(bars.Low[temp] >= bars.High[bar] && (bars.High[temp] - bars.StepPrice * bars.StepCount) <= bars.High[bar]) && listLowFractal.Contains(temp))     // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BsyAndBpy1High(int bar, List<int> listHighFractal, Bars bars, out int bsy)               // Повторяющийся уровень для шорта
        {
            int fine = StartBarForFindBsy(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.High[temp] == bars.High[bar]*/(bars.High[temp] >= bars.High[bar] && (bars.High[temp] - bars.StepPrice * bars.StepCount) <= bars.High[bar]) && listHighFractal.Contains(temp)) // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BsyAndBpy1Low(int bar, List<int> listLowFractal, Bars bars, out int bsy)                // Повторяющийся уровень для лонга
        {
            int fine = StartBarForFindBsy(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.Low[temp] == bars.Low[bar]*/(bars.Low[temp] <= bars.Low[bar] && (bars.Low[temp] + bars.StepPrice * bars.StepCount) >= bars.Low[bar]) && listLowFractal.Contains(temp))      // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool Bpy1AndBpy2High(int bar, Bars bars)              // Подтверждение повторяющегося уровеня для шорта (БПУ2)
        {
            if (bars.High[bar] >= (bars.High[bar - 1] - (bars.High[bar - 1] / 2500)) && bars.High[bar] <= bars.High[bar - 1])
                return true;
            return false;
        }
        public static bool Bpy1AndBpy2Low(int bar, Bars bars)               // Подтверждение повторяющегося уровеня для лонга (БПУ2)   
        {
            if (bars.Low[bar] <= (bars.Low[bar - 1] + (bars.Low[bar - 1] / 2500)) && bars.Low[bar] >= bars.Low[bar - 1])
                return true;
            return false;
        }
        public static string FindModelRepeatLevel(int bar, List<int> listHighFractal, List<int> listLowFractal, Bars bars, out int bsy)                // Начало анализа баров для поиска повторяющегося уровня 
        {
            if (BsyAndBpy1High(bar - 1, listHighFractal, bars, out bsy) && Bpy1AndBpy2High(bar, bars))
            {
                return "Short";
            }
            if (BsyAndBpy1Low(bar - 1, listLowFractal, bars, out bsy) && Bpy1AndBpy2Low(bar, bars))
            {
                return "Long";
            }
            return "Nothing";
        }
        public static string FindModelMirrorLevel(int bar, List<int> listHighFractal, List<int> listLowFractal, Bars bars, out int bsy)                // Начало анализа баров для поиска зеркального уровня 
        {
            if (BsyAndPby1MirrorForShort(bar - 1, listLowFractal, bars, out bsy) && Bpy1AndBpy2High(bar, bars))
                return "Short";
            if (BsyAndPby1MirrorForLong(bar - 1, listHighFractal, bars, out bsy) && Bpy1AndBpy2Low(bar, bars))
                return "Long";
            return "Nothing";
        }
    }
}
