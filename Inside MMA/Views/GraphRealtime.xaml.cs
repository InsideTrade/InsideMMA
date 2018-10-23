using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Xml.Serialization;
using MVVM_Solution.Models;
using MVVM_Solution.ViewModels;
using ComboBox = System.Windows.Controls.ComboBox;
using DataFormats = System.Windows.Forms.DataFormats;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace MVVM_Solution.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ChartWindow
    {
        private int _timeframe;
        private List<Tick> list_ticks = new List<Tick>();
        private Chart _chartForCandle; // это хранится адрес для нашего чарта
        private string _board;
        private string _seccode;
        private Candle[] _candleArray; // это массив свечек
        public ChartWindow(string board, string seccode, int timeframe) //окно загружается.
        {
            _board = board;
            _seccode = seccode;
            _timeframe = timeframe;
            TXmlConnector.SendNewTicks += TicksToCandles;
            var cmd = ConnectorCommands.SubUnbubTics("subscribe_ticks", board, seccode, "1", "true");
            TXmlConnector.ConnectorSendCommand(cmd);
            InitializeComponent();
            // вызываем метод для создания чарта
            CreateChart();
        }

        private void ChartWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var cmd = ConnectorCommands.SubUnbubTics("unsubscribe_ticks", _board, _seccode, "1", "true");
            TXmlConnector.ConnectorSendCommand(cmd);
            TXmlConnector.SendNewTicks -= TicksToCandles;
        }


        private void TicksToCandles(string data)
        {
            List<Tick> ticks = new List<Tick>();
            if (data != null)
            {
                ticks =
                    ((List<Tick>)
                        new XmlSerializer(typeof(List<Tick>), new XmlRootAttribute("ticks")).Deserialize(
                            new StringReader(data))).Where(item => item.Board == _board && item.Seccode == _seccode)
                        .ToList();
            }
            

            if (list_ticks == null)
                list_ticks = ticks;
            list_ticks?.AddRange(ticks);

            var interval = new TimeSpan(0, _timeframe, 0);
            var array = (from t in list_ticks
                                 group t by DateTime.Parse(t.Tradetime).Ticks / interval.Ticks
                                 into g
                                 select new Candle
                                 {
                                     High = (from t2 in g select t2.Price).Max(),
                                     Low = (from t2 in g select t2.Price).Min(),
                                     Open = g.First().Price,
                                     Close = g.Last().Price,
                                     Volume = g.Count(),
                                     Time = g.First().Tradetime
                                 }).ToArray();

            if (array.Length == _candleArray?.Length)
            {
                LoadNewCandle(array.Last(), array.Length - 1);
                _candleArray = array;
            }
            else
            {
                _candleArray = array;
                var worker = new Thread(FastStart) { IsBackground = true };
                worker.Start();
            }
        }


        private void CreateChart() // метод создающий чарт
        {
            // создаём чарт от Win Forms
            _chartForCandle = new Chart();
            _chartForCandle.BackColor = Color.FromArgb(212121);
            // привязываем его к хосту. 
            ChartHost.Child = _chartForCandle;
            ChartHost.Child.Show();

            // на всякий случай чистим в нём всё
            _chartForCandle.Series.Clear();
            _chartForCandle.ChartAreas.Clear();

            // свечи
            ChartArea candleArea = new ChartArea("ChartAreaCandle") // создаём область на графике
            {
                CursorX = {IsUserSelectionEnabled = true, IsUserEnabled = true},
                // разрешаем пользователю изменять рамки представления
                CursorY = {AxisType = AxisType.Secondary},
                Position = {Height = 70, X = 0, Width = 100, Y = 0}, //чертa
                ShadowOffset = 0,
                BackColor = Color.FromArgb(212121)
            };
            candleArea.AxisX.ScrollBar.LineColor = Color.White;
            candleArea.AxisX.ScrollBar.ButtonColor = Color.Gray;
            candleArea.AxisX.ScrollBar.BackColor = Color.Black;
            _chartForCandle.ChartAreas.Add(candleArea); // добавляем область на чарт
            _chartForCandle.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            _chartForCandle.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            _chartForCandle.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisX.MinorTickMark.LineColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisX.LineColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            _chartForCandle.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;
            _chartForCandle.ChartAreas[0].AxisY2.LineColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisY2.MajorTickMark.LineColor = Color.White;
            _chartForCandle.ChartAreas[0].AxisY2.MinorTickMark.LineColor = Color.White;

            Series candleSeries = new Series("SeriesCandle") // создаём для нашей области коллекцию значений
            {
                ChartType = SeriesChartType.Candlestick, // назначаем этой коллекции тип "Свечи"
                YAxisType = AxisType.Secondary, // назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                ChartArea = "ChartAreaCandle" // помещаем нашу коллекцию на ранее созданную область
            };

            _chartForCandle.Series.Add(candleSeries); // добавляем коллекцию на чарт

// объём
            ChartArea volumeArea = new ChartArea("ChartAreaVolume") // создаём область для объёма
            {
                CursorX = {IsUserEnabled = true}, //чертa
                CursorY = {AxisType = AxisType.Secondary }, // ось У правая
                AlignWithChartArea = "ChartAreaCandle",// выравниваем по верхней диаграмме
                Position = {Height = 30, X = 0, Width = 100, Y = 70},
                AxisX = { Enabled = AxisEnabled.False},// отменяем прорисовку оси Х
                ShadowOffset = 0,
                BackColor = Color.FromArgb(212121)
            };
            volumeArea.CursorX.LineColor = Color.White;
            volumeArea.AxisY2.MajorGrid.Enabled = false;
            volumeArea.AxisY2.MinorGrid.Enabled = false;
            volumeArea.AxisY2.MajorTickMark.LineColor = Color.White;
            volumeArea.AxisY2.MinorTickMark.LineColor = Color.White;
            volumeArea.AxisX.LineColor = Color.White;
            volumeArea.AxisY2.LabelStyle.Format = "F0";
            volumeArea.AxisY2.LabelStyle.ForeColor = Color.White;
            volumeArea.AxisY2.LineColor = Color.White;
            volumeArea.AxisY2.MajorGrid.LineColor = Color.White;
            volumeArea.AxisY.IsInterlaced = false;
            _chartForCandle.ChartAreas.Add(volumeArea);

            Series volumeSeries = new Series("SeriesVolume") // создаём для нашей области коллекцию значений
            {
                ChartType = SeriesChartType.Column, // назначаем этой коллекции тип "столбцы"
                YAxisType = AxisType.Secondary, // назначаем ей правую линейку по шкале Y (просто для красоты)
                ChartArea = "ChartAreaVolume" // помещаем нашу коллекцию на ранее созданную область
            };

            _chartForCandle.Series.Add(volumeSeries);

            // общее
            foreach (ChartArea area in _chartForCandle.ChartAreas)
            {
                // Делаем курсор по Y красным и толстым
                area.CursorX.LineColor = Color.Red;
                area.CursorX.LineWidth = 2;
            }

            // подписываемся на события изменения масштабов
            _chartForCandle.AxisScrollBarClicked += chartForCandle_AxisScrollBarClicked; // событие передвижения курсора
            _chartForCandle.AxisViewChanged += chartForCandle_AxisViewChanged; // событие изменения масштаба
            _chartForCandle.CursorPositionChanged += chartForCandle_CursorPositionChanged;
            // событие выделения диаграммы
        }


        private void LoadCandleOnChartSlow() // прогрузить загруженные свечки на график по одной
        {

            if (_candleArray == null)
            {
                //если наш массив пуст по каким-то причинам
                return;
            }

            for (int i = 0; i < _candleArray.Length; i++)
            {
                // отправляем наш массив по свечкам на прорисовку
                Thread.Sleep(1);
                // спим 5ть миллисекунд между свечками, чтобы форма не висела и могла отвечать на запросы пользователя 
                LoadNewCandle(_candleArray[i], i);
            }

        }

        private void LoadNewCandle(Candle newCandle, int numberInArray) // добавить одну свечу на график
        {
            if (!CheckAccess())
            {
                // перезаходим в метод потоком формы, чтобы не было исключения
                Dispatcher.Invoke(new Action<Candle, int>(LoadNewCandle), newCandle, numberInArray);
                return;
            }
            // свечи
            Series candleSeries = _chartForCandle.Series.FindByName("SeriesCandle");

            if (candleSeries != null)
            {
                candleSeries.Points.RemoveAt(candleSeries.Points.Count - 1);
                // забиваем новую свечку
                candleSeries.Points.AddXY(numberInArray, newCandle.Low, newCandle.High, newCandle.Open, newCandle.Close);
                candleSeries.Points.Last().ToolTip =
                    $"Low: {newCandle.Low}\nHigh: {newCandle.High}\nOpen: {newCandle.Open}\nClose: {newCandle.Close}\nVolume: {newCandle.Volume}";
      
                // подписываем время
                candleSeries.Points[candleSeries.Points.Count - 1].AxisLabel =
                    newCandle.Time.ToString(CultureInfo.InvariantCulture);

                // разукрышиваем в привычные цвета
                if (newCandle.Close > newCandle.Open)
                {
                    candleSeries.Points[candleSeries.Points.Count - 1].Color = Color.Green;
                    candleSeries.Points[candleSeries.Points.Count - 1].BorderWidth = 3;
                }
                else
                {
                    candleSeries.Points[candleSeries.Points.Count - 1].Color = Color.Red;
                    candleSeries.Points[candleSeries.Points.Count - 1].BackSecondaryColor = Color.Red;
                    candleSeries.Points[candleSeries.Points.Count - 1].BorderWidth = 3;
                }

                ChartArea candleArea = _chartForCandle.ChartAreas.FindByName("ChartAreaCandle");
                if (candleArea != null && candleArea.AxisX.ScrollBar.IsVisible) // если уже выбран какой-то диапазон
                {
                    // сдвигаем представление вправо
                    candleArea.AxisX.ScaleView.Scroll(_chartForCandle.ChartAreas[0].AxisX.Maximum);
                }
            }
            // объём
            Series volumeSeries = _chartForCandle.Series.FindByName("SeriesVolume");

            if (volumeSeries != null)
            {
                volumeSeries.Points.AddXY(numberInArray, newCandle.Volume);
                // разукрышиваем в привычные цвета
                if (volumeSeries.Points.Count > 1)
                {
                    if (volumeSeries.Points[volumeSeries.Points.Count - 2].YValues[0] < newCandle.Volume)
                    {
                        volumeSeries.Points[volumeSeries.Points.Count - 1].Color = Color.Green;
                    }
                    else
                    {
                        volumeSeries.Points[volumeSeries.Points.Count - 1].Color = Color.Red;
                    }
                }
            }

            ChartResize(); // Выводим нормальные рамки
        }

        private void ChartResize() // устанавливает границы представления по оси У
        {
            // вообще-то можно это автоматике доверить, но там вечно косяки какие-то, поэтому лучше самому следить за всеми осями
            try
            {
                if (_candleArray == null)
                {
                    return;
                }
                // свечи
                Series candleSeries = _chartForCandle.Series.FindByName("SeriesCandle");
                ChartArea candleArea = _chartForCandle.ChartAreas.FindByName("ChartAreaCandle");

                if (candleArea == null ||
                    candleSeries == null)
                {
                    return;
                }

                int startPozition = 0; // первая отображаемая свеча
                int endPozition = candleSeries.Points.Count; // последняя отображаемая свеча

                
                if (_chartForCandle.ChartAreas[0].AxisX.ScrollBar.IsVisible)
                {
                    // если уже выбран какой-то диапазон, назначаем первую и последнюю исходя из этого диапазона

                    startPozition = Convert.ToInt32(candleArea.AxisX.ScaleView.Position);
                    endPozition = Convert.ToInt32(candleArea.AxisX.ScaleView.Position) +
                                    Convert.ToInt32(candleArea.AxisX.ScaleView.Size);
                }

                candleArea.AxisY2.Maximum = GetMaxValueOnChart(_candleArray, startPozition, endPozition);
                candleArea.AxisY2.Minimum = GetMinValueOnChart(_candleArray, startPozition, endPozition);

                // объёмы
                Series volumeSeries = _chartForCandle.Series.FindByName("SeriesVolume");
                ChartArea volumeArea = _chartForCandle.ChartAreas.FindByName("ChartAreaVolume");

                if (volumeSeries != null &&
                    volumeArea != null)
                {
                    volumeArea.AxisY2.Maximum = GetMaxVolume(_candleArray, startPozition, endPozition);
                    volumeArea.AxisY2.Minimum = 0;
                }

                _chartForCandle.Refresh();

            }
            catch (Exception error)
            {
                MessageBox.Show("Обибка при изменении ширины представления. Ошибка: " + error);
            }
        }

        private double GetMaxVolume(Candle[] book, int start, int end) // берёт максимальное значение объёма за период
        {
            double result = double.MinValue;

            for (int i = start; i < end && i < book.Length; i++)
            {
                if (book[i].Volume > result)
                {
                    result = book[i].Volume;
                }
            }

            return result;
        }

        private double GetMinValueOnChart(Candle[] book, int start, int end)
        // берёт минимальное значение из массива свечек
        {
            double result = double.MaxValue;

            for (int i = start; i < end && i < book.Length; i++)
            {
                if (book[i].Low < result)
                {
                    result = book[i].Low;
                }
            }

            return result;
        }

        private double GetMaxValueOnChart(Candle[] book, int start, int end)
        // берёт максимальное значение из массива свечек
        {
            double result = 0;

            for (int i = start; i < end && i < book.Length; i++)
            {
                if (book[i].High > result)
                {
                    result = book[i].High;
                }
            }

            return result;
        }

        private void FastStart()
        {
            //LoadCandleFromFile();
            LoadCandleOnChartFast();
        }

        private void LoadCandleOnChartFast() // формирует серии данных
        {
            // суть быстрой прогрузки в том, чтобы создать уже готовые серии данных и только потом
            // подгружать их на график. 
            Series candleSeries = new Series("SeriesCandle")
            {
                ChartType = SeriesChartType.Candlestick,// назначаем этой коллекции тип "Свечи"
                YAxisType = AxisType.Secondary,// назначаем ей правую линейку по шкале Y (просто для красоты)
                ChartArea = "ChartAreaCandle",// помещаем нашу коллекцию на ранее созданную область
                ShadowOffset = 2,  // наводим тень
                YValuesPerPoint = 4 // насильно устанавливаем число У точек для серии
            };

            for (int i = 0; i < _candleArray.Length; i++)
            {
                // забиваем новую свечку
                candleSeries.Points.AddXY(i, _candleArray[i].Low, _candleArray[i].High, _candleArray[i].Open,
                    _candleArray[i].Close);
                candleSeries.Points[i].ToolTip =
                    $"Low: {_candleArray[i].Low}\nHigh: {_candleArray[i].High}\nOpen: {_candleArray[i].Open}\nClose: {_candleArray[i].Close}\nVolume: {_candleArray[i].Volume}";

                // подписываем время
                candleSeries.Points[candleSeries.Points.Count - 1].AxisLabel =
                    _candleArray[i].Time.ToString(CultureInfo.InvariantCulture);

                // разукрышиваем в привычные цвета
                if (_candleArray[i].Close > _candleArray[i].Open)
                {
                    candleSeries.Points[candleSeries.Points.Count - 1].Color = Color.Green;
                    candleSeries.Points[candleSeries.Points.Count - 1].BorderWidth = 3;
                }
                else
                {
                    candleSeries.Points[candleSeries.Points.Count - 1].Color = Color.Red;
                    candleSeries.Points[candleSeries.Points.Count - 1].BorderWidth = 3;
                    candleSeries.Points[candleSeries.Points.Count - 1].BackSecondaryColor = Color.Red;
                }
            }

            Series volumeSeries = new Series("SeriesVolume")
            {
                ChartType = SeriesChartType.Column, // назначаем этой коллекции тип "Свечи"
                YAxisType = AxisType.Secondary,// назначаем ей правую линейку по шкале Y (просто для красоты) Везде ж так
                ChartArea = "ChartAreaVolume", // помещаем нашу коллекцию на ранее созданную область
                ShadowOffset = 2 // наводим тень
            };

            for (int i = 0; i < _candleArray.Length; i++)
            {
                volumeSeries.Points.AddXY(i, _candleArray[i].Volume);
                // разукрышиваем в привычные цвета
                if (volumeSeries.Points.Count > 1)
                {
                    if (volumeSeries.Points[volumeSeries.Points.Count - 2].YValues[0] < _candleArray[i].Volume)
                    {
                        volumeSeries.Points[volumeSeries.Points.Count - 1].Color = Color.Green;
                    }
                    else
                    {
                        volumeSeries.Points[volumeSeries.Points.Count - 1].Color = Color.Red;
                    }
                }
            }

            SetSeries(candleSeries, volumeSeries);
 
        }

        private void SetSeries(Series candleSeries, Series volumeSeries) // подгружает серии данных на график
        {
            if (!CheckAccess())
            {
                // перезаходим в метод потоком формы, чтобы не было исключения
                Dispatcher.Invoke(new Action<Series, Series>(SetSeries), candleSeries, volumeSeries);
                return;
            }

            _chartForCandle.Series.Clear(); // убираем с нашего графика все до этого созданные серии с данными
            _chartForCandle.Series.Add(candleSeries);
            _chartForCandle.Series.Add(volumeSeries);



            ChartArea candleArea = _chartForCandle.ChartAreas.FindByName("ChartAreaCandle");
            if (candleArea != null && candleArea.AxisX.ScrollBar.IsVisible)
            // если уже выбран какой-то диапазон
            {
                // сдвигаем представление вправо
                candleArea.AxisX.ScaleView.Scroll(_chartForCandle.ChartAreas[0].AxisX.Maximum);
            }
            ChartResize();
            _chartForCandle.Refresh();
        }

        // события
        private void chartForCandle_CursorPositionChanged(object sender, CursorEventArgs e)
        // событие изменение отображения диаграммы
        {
            ChartResize();
        }

        private void chartForCandle_AxisViewChanged(object sender, ViewEventArgs e)
        // событие изменение отображения диаграммы 
        {
            ChartResize();
        }

        private void chartForCandle_AxisScrollBarClicked(object sender, ScrollBarEventArgs e)
        // событие изменение отображения диаграммы
        {
            ChartResize();
        }
        
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (item.Content == null) return;
            switch (item.Content.ToString())
            {
                case "1 min":
                    {
                        _timeframe = 1;
                        break;
                    }
                case "5 min":
                    {
                        _timeframe = 5;
                        break;
                    }
                case "15 min":
                    {
                        _timeframe = 15;
                        break;
                    }
                case "60 min":
                    {
                        _timeframe = 60;
                        break;
                    }

            }

            CreateChart();
            var interval = new TimeSpan(0, _timeframe, 0);
            _candleArray = (from t in list_ticks
                         group t by DateTime.Parse(t.Tradetime).Ticks / interval.Ticks
                                 into g
                         select new Candle
                         {
                             High = (from t2 in g select t2.Price).Max(),
                             Low = (from t2 in g select t2.Price).Min(),
                             Open = g.First().Price,
                             Close = g.Last().Price,
                             Volume = g.Count(),
                             Time = g.First().Tradetime
                         }).ToArray();
            var worker = new Thread(FastStart) { IsBackground = true };
            worker.Start();
        }

    }

}
