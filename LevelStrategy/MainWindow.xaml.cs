using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using LevelStrategy.Model;
// !!! добавляем нужные юзинги

namespace LevelStrategy
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        System.Windows.Forms.DataVisualization.Charting.Chart chartForCandle; // это хранится адрес для нашего чарта

        private string pathToHistory = null; // это адрес для строки с путём к  данным

        private Candle[] CandleArray; // это массив свечек

        public MainWindow(Bars bars) //окно загружается.
        {   
            InitializeComponent();

            // вызываем метод для создания чарта
            CreateChart();
            LoadCandleFromFile(bars);
        }

        private void CreateChart() // метод создающий чарт
        {
            // создаём чарт от Win Forms
            chartForCandle = new System.Windows.Forms.DataVisualization.Charting.Chart();
            // привязываем его к хосту.
            hostChart.Child = chartForCandle;
            hostChart.Child.Show();

            // на всякий случай чистим в нём всё
            chartForCandle.Series.Clear();
            chartForCandle.ChartAreas.Clear();

            // создаём область на чарте
            chartForCandle.ChartAreas.Add("ChartAreaCandle");
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorX.IsUserSelectionEnabled = true; // разрешаем пользователю изменять рамки представления
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorX.IsUserEnabled = true; //чертa
            chartForCandle.ChartAreas.FindByName("ChartAreaCandle").CursorY.AxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary; // ось У правая

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

            for (int i = 0; i < chartForCandle.ChartAreas.Count; i++)
            { // Делаем курсор по Y красным и толстым
                chartForCandle.ChartAreas[i].CursorX.LineColor = System.Drawing.Color.Red;
                chartForCandle.ChartAreas[i].CursorX.LineWidth = 2;
            }

            // подписываемся на события изменения масштабов
            chartForCandle.AxisScrollBarClicked += chartForCandle_AxisScrollBarClicked; // событие передвижения курсора
            chartForCandle.AxisViewChanged += chartForCandle_AxisViewChanged; // событие изменения масштаба
            chartForCandle.CursorPositionChanged += chartForCandle_CursorPositionChanged; // событие выделения диаграммы
        }

        private void buttonRew_Click(object sender, RoutedEventArgs e) // кнопка перемотать
        {
            chartForCandle.Visible = false; // прячем чарт

            Thread Worker = new Thread(Rewind);
            Worker.IsBackground = true;
            Worker.Start();// отправляем новый поток который через пять секунд откроет чарт
        }

        private void SendHistory_Click(object sender, RoutedEventArgs e) // подключаем историю
        {
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = true;
            myDialog.ShowDialog();

            if (myDialog.FileName != "") // если хоть что-то выбрано и это свечи
            { 
                // здесь происходит сохранение адреса выбранного фала.
                // по хорошему надо бы здесь поставить проверку, что в нём лежит
                pathToHistory = myDialog.FileName;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //if(pathToHistory == null)
            //{// если история ещё не подключена
            //    System.Windows.MessageBox.Show("Прежде чем прогрузить график, надо подгрузить историю");
            //    return;
            //}

            for (int i = 0; i < chartForCandle.Series.Count; i++)
            {// очищаем все свечки на графике
                chartForCandle.Series[i].Points.Clear();
            }

            // вызываем метод прорисовки графика, но делаем это отдельным потоком, чтобы форма оставалась живой
            Thread Worker = new Thread(StartPaintChart);
            Worker.IsBackground = true;
            Worker.Start();
        }

        private void StartPaintChart() // метод вызывающийся в новом потоке, для прорисовки графика
        {
           // LoadCandleFromFile();
            LoadCandleOnChart();
        }

        private void LoadCandleFromFile(Bars bars) // загрузить свечки из файла
        {
            Candle[] newCandleArray;
            if(bars.Count > 0)
            {
                newCandleArray = new Candle[bars.Count - 1];
                for (int i = 0; i < bars.Count - 1; i++)
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

        private void LoadCandleOnChart() // прогрузить загруженные свечки на график
        {

            if(CandleArray == null)
            {//если наш массив пуст по каким-то причинам
                return;
            }
            if (!this.CheckAccess())
            {// перезаходим в метод потоком формы, чтобы не было исключения
            //    Thread.Sleep(5000);// ждём пять секунд, чтобы свечи прорисовались
                this.Dispatcher.Invoke(new Action(LoadCandleOnChart));
                return;
            }
            chartForCandle.Visible = false;
            for (int i= 0; i < CandleArray.Length; i++)
            {// отправляем наш массив по свечкам на прорисовку
                LoadNewCandle(CandleArray[i],i);
            }
            chartForCandle.Visible = true;
        }

        private void LoadNewCandle(Candle newCandle, int numberInArray) // добавить одну свечу на график
        {
            if (!this.CheckAccess())
            {// перезаходим в метод потоком формы, чтобы не было исключения
                this.Dispatcher.Invoke(new Action<Candle, int>(LoadNewCandle), newCandle, numberInArray);
                return;
            }
            // забиваем новую свечку
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
            }

            if (chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScrollBar.IsVisible == true)
            {// если уже выбран какой-то диапазон
                chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisX.ScaleView.Scroll(chartForCandle.ChartAreas[0].AxisX.Maximum); // сдвигаем представление вправо
            }


            ChartResize(); // Выводим нормальные рамки
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


                chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Maximum = GetMaxValueOnChart(CandleArray, startPozition, endPozition);

                chartForCandle.ChartAreas.FindByName("ChartAreaCandle").AxisY2.Minimum = GetMinValueOnChart(CandleArray, startPozition, endPozition);

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
            if (!this.CheckAccess())
            {// перезаходим в метод потоком формы, чтобы не было исключения
                Thread.Sleep(5000);// ждём пять секунд, чтобы свечи прорисовались
                this.Dispatcher.Invoke(new Action(Rewind));
                return;
            }

            chartForCandle.Visible = true; // открываем чарт
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

    }
}
