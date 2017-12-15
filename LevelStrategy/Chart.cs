using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LevelStrategy.Model;
using System.Threading;
using LevelStrategy.DAL;

namespace LevelStrategy
{
    public partial class Chart : Form
    {
        private SignalData signal;
        private List<Candle> CandleArray; // это массив свечек
        private double stepPrice;
        private Bars bars;
        private DataReception data;
        private DateTime timeLastData;
        private List<double> drawLine;
        private static readonly Mutex mtx = new Mutex();

        public Chart(Bars bars, SignalData signal, DataReception data)
        {
            InitializeComponent();

            this.data = data;
            this.bars = bars;
            label1.Text = String.Empty;
            label2.Text = String.Empty;
            label3.Text = String.Empty;
            label4.Text = String.Empty;
            timeLastData = bars.LastGet;
            this.signal = signal;

            CreateChart();

            LoadCandleFromFile(bars);
            PaintData(signal);

            textBox1.Text = signal.Level.ToString();
            stepPrice = bars.StepPrice;
            CalculatePesent();
            ChartResize();
            this.Text = String.Format($"{bars.Name}_{bars.TimeFrame} - {signal.SignalType} || Люфт - {signal.CancelSignal}");
            textBox2.Text = "1";
            textBox3.Text = GetOneLotPrice(bars.Name).ToString();
        }

        private double GetOneLotPrice(string security)
        {
            int countLot = 0;
            if(textBox2.Text != String.Empty)
            {
                countLot = Int32.Parse(textBox2.Text);
            }
            if (security.Substring(0, 2) == "GZ" && security.Substring(0, 2) == "SR" && security.Substring(0, 2) == "Eu" && security.Substring(0, 2) == "GD" && security.Substring(0, 2) == "RI" && security.Substring(0, 2) == "Si" && security.Substring(0, 2) == "BR")
                security = security.Substring(0, 2);
            switch (security)
            {
                case "SBER":
                    return signal.Level * 10 * countLot;
                case "MOEX":
                    return signal.Level * 10 * countLot;
                case "ROSN":
                    return signal.Level * 10 * countLot;
                case "NLMK":
                    return signal.Level * 10 * countLot;
                case "LKOH":
                    return signal.Level * 1 * countLot;
                case "GAZP":
                    return signal.Level * 10 * countLot;
                case "URKA":
                    return signal.Level * 10 * countLot;
                case "CHMF":
                    return signal.Level * 10 * countLot;
                case "VTBR":
                    return signal.Level * 10000 * countLot;
                case "MAGN":
                    return signal.Level * 100 * countLot;
                case "MGNT":
                    return signal.Level * 1 * countLot;
                case "NVTK":
                    return signal.Level * 10 * countLot;
                case "SBERP":
                    return signal.Level * 100 * countLot;
                case "GMKN":
                    return signal.Level * 1 * countLot;
                case "ALRS":
                    return signal.Level * 100 * countLot;
                case "MTSS":
                    return signal.Level * 10 * countLot;
                case "PHOR":
                    return signal.Level * 1 * countLot;
                case "AFLT":
                    return signal.Level * 100 * countLot;
                case "YNDX":
                    return signal.Level * 1 * countLot;
                case "MTLRP":
                    return signal.Level * 10 * countLot;
                case "SNGS":
                    return signal.Level * 100 * countLot;
                case "FEES":
                    return signal.Level * 10000 * countLot;
                case "RTKM":
                    return signal.Level * 10 * countLot;
                case "RASP":
                    return signal.Level * 10 * countLot;
                case "GZ":
                    return signal.Level * 100 * countLot;
                case "SR":
                    return signal.Level * 100 * countLot;
                case "Eu":
                    return signal.Level * 1000 * countLot; ;
                case "GD":
                    return signal.Level * 1 * countLot;
                case "RI":
                    return signal.Level * 1 * countLot;
                case "Si":
                    return signal.Level * 1000 * countLot;
                case "BR":
                    return signal.Level * 10 * countLot;
                default:
                    return 0;
            }
        }

        private void PaintData(SignalData signal)
        {
            //for (int i = 0; i < chartForCandle.Series.Count; i++)
            //{// очищаем все свечки на графике
            //    chartForCandle.Series[i].Points.Clear();
            //}
            // вызываем метод прорисовки графика, но делаем это отдельным потоком, чтобы форма оставалась живой
            //  Task.Run(() => { StartPaintChart(signal); });
            Thread Worker = new Thread(StartPaintChart);
            Worker.IsBackground = true;
            Worker.Start();
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            chartForCandle.Visible = false; // прячем чарт

            Thread Worker = new Thread(Rewind);
            Worker.IsBackground = true;
            Worker.Start();// отправляем новый поток который через пять секунд откроет чарт
        }
        

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            SetCommandOrder();
            button3.Enabled = true;
        }

        private void SetCommandOrder()
        {
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            mtx.WaitOne();
            bars.Orders.Add(new Order(bars.ClassCod, bars.Name, Double.Parse(textBox1.Text), signal.SignalType[0] == 'S' ? "S" : "B", Int32.Parse(textBox2.Text), signal.CancelSignal));

            mtx.ReleaseMutex();
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateOrderString(bars, signal), "SetOrder");
            //  data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateStopLossOrderString(bars, signal), "Set_SL");
            data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateTakeProfitStopLossString(bars, signal), "SetTP_SL");
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            //while (!bars.Orders.Any(x => x.transactionId > 0 && x.StopProfitId > 0))
            //    Thread.Sleep(1000);
            //data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, String.Format($"{bars.Name};{bars.TimeFrame}"), "CheckTable");
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
        }

        private string CreateStopLossOrderString(Bars bars, SignalData signal)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(bars.Account).Append(';');
            builder.Append(bars.ClassCod).Append(';');
            builder.Append(bars.Name).Append(';');
            builder.Append(signal.SignalType[0] == 'S' ? "B" : "S").Append(';');
            builder.Append(signal.SignalType[0] == 'S' ? listLevelST[0] : listLevelST[3]).Append(';');
            builder.Append(textBox2.Text);
            return builder.ToString();
        }
        
        private string CreateTakeProfitStopLossString(Bars bars, SignalData signal)
        {
            double level = Double.Parse(textBox1.Text);
            int profit_size = (int)Math.Abs((level - (signal.SignalType[0] == 'S' ? listLevelST[3] : listLevelST[0])) / bars.StepPrice);
            int stop_size = (int)(Math.Abs((level -(signal.SignalType[0] == 'S' ? listLevelST[1] : listLevelST[2])) / bars.StepPrice));
            StringBuilder builder = new StringBuilder();
            builder.Append(bars.Account).Append(';');
            builder.Append(bars.ClassCod).Append(';');
            builder.Append(bars.Name).Append(';');
            builder.Append(signal.SignalType[0] == 'S' ? "B" : "S").Append(';');
            builder.Append(textBox1.Text).Append(';');
            builder.Append(textBox2.Text).Append(';');
            builder.Append(profit_size).Append(';');
            builder.Append(stop_size);
            return builder.ToString();
        }

        private string CreateOrderString(Bars bars, SignalData signal)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(bars.Account).Append(';');
            builder.Append(bars.ClassCod).Append(';');
            builder.Append(bars.Name).Append(';');
            builder.Append(textBox1.Text).Append(';');
            builder.Append(signal.SignalType[0] == 'S'?"S":"B").Append(';');
            builder.Append(textBox2.Text);
            return builder.ToString();
        }

        private void CreateChart() // метод создающий чарт
        {
            // на всякий случай чистим в нём всё
            chartForCandle.Series.Clear();
            chartForCandle.ChartAreas.Clear();

            // создаём область на чарте
            chartForCandle.ChartAreas.Add("ChartAreaCandle");
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorX.IsUserSelectionEnabled = true; // разрешаем пользователю изменять рамки представления
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorX.IsUserEnabled = true; //чертa
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorY.AxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary; // ось У правая
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").BackColor = Color.Black;

            // создаём для нашей области коллекцию значений
            chartForCandle.Series.Add("SeriesCandle");
            // назначаем этой коллекции тип "Свечи"
            chartForCandle.Series.FindByName("SeriesCandle").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
            chartForCandle.Series.FindByName("SeriesCandle").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            // помещаем нашу коллекцию на ранее созданную область
            chartForCandle.Series.FindByName("SeriesCandle").ChartArea = "ChartAreaCandle";
            // наводим тень
            chartForCandle.Series.FindByName("SeriesCandle").ShadowOffset = 2;

            //делаем чарт для рисования линий
            {

                chartForCandle.Series.Add("SeriesCandleLine");
                // назначаем этой коллекции тип "Line"
                chartForCandle.Series.FindByName("SeriesCandleLine").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                chartForCandle.Series.FindByName("SeriesCandleLine").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                // помещаем нашу коллекцию на ранее созданную область
                chartForCandle.Series.FindByName("SeriesCandleLine").ChartArea = "ChartAreaCandle";
                chartForCandle.Series.FindByName("SeriesCandleLine").BorderWidth = 2;

                chartForCandle.Series.Add("SeriesCandleLineLevel");
                // назначаем этой коллекции тип "Line"
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                // помещаем нашу коллекцию на ранее созданную область
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").ChartArea = "ChartAreaCandle";
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Color = Color.Blue;
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").BorderWidth = 1;

                chartForCandle.Series.Add("SeriesCandleLineStop");
                // назначаем этой коллекции тип "Line"
                chartForCandle.Series.FindByName("SeriesCandleLineStop").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                chartForCandle.Series.FindByName("SeriesCandleLineStop").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                // помещаем нашу коллекцию на ранее созданную область
                chartForCandle.Series.FindByName("SeriesCandleLineStop").ChartArea = "ChartAreaCandle";
                chartForCandle.Series.FindByName("SeriesCandleLineStop").Color = Color.Red;
                chartForCandle.Series.FindByName("SeriesCandleLineStop").BorderWidth = 1;

                chartForCandle.Series.Add("SeriesCandleLineTake");
                // назначаем этой коллекции тип "Line"
                chartForCandle.Series.FindByName("SeriesCandleLineTake").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                chartForCandle.Series.FindByName("SeriesCandleLineTake").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                // помещаем нашу коллекцию на ранее созданную область
                chartForCandle.Series.FindByName("SeriesCandleLineTake").ChartArea = "ChartAreaCandle";
                chartForCandle.Series.FindByName("SeriesCandleLineTake").Color = Color.Lime;
                chartForCandle.Series.FindByName("SeriesCandleLineTake").BorderWidth = 1;

                chartForCandle.Series.Add("SeriesCandleLineLyftCancel");
                // назначаем этой коллекции тип "Line"
                chartForCandle.Series.FindByName("SeriesCandleLineLyftCancel").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                chartForCandle.Series.FindByName("SeriesCandleLineLyftCancel").YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                // помещаем нашу коллекцию на ранее созданную область
                chartForCandle.Series.FindByName("SeriesCandleLineLyftCancel").ChartArea = "ChartAreaCandle";
                chartForCandle.Series.FindByName("SeriesCandleLineLyftCancel").Color = Color.Magenta;
                chartForCandle.Series.FindByName("SeriesCandleLineLyftCancel").BorderWidth = 1;
            }

            for (int i = 0; i < chartForCandle.ChartAreas.Count; i++)
            { // Делаем курсор по Y красным и толстым
                chartForCandle.ChartAreas[i].CursorX.LineColor = System.Drawing.Color.Red;
                chartForCandle.ChartAreas[i].CursorX.LineWidth = 2;
            }
            chartForCandle.Legends.Clear();
            // подписываемся на события изменения масштабов
            chartForCandle.AxisScrollBarClicked += chartForCandle_AxisScrollBarClicked; // событие передвижения курсора
            chartForCandle.AxisViewChanged += chartForCandle_AxisViewChanged; // событие изменения масштаба
            chartForCandle.CursorPositionChanged += chartForCandle_CursorPositionChanged; // событие выделения диаграммы
        }
        
        public void StartPaintChart() // метод вызывающийся в новом потоке, для прорисовки графика
        {
            // LoadCandleFromFile();
            LoadCandleOnChart(signal);
        }

        private void LoadCandleFromFile(Bars bars) // загрузить свечки из файла
        {
            List<Candle> newCandleArray;
            if (bars.Count > 0)
            {
                newCandleArray = new List<Candle>();
                for (int i = 0; i < bars.Count; i++)
                {
                    newCandleArray.Add(new Candle() { close = bars.Close[i], high = bars.High[i], low = bars.Low[i], open = bars.Open[i], volume = bars.Volume[i], time = bars.Time[i] });
                }
                CandleArray = newCandleArray;
            }

            //try
            //{ // используем перехватчик исключений, т.к. файл может быть занят или содержать каку.

            //    Candle[] newCandleArray;

            //    int lenghtArray = 0;

            //    using(StreamReader Reader = new StreamReader(pathToHistory))
            //    {// подсоединяемся к файлу
            //        while(!Reader.EndOfStream)
            //        {//считаем кол-во строк
            //            lenghtArray++;
            //            Reader.ReadLine();
            //        }
            //    }

            //    newCandleArray = new Candle[lenghtArray]; // создаём новый массив для свечек


            //    using (StreamReader Reader = new StreamReader(pathToHistory))
            //    {// подсоединяемся к файлу

            //        for (int iteratorArray = 0; iteratorArray < newCandleArray.Length; iteratorArray++)
            //        {// закачиваем свечки из файла в массив
            //            newCandleArray[iteratorArray] = new Candle();
            //            newCandleArray[iteratorArray].SetCandleFromString(Reader.ReadLine());
            //        }
            //    }

            // сохраняем изменения
            //}
            //catch (Exception error)
            //{
            //    System.Windows.MessageBox.Show("Произошла ошибка при скачивании данных из файла. Ошибка: " + error.ToString());
            //}

        }

        private void LoadCandleOnChart(SignalData signal, int startPos = 0) // прогрузить загруженные свечки на график
        {
            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action<SignalData, int>(LoadCandleOnChart), signal, startPos);
                return;
            }
            if (CandleArray == null)
            {//если наш массив пуст по каким-то причинам
                return;
            }
            chartForCandle.Visible = false;
            
            if (startPos == 0)
            {
                for (int i = 0; i < CandleArray.Count; i++)
                {// отправляем наш массив по свечкам на прорисовку
                    LoadNewCandle(CandleArray[i], i);
                }

                DrawLine(signal);
            }
            else
            {
                for (int i = startPos - 1; i < CandleArray.Count; i++)
                {// отправляем наш массив по свечкам на прорисовку
                    LoadNewCandle(CandleArray[i], i);
                }
            }
            ChartResize();

            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action(() => { chartForCandle.Visible = true; }));
                return;
            }
            else
                chartForCandle.Visible = true;
        }

        private void DrawLineStopAndTakeProfit(List<double> listLevel, bool second = false)
        {
            for (int i = 2; i < 6; i++)
            {
                if (!second)
                {
                    chartForCandle.Series[i].Points.AddXY(0, listLevel[i - 2]);
                    chartForCandle.Series[i].Points.AddXY(CandleArray.Count, listLevel[i - 2]);
                }
                else
                    chartForCandle.Series[i].Points.AddXY(CandleArray.Count, listLevel[i - 2]);
            }
        }

        //private void DrawLineStopAndTakeProfit(List<double> listLevel)
        //{
        //    chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, signal.Level);
        //    for (int i = 0; i < listLevel.Count; i++)
        //    {
        //        chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, listLevel[i]);
        //        chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(CandleArray.Count - 1, listLevel[i]);
        //        chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, listLevel[i]);
        //    }
        //}

        private void DrawLine(SignalData signal)
        {
            int candleBsy;
            int candleBpy1;
            int candleBpy2;
            if (!hourTimeFrame)
            {
                candleBsy = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBsy));
                candleBpy1 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBpy1));
                candleBpy2 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBpy2));
            }
            else
            {
                candleBsy = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time.Date == signal.DateBsy.Date));
                candleBpy1 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time.Date == signal.DateBpy1.Date));
                candleBpy2 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time.Date == signal.DateBpy2.Date));
            }

                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high);
                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);


                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy1, CandleArray[candleBpy1].high);
                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);

                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBsy, CandleArray[candleBsy].high);
                chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);

                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, signal.Level);
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(CandleArray.Count - 1, signal.Level);
            
        }

        private void LoadNewCandle(Candle newCandle, int numberInArray) // добавить одну свечу на график
        {
            // забиваем новую свечку
            try
            {
                chartForCandle.Series.FindByName("SeriesCandle").Points.AddXY(numberInArray, newCandle.low, newCandle.high, newCandle.open, newCandle.close);

                // подписываем время
                chartForCandle.Series.FindByName("SeriesCandle").Points[chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1].AxisLabel = newCandle.time.ToString();

                // разукрышиваем в привычные цвета
                if (newCandle.close > newCandle.open)
                {
                    chartForCandle.Series.FindByName("SeriesCandle").Points[chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1].Color = System.Drawing.Color.Green;
                }
                else
                {
                    chartForCandle.Series.FindByName("SeriesCandle").Points[chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1].Color = System.Drawing.Color.Red;
                    chartForCandle.Series.FindByName("SeriesCandle").Points[chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1].BackSecondaryColor = System.Drawing.Color.Red;
                }

                if (chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScrollBar.IsVisible == true)
                {// если уже выбран какой-то диапазон
                    chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Maximum); // сдвигаем представление вправо
                }


              //  ChartResize(); // Выводим нормальные рамки
            }
            catch
            {// перезаходим в метод потоком формы, чтобы не было исключения
              //  MessageBox.Show("Paint");
                Invoke(new Action<Candle, int>(LoadNewCandle), newCandle, numberInArray);
                return;
            }
        }

        private void ChartResize() // устанавливает границы представления по оси У
        {// вообще-то можно это автоматике доверить, но там вечно косяки какие-то, поэтому лучше самому следить за всеми осями
            try
            {
                if (CandleArray == null)
                {
                    return;
                }

                int startPozition = 0; // первая отображаемая свеча
                int endPozition = chartForCandle.Series.FindByName("SeriesCandle").Points.Count; // последняя отображаемая свеча

                if (chartForCandle.ChartAreas[0].AxisX.ScrollBar.IsVisible == true)
                {// если уже выбран какой-то диапазон, назначаем первую и последнюю исходя из этого диапазона
                    startPozition = Convert.ToInt32(chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.Position);
                    endPozition = Convert.ToInt32(chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.Position) +
                       Convert.ToInt32(chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.Size);
                }


                chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Maximum = GetMaxValueOnChart(CandleArray, startPozition, endPozition) + GetMaxValueOnChart(CandleArray, startPozition, endPozition) * 0.001;

                chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Minimum = GetMinValueOnChart(CandleArray, startPozition, endPozition) - GetMinValueOnChart(CandleArray, startPozition, endPozition) * 0.001;

                if(listLevelST.Count > 3)
                {
                    chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Maximum = chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Maximum > listLevelST[0] + listLevelST[0] * 0.001 ? chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Maximum : listLevelST[0] + listLevelST[0] * 0.001;

                    chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Minimum = chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Minimum < listLevelST[3] - listLevelST[3] * 0.001 ? chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Minimum : listLevelST[3] - listLevelST[3] * 0.001;
                }


                chartForCandle.Refresh();
            }
            catch
            {
                return;
            }
        }

        private double GetMinValueOnChart(List<Candle> Book, int start, int end) // берёт минимальное значение из массива свечек
        {
            double result = double.MaxValue;

            for (int i = start; i < end && i < Book.Count; i++)
            {
                if (Book[i].low < result)
                {
                    result = Book[i].low;
                }
            }

            return result;
        }

        private double GetMaxValueOnChart(List<Candle> Book, int start, int end) // берёт максимальное значение из массива свечек
        {
            double result = 0;

            for (int i = start; i < end && i < Book.Count; i++)
            {
                if (Book[i].high > result)
                {
                    result = Book[i].high;
                }
            }

            return result;
        }

        private void Rewind() // перемотка
        {
            try
            {
                chartForCandle.Visible = true; // открываем чарт
                
            }
            catch
            {
                Invoke(new Action(Rewind));
                return;
            }
        }

        // события
        void chartForCandle_CursorPositionChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e) // событие изменение отображения диаграммы
        {
            ChartResize();
        }

        void chartForCandle_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e) // событие изменение отображения диаграммы 
        {
            ChartResize();
        }

        void chartForCandle_AxisScrollBarClicked(object sender, System.Windows.Forms.DataVisualization.Charting.ScrollBarEventArgs e) // событие изменение отображения диаграммы
        {
            ChartResize();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox1.Text != String.Empty)
                CalculatePesent();
            else
            {
                label1.Text = String.Empty;
                label2.Text = String.Empty;
                label3.Text = String.Empty;
                label4.Text = String.Empty;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9\,\.]") && e.KeyChar != 8)
                e.Handled = true;
        }

        private static int CalculateSignsCount(string number)
        {
            if (!number.Replace(".", ",").Contains(","))
                return 1;
            return (number.Length - 1) - number.Replace(".", ",").IndexOf(",");
        }
        private List<double> listLevelST;
        private int countSigns;
        private void CalculatePesent()
        {
            if (textBox1.Text[textBox1.TextLength - 1] == ',' || textBox1.Text[textBox1.TextLength - 1] == '.')
                return;

            double temp = Double.Parse(textBox1.Text.Replace(".", ","));

            if (textBox1.Text.Replace(".", ",").Contains(",") && stepPrice == 0)
            {
                countSigns = CalculateSignsCount(textBox1.Text);
            }
            else
            {
                countSigns = CalculateSignsCount(stepPrice.ToString());
            }

            label1.Text = String.Format("TP 1.2 - {0}", Math.Round((temp / 100 * 1.2 + temp), countSigns).ToString());
            label2.Text = String.Format("ST 0.4 - {0}", Math.Round((temp / 100 * 0.4 + temp), countSigns).ToString());
            label3.Text = String.Format("ST -0.4 - {0}", Math.Round((temp - temp / 100 * 0.4), countSigns).ToString());
            label4.Text = String.Format("TP -1.2 - {0}", Math.Round((temp - temp / 100 * 1.2), countSigns).ToString());
            listLevelST = new List<double>()
            {
                Math.Round((temp / 100 * 1.2 + temp), countSigns),  Math.Round((temp / 100 * 0.4 + temp), countSigns),
                Math.Round((temp - temp / 100 * 0.4), countSigns), Math.Round((temp - temp / 100 * 1.2), countSigns)
            };
            drawLine = GetLewelForPaint(signal, listLevelST);
            DrawLineStopAndTakeProfit(drawLine);
        }

        private List<double> GetLewelForPaint(SignalData signal, List<double> allLevel)
        {
            List<double> temp = new List<double>();
            temp.Add(signal.Level);
            temp.Add(signal.SignalType[0] == 'S'?allLevel[1]:allLevel[2]);
            temp.Add(signal.SignalType[0] == 'S' ? allLevel[3] : allLevel[0]);
            temp.Add(signal.CancelSignal);
            return temp;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (button1.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action(() => { button1.Enabled = false; ; }));
            }
            else
                button1.Enabled = false;

            data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + bars.TimeFrame + ';' + bars.Count, "GetCandle");
            Task.Run(() => { AddLastCandle(); });
        }

        private void AddLastCandle()
        {
            while (timeLastData == bars.LastGet)
            {
                Thread.Sleep(100);
            }
            timeLastData = bars.LastGet;
            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action(() =>
                {
                    chartForCandle.Visible = false;

                    chartForCandle.Series.FindByName("SeriesCandle").Points.RemoveAt(chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1);

                    chartForCandle.Series.FindByName("SeriesCandle").Points.RemoveAt(chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1);

                    CandleArray.RemoveAt(CandleArray.Count - 1);

                }));
            }
            else
            {
                chartForCandle.Visible = false;
                chartForCandle.Series.FindByName("SeriesCandle").Points.RemoveAt(chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1);
                chartForCandle.Series.FindByName("SeriesCandle").Points.RemoveAt(chartForCandle.Series.FindByName("SeriesCandle").Points.Count - 1);
                CandleArray.RemoveAt(CandleArray.Count - 1);
            }
            

            int startPos = CandleArray.Count;

            for (int i = CandleArray.Count; i < bars.Count; i++)
            {
                CandleArray.Add(new Candle() { close = bars.Close[i], high = bars.High[i], low = bars.Low[i], open = bars.Open[i], volume = bars.Volume[i], time = bars.Time[i] });
            }


            LoadCandleOnChart(this.signal, startPos);
            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action(() =>
                {
                    //   chartForCandle.Series.Clear();
                    //   chartForCandle.ChartAreas.Clear();
                    chartForCandle.Refresh();
                    DrawLineStopAndTakeProfit(drawLine, true);
                    chartForCandle.ChartAreas[0].AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Maximum);
                    button1.Enabled = true;
                }));
            }
            else
            {
                //   chartForCandle.Series.Clear();
                //    chartForCandle.ChartAreas.Clear();
                chartForCandle.Refresh();
                DrawLineStopAndTakeProfit(drawLine, true);
                chartForCandle.ChartAreas[0].AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Maximum);
                button1.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mtx.WaitOne();
            int counter = 0;
            List<Order> listOrders = new List<Order>();
            foreach (Order i in bars.Orders)
            {
                counter++;
                DialogResult temp = MessageBox.Show($"Удалить ордер {counter} : {i.classCode} {i.security} операция - {i.operation} кол-во - {i.quantity} по цене {i.price}","Удаление ордеров из проверки", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
                if (temp == DialogResult.Yes)
                {
                    listOrders.Add(i);
                }
                else if (temp == DialogResult.Cancel)
                    break;
            }
            listOrders.ForEach(x => bars.Orders.Remove(x));
            
          //  bars.Orders.RemoveRange(0, bars.Orders.Count);
            mtx.ReleaseMutex();
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
            //if(bars.Orders.Count > 0)
            //{
            //    Order order = bars.Orders.FirstOrDefault(x => x.transactionId > 0 && x.StopProfitId > 0 && x.numberOrder > 0 && x.numberStopProfitOrder > 0);
            //    if (order != null)
            //    {
            //        DataReception.SetQUIKCommandData(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberOrder + ';' + bars.Account, "KillOrder");
            //        DataReception.SetQUIKCommandData(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, bars.ClassCod + ';' + bars.Name + ';' + order.numberStopProfitOrder + ';' + bars.Account, "KillStopOrder");
            //        bars.Orders.RemoveAll(x => x.transactionId == order.transactionId && x.StopProfitId == order.StopProfitId);
            //    }
            //    else
            //        MessageBox.Show("Не определны номера ордеров!");
            //}
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9]") && e.KeyChar != 8)
                e.Handled = true;
        }

        private bool hourTimeFrame = false;

        private void button4_Click(object sender, EventArgs e)
        {
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.ZoomReset();
            for (int i = 0; i < 6; i++)
            {
                chartForCandle.Series[i].Points.Clear();
            }
            if (!hourTimeFrame)
            {
                List<Candle> candleList = new List<Candle>();
                Candle temp = new Candle();
                DateTime start = CandleArray[0].time;
                temp.time = CandleArray[0].time;
                temp.open = CandleArray[0].open;
                temp.high = CandleArray[0].high;
                temp.low = CandleArray[0].low;
                foreach (Candle i in CandleArray)
                {
                    if (i.time.Date > start.Date)
                    {
                        start = i.time;
                        candleList.Add(temp);
                        temp = new Candle();
                        temp.time = start;
                        temp.open = i.open;
                        temp.high = i.high;
                        temp.low = i.low;
                        temp.close = i.close;
                    }
                    else if (i.time.Date == start.Date)
                    {
                        temp.close = i.close;
                        if (i.high > temp.high)
                            temp.high = i.high;
                        if (i.low < temp.low)
                            temp.low = i.low;
                    }
                }
                candleList.Add(temp);
                CandleArray = candleList;
                PaintData(signal);
                DrawLineStopAndTakeProfit(drawLine);
                chartForCandle.ChartAreas[0].AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Minimum);
                hourTimeFrame = true;
                button1.Enabled = false;
            }
            else
            {
                LoadCandleFromFile(bars);
                PaintData(signal);
                DrawLineStopAndTakeProfit(drawLine);
                chartForCandle.ChartAreas[0].AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Minimum);
                hourTimeFrame = false;
                button1.Enabled = true;
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            textBox3.Text = GetOneLotPrice(bars.Name).ToString();
        }
    }
}
