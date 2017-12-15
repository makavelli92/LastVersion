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
using System.Windows.Forms;

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

        private static void AddOrder(List<Data> listBars, String[] substrings)
        {
            mtx.WaitOne();

            Bars temp = (Bars)listBars.FirstOrDefault(x => x.ClassCod == substrings[1] && x.Name == substrings[2] && x.Orders.Any(y => y.price == Double.Parse(substrings[3]) && y.operation == substrings[4]));

            Order order = temp.Orders.FirstOrDefault(y => y.price == Double.Parse(substrings[3]) && y.operation == substrings[4]);

            order.transactionId = Int64.Parse(substrings[6]);

            mtx.ReleaseMutex();
        }

        private static void AddStopProfitId(List<Data> listBars, String[] substrings)
        {
            mtx.WaitOne();

            Bars temp = (Bars)listBars.FirstOrDefault(x => x.ClassCod == substrings[1] && x.Name == substrings[2] && x.Orders.Any(y => y.OnlyOrder));

            Order order = temp.Orders.FirstOrDefault(y => y.OnlyOrder);

            order.StopProfitId = Int64.Parse(substrings[6]);

            mtx.ReleaseMutex();
        }

        private static void CheckForKillOrders(Bars bars, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand)
        {
            // Order order = bars.Orders.FirstOrDefault(x => x.deleteLevel > 0);
            List<Order> temp = new List<Order>();
            foreach (Order order in bars.Orders)
            {
                if (order.operation == "B")
                {
                    if (bars.High.Last() > order.deleteLevel)
                    {
                        Task.Run(() => { MessageBox.Show($"Цена перешла границу захода по инструменту {bars.Name}! {bars.High.Last()} > {order.deleteLevel}"); });
                        temp.Add(order);
                      //  bars.Orders.Remove(order);
                    }
                }
                if (order.operation == "S")
                {
                    if (bars.Low.Last() < order.deleteLevel)
                    {
                        Task.Run(() => { MessageBox.Show($"Цена перешла границу захода по инструменту {bars.Name}! {bars.Low.Last()} < {order.deleteLevel}"); });
                        temp.Add(order);
                      //  bars.Orders.Remove(order);
                    }
                }
            }
            temp.ForEach(x => bars.Orders.Remove(x));
            //if (order != null)
            //{
            //    if (order.operation == "B")
            //    {
            //        if (bars.High.Last() > order.deleteLevel)
            //        {
            //            Task.Run(() => { MessageBox.Show($"Цена перешла границу захода по инструменту {bars.Name}! {bars.High.Last()} > {order.deleteLevel}"); });
            //            bars.Orders.Remove(order);
            //        }
            //    }
            //    if (order.operation == "S")
            //    {
            //        if (bars.Low.Last() < order.deleteLevel)
            //        {
            //            Task.Run(() => { MessageBox.Show($"Цена перешла границу захода по инструменту {bars.Name}! {bars.Low.Last()} < {order.deleteLevel}"); });
            //            bars.Orders.Remove(order);
            //        }
            //    }
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            //Order order = bars.Orders.FirstOrDefault(x => x.transactionId > 0 && x.StopProfitId > 0 && x.numberOrder > 0 && x.numberStopProfitOrder > 0);
            //bool tmp = false;
            //if (order != null)
            //{
            //    if (order.operation == "B")
            //    {
            //        if (bars.High.Last() > order.deleteLevel)
            //        {
            //            DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberOrder + ';' + bars.Account, "KillOrder");
            //            DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberStopProfitOrder + ';' + bars.Account, "KillStopOrder");
            //            mtx.WaitOne();
            //            tmp = true;
            //            bars.Orders.Remove(order);
            //        }
            //    }
            //    if (order.operation == "S")
            //    {
            //        if (bars.Low.Last() < order.deleteLevel)
            //        {
            //            DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberOrder, "KillOrder");
            //            DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberStopProfitOrder, "KillStopOrder");
            //            mtx.WaitOne();
            //            tmp = true;
            //            bars.Orders.RemoveAll(x => x.transactionId == order.transactionId && x.StopProfitId == order.StopProfitId);
            //        }
            //    }
            //}
            //if (tmp)
            //    mtx.ReleaseMutex();
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
        }

        public static void CheckOrders(List<Data> listBars, String[] substrings, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand)
        {
            Bars bars = (Bars)listBars.FirstOrDefault(x => x.Name == substrings[1] && x.TimeFrame == Int32.Parse(substrings[2]) && x.Orders.Count > 0 && x.Orders.Any(y => y.StopProfitId > 0 && y.transactionId > 0));
            if(bars != null)
            {
                Order order = bars.Orders.FirstOrDefault(y => y.StopProfitId > 0 && y.transactionId > 0);
                bool tmp = false;
                if (substrings[3] == "DeleteAllOrders" || substrings[4] == "DeleteAllStopProfitOrders")
                {
                    if (substrings[3] == "DeleteAllOrders" && substrings[4] != "DeleteAllStopProfitOrders")
                    {
                        DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberStopProfitOrder + ';' + bars.Account, "KillStopOrder");
                        mtx.WaitOne();
                        tmp = true;
                        bars.Orders.RemoveAll(x => x.transactionId == order.transactionId && x.StopProfitId == order.StopProfitId);
                    }
                    if (substrings[3] != "DeleteAllOrders" && substrings[4] == "DeleteAllStopProfitOrders")
                    {
                        DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberOrder + ';' + bars.Account, "KillOrder");
                        mtx.WaitOne();
                        tmp = true;
                        bars.Orders.RemoveAll(x => x.transactionId == order.transactionId && x.StopProfitId == order.StopProfitId);
                    }
                    if (substrings[3] == "DeleteAllOrders" && substrings[4] == "DeleteAllStopProfitOrders" && !tmp)
                    {
                        mtx.WaitOne();
                        tmp = true;
                        bars.Orders.RemoveAll(x => x.transactionId == order.transactionId && x.StopProfitId == order.StopProfitId);
                    }

                }
                else
                {
                    mtx.WaitOne();
                    tmp = true;
                    if (Int64.Parse(substrings[3]) != order.numberOrder)
                        order.numberOrder = Int64.Parse(substrings[3]);

                    if (Int64.Parse(substrings[4]) != order.numberStopProfitOrder)
                        order.numberStopProfitOrder = Int64.Parse(substrings[4]);
                }
                if (!tmp)
                    mtx.WaitOne();
                bars.LastCheckTable = DateTime.Now;
                bars.SendCheckTable = true;
                mtx.ReleaseMutex();
            }
        }
        
        public static void AddData(List<Data> listBars, String[] substrings, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand)
        {
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            //if (substrings[0] == "orders")
            //{
            //    CheckOrders(listBars, substrings,  SW_Command,  SR_FlagCommand,  SW_FlagCommand);
            //    return;
            //}
            //if (substrings[0] == "Order")
            //{
            //    AddOrder(listBars, substrings);
            //    return;
            //}
            //else if (substrings[0] == "StopProfitOrder")
            //{
            //    AddStopProfitId(listBars, substrings);
            //    return;
            //}
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            mtx.WaitOne();
            Data temp = listBars.FirstOrDefault(x => x.Name == substrings[0] && x.TimeFrame == Int32.Parse(substrings[1]));

            if (substrings[1] != "0")
            {
                Bars tmp = temp as Bars;

                if (tmp.Count > 0 && !tmp.Time.Contains(DateTime.Parse(substrings[2])))
                {
                    Task.Run(() =>
                    {
                        Logger.Info($@"Не обнаружено прежних данных, формирую еще запрос на данные, теперь увеличу глубину запроса для - " + temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';');
                        // MessageBox.Show("Не обнаружено прежних данных");
                        int tempCount = (DateTime.Parse(substrings[2]) - tmp.LastTime).Minutes / tmp.TimeFrame;
                        DataReception.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + (temp.Count - tempCount - 1), "GetCandle");
                    });
                    return;
                }
                if (tmp.Count == 0 && tmp.history)
                {
                    DateTime time = tmp.From;
                    for (int i = 2; i < substrings.Length - 1; i += 6)
                    {
                        if (DateTime.Parse(substrings[i]) >= time)
                        {
                            tmp.temp = substrings.Length / 6 - (substrings.Length  - i) / 6;
                            break;
                        }
                    }
                }

                for (int i = 2; i <= substrings.Length + - 1/*(tmp.temp <= 0 ? -1 : -tmp.temp)*/; i += 6)
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
                    if (tmp.Count > tmp.temp && tmp.first == "first" && tmp.history)
                        Worker.StartStrategy(tmp);
                }
                tmp.first = "second";
                tmp.LastGet = DateTime.Now;
                //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
                if (tmp.Orders.Count > 0)// && tmp.Orders.Any(x => x.StopProfitId > 0 && x.transactionId > 0))
                {
                    CheckForKillOrders(tmp, SW_Command, SR_FlagCommand, SW_FlagCommand);
                }
                //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
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
                Ticks ticks = temp as Ticks;
                if (substrings[2] == "0")
                {
                    ticks.CountTicks = Int32.Parse(substrings[3]);
                    Task.Run(() =>
                    {
                        int tempCount = ticks.CountTicks - 1 > -1 ? ticks.CountTicks - 1 : 0;
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

                    ticks = temp as Ticks;
                    ticks.worker.StartFind((Ticks)temp);
                }
            }
            temp.ProcessType = "SendCommand";
            mtx.ReleaseMutex();
        }
    }
}
