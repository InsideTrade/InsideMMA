using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Microsoft.Win32;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Core;
using SciChart.Core.Extensions;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Events;
using SciChart.Core.Utility.Mouse;
using SciChart.Data.Model;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для SciChartWindow.xaml
    /// </summary>
    public partial class SciChartWindow
    {
        private bool _resized;
        private double _currentYValue;
        public SciChartWindow()
        {
            InitializeComponent();
        }
        ~SciChartWindow()
        {
            Debug.WriteLine("SciChart disposed");
        }
        private void StockChart_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AddTrendline.IsChecked == false)
                return;
            var yCalc = StockChart.YAxis.GetCurrentCoordinateCalculator();
            var mousePoint = e.GetPosition(StockChart.ModifierSurface as UIElement);
            var yDataValue = yCalc.GetDataValue(mousePoint.Y);
            var annotation = new HorizontalLineAnnotation
            {
                Stroke = new SolidColorBrush(Colors.Orange),
                StrokeThickness = 1,
                Y1 = yDataValue,
                IsEditable = true
            };
            //annotation.StrokeDashArray = new DoubleCollection { 10, 5 };
            annotation.AnnotationLabels.Add(new AnnotationLabel { LabelPlacement = LabelPlacement.Axis, Foreground = Brushes.White });
            StockChart.Annotations.Add(annotation);

            var popup = new Popup();
            popup.PopupAnimation = PopupAnimation.Fade;
            popup.Placement = PlacementMode.Center;
        }

        private void RemoveTrendlines(object sender, RoutedEventArgs e)
        {
            var annotations = StockChart.Annotations;
            annotations.RemoveWhere(item => item.GetType() == typeof(HorizontalLineAnnotation) && ((HorizontalLineAnnotation)item).Name == "");
            StockChart.Annotations = annotations;
        }

        private void TakeScreenshot(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Title = "Save screenshot", Filter = "Image (*.jpg)|*.jpg", AddExtension = true, CheckPathExists = true };
            if (dialog.ShowDialog() == true)
                StockChart.ExportToFile(dialog.FileName, ExportType.Jpeg, false);
        }

        private void StockChart_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var yCalc = StockChart.YAxis.GetCurrentCoordinateCalculator();
            var mousePoint = e.GetPosition(StockChart.ModifierSurface as UIElement);
            ((SciChartViewModel)DataContext).CurrentPrice = yCalc.GetDataValue(mousePoint.Y).RoundOff(2, MidpointRounding.AwayFromZero);
        }

        private void VisibleRangeChanged(object sender, VisibleRangeChangedEventArgs e)
        {
            var range = ColumnXAxis.ToDateRange((IndexRange) e.NewVisibleRange);
            BubbleXAxis.VisibleRange = range;
        }
        

        //private void TrendlinesToggleClick(object sender, RoutedEventArgs e)
        //{
        //    var toggle = (ToggleButton) sender;
        //    var annotations = StockChart.Annotations;
        //    if (toggle.IsChecked == true)
        //        annotations.Where(x => x.GetType() == typeof(HorizontalLineAnnotation) &&
        //                               ((HorizontalLineAnnotation) x).Name == "history").ForEachDo(annotation => annotation.Show());
        //    else
        //        annotations.Where(x => x.GetType() == typeof(HorizontalLineAnnotation) &&
        //                               ((HorizontalLineAnnotation)x).Name == "history").ForEachDo(annotation => annotation.Hide());
            
        //}

        private void ShinkEnlargeClick(object sender, RoutedEventArgs e)
        {
            var rect = ShrinkEnlargeButton.FindChild<Rectangle>("Rectangle");
            var brush = (VisualBrush)rect.OpacityMask;
            if (_resized)
            {
                Height *= 2;
                Width *= 2;
                brush.Visual = FindResource("appbar_magnify_arrow_down") as Visual;
                _resized = false;
            }
            else
            {
                Height /= 2;
                Width /= 2;
                brush.Visual = FindResource("appbar_magnify_arrow_up") as Visual;
                _resized = true;
            }
        }
    }
}
