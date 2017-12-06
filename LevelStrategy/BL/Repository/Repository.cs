using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LevelStrategy.DAL;
using LevelStrategy.Model;
using NLog;

namespace LevelStrategy.BL.Repository
{
    public class Repository
    {
        private static readonly ILogger Logger = LogManager.GetLogger("info");

        private static readonly Mutex mtx = new Mutex();

        public static void RemoveBarsIndex(Bars bars, int index)
        {
            bars.Open.RemoveAt(index);
            bars.Close.RemoveAt(index);
            bars.High.RemoveAt(index);
            bars.Low.RemoveAt(index);
            bars.Volume.RemoveAt(index);
            bars.Time.RemoveAt(index);
        }

        public static void AddData(List<Data> listBars, String[] substrings, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand)
        {
            mtx.WaitOne();
            Data temp = listBars.FirstOrDefault(x => x.Name == substrings[0] && x.TimeFrame == Int32.Parse(substrings[1]));

            if (substrings[1] != "0")
            {
                Bars tmp = temp as Bars;

                if (tmp.Count > 0 && !tmp.Time.Contains(DateTime.Parse(substrings[2])))
                {
                    Task.Run(() =>
                    {
                        Logger.Info($@"Не обнаружено прежних данных, формирую еще запрос на данные теперь увеличу глубину запроса для - " + temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';');
                        // MessageBox.Show("Не обнаружено прежних данных");
                        int tempCount = (DateTime.Parse(substrings[2]) - tmp.LastTime).Minutes / tmp.TimeFrame;
                        DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + (temp.Count - tempCount - 1), "GetCandle");
                    });
                    return;
                }
                //if (tmp.Count == 0)
                //{
                //    DateTime time = tmp.From;
                //    for (int i = 2; i < substrings.Length - 1; i += 6)
                //    {
                //        if (DateTime.Parse(substrings[i]) >= time)
                //        {
                //            tmp.temp = substrings.Length - i;
                //            break;
                //        }
                //    }
                //}

                for (int i = 2; i <= substrings.Length + (tmp.temp <= 0 ? -1 : -tmp.temp); i += 6)
                {
                    if (substrings[i] == String.Empty)
                        break;
                    if (tmp.Time.Contains(DateTime.Parse(substrings[i])))
                        RemoveBarsIndex(tmp, tmp.Time.IndexOf(DateTime.Parse(substrings[i])));
                    tmp.Time.Add(DateTime.Parse(substrings[i]));

                    tmp.Open.Add(Double.Parse(substrings[i + 1], CultureInfo.InvariantCulture));

                    tmp.High.Add(Double.Parse(substrings[i + 2], CultureInfo.InvariantCulture));

                    tmp.Low.Add(Double.Parse(substrings[i + 3], CultureInfo.InvariantCulture));

                    tmp.Close.Add(Double.Parse(substrings[i + 4], CultureInfo.InvariantCulture));

                    tmp.Volume.Add(Double.Parse(substrings[i + 5], CultureInfo.InvariantCulture));
                }

                //if (tmp.temp > 0)
                //    tmp.temp -= 6;
                Logger.Info($@"Данные принял и добавил успешно, отправляю на высчитыванеи индикаторов");
                //if (tmp.temp < 6)
                //{
                //    Worker.StartStrategy(tmp);
                //    temp.ProcessType = "SendCommand";
                //    mtx.ReleaseMutex();
                //    return;
                //}
                //else
                    Worker.StartStrategy(tmp);


                //Task.Run(() =>
                //{
                //    DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + (temp.Count), "GetCandle");
                //});
            }
            else
            {
                if (substrings[2] == "0")
                {
                    temp.Count = Int32.Parse(substrings[3]);
                    Task.Run(() =>
                    {
                        int tempCount = temp.Count - 1 > -1 ? temp.Count - 1 : 0;
                        DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + (tempCount), "GetCandle");
                    });
                }
                else
                {
                    for (int i = 3; i < substrings.Length - 1; i = i + 3)
                    {
                        temp.Time.Add(DateTime.Parse(substrings[i]));

                        temp.Close.Add(Double.Parse(substrings[i + 1], CultureInfo.InvariantCulture));

                        temp.Volume.Add(Double.Parse(substrings[i + 2], CultureInfo.InvariantCulture));
                    }
                }
            }
            temp.ProcessType = "SendCommand";
            mtx.ReleaseMutex();
        }
    }
}
