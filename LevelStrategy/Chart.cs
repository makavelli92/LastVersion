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
        private Candle[] CandleArray; // это массив свечек
        private double stepPrice;
        private Bars bars;
        private DataReception data;

        public Chart(Bars bars, SignalData signal, DataReception data)
        {
            InitializeComponent();

            this.data = data;
            this.bars = bars;
            label1.Text = String.Empty;
            label2.Text = String.Empty;
            label3.Text = String.Empty;
            label4.Text = String.Empty;

            this.signal = signal;

            CreateChart();

            LoadCandleFromFile(bars);
            PaintData(signal);

            textBox1.Text = signal.Level.ToString();
            stepPrice = bars.StepPrice;
            CalculatePesent();
            ChartResize();
            this.Text = signal.SignalType;
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
            SetCommandOrder();
        }

        private void SetCommandOrder()
        {
            data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateOrderString(bars, signal), "SetOrder");
            //  data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateStopLossOrderString(bars, signal), "Set_SL");
            data.SetQUIKCommandDataObject(data.SW_Command, data.SR_FlagCommand, data.SW_FlagCommand, CreateTakeProfitStopLossString(bars, signal), "SetTP_SL");
        }

        private string CreateStopLossOrderString(Bars bars, SignalData signal)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(bars.Account).Append(';');
            builder.Append(bars.ClassCod).Append(';');
            builder.Append(bars.Name).Append(';');
            builder.Append(signal.SignalType[0] == 'S' ? "B" : "S").Append(';');
            builder.Append(signal.SignalType[0] == 'S' ? listLevelST[0] : listLevelST[3]).Append(';');
            builder.Append("1");
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
            builder.Append("1").Append(';');
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
            builder.Append("1");
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
            Candle[] newCandleArray;
            if (bars.Count > 0)
            {
                newCandleArray = new Candle[bars.Count];
                for (int i = 0; i < bars.Count; i++)
                {
                    newCandleArray[i] = new Candle() { close = bars.Close[i], high = bars.High[i], low = bars.Low[i], open = bars.Open[i], volume = bars.Volume[i], time = bars.Time[i] };
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

        private void LoadCandleOnChart(SignalData signal) // прогрузить загруженные свечки на график
        {
            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action<SignalData>(LoadCandleOnChart), signal);
                return;
            }
            if (CandleArray == null)
            {//если наш массив пуст по каким-то причинам
                return;
            }
            chartForCandle.Visible = false;
            
          //  MessageBox.Show("111");
            for (int i = 0; i < CandleArray.Length; i++)
            {// отправляем наш массив по свечкам на прорисовку
                LoadNewCandle(CandleArray[i], i);
            }

            DrawLine(signal);

            ChartResize();

            if (chartForCandle.InvokeRequired)
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Invoke(new Action(() => { chartForCandle.Visible = true; }));
                return;
            }
            else
                chartForCandle.Visible = true;

        }

        private void DrawLineStopAndTakeProfit(List<double> listLevel)
        {
            chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, signal.Level);
            for (int i = 0; i < listLevel.Count; i++)
            {
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, listLevel[i]);
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(CandleArray.Length - 1, listLevel[i]);
                chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, listLevel[i]);
            }
        }

        private void DrawLine(SignalData signal)
        {
            int candleBsy = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBsy));
            int candleBpy1 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBpy1));
            int candleBpy2 = CandleArray.ToList().IndexOf(CandleArray.First(x => x.time == signal.DateBpy2));

            
            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high);
            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);


            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy1, CandleArray[candleBpy1].high);
            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);

            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBsy, CandleArray[candleBsy].high);
            chartForCandle.Series.FindByName("SeriesCandleLine").Points.AddXY(candleBpy2, CandleArray[candleBpy2].high + CandleArray[candleBpy2].high / 100);

            chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(0, signal.Level);
            chartForCandle.Series.FindByName("SeriesCandleLineLevel").Points.AddXY(CandleArray.Length - 1, signal.Level);
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
                MessageBox.Show("Paint");
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

        private double GetMinValueOnChart(Candle[] Book, int start, int end) // берёт минимальное значение из массива свечек
        {
            double result = double.MaxValue;

            for (int i = start; i < end && i < Book.Length; i++)
            {
                if (Book[i].low < result)
                {
                    result = Book[i].low;
                }
            }

            return result;
        }

        private double GetMaxValueOnChart(Candle[] Book, int start, int end) // берёт максимальное значение из массива свечек
        {
            double result = 0;

            for (int i = start; i < end && i < Book.Length; i++)
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

            label1.Text = String.Format("TP 0.6 - {0}", Math.Round((temp / 100 * 0.6 + temp), countSigns).ToString());
            label2.Text = String.Format("ST 0.2 - {0}", Math.Round((temp / 100 * 0.2 + temp), countSigns).ToString());
            label3.Text = String.Format("ST -0.2 - {0}", Math.Round((temp - temp / 100 * 0.2), countSigns).ToString());
            label4.Text = String.Format("TP -0.6 - {0}", Math.Round((temp - temp / 100 * 0.6), countSigns).ToString());
            listLevelST = new List<double>()
            {
                Math.Round((temp / 100 * 0.6 + temp), countSigns),  Math.Round((temp / 100 * 0.2 + temp), countSigns),
                Math.Round((temp - temp / 100 * 0.2), countSigns), Math.Round((temp - temp / 100 * 0.6), countSigns)
            };
            DrawLineStopAndTakeProfit(listLevelST);
        }
    }
}
