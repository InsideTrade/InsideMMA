using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MVVM_Solution.Annotations;
using MVVM_Solution.Models;
using MVVM_Solution.ViewModels;
using Visifire.Charts;

namespace MVVM_Solution.Views
{
    /// <summary>
    /// Логика взаимодействия для VisiFireChart.xaml
    /// </summary>
    public partial class VisiFireChart : Window
    {
        public VisiFireChartViewModel Vm { get; set; }= new VisiFireChartViewModel();
        public VisiFireChart()
        {
            InitializeComponent();
            DataContext = Vm;
            Closing += Vm.OnWindowClosing;
            Chart.PlotArea.MouseLeftButtonDown += PlotArea_MouseLeftButtonDown;
        }

        private void PlotArea_MouseLeftButtonDown(object sender, PlotAreaMouseButtonEventArgs e)
        {
            Chart.TrendLines.Add(new TrendLine
            {
                Orientation = Orientation.Horizontal,
                Value = e.GetYValue(Chart.AxesY[1]),
                LineColor = Brushes.DeepSkyBlue,
                LabelText = e.GetYValue(Chart.AxesY[1]).ToString("F2"),
                LabelFontColor = Brushes.DeepSkyBlue,
                AxisType = AxisTypes.Secondary
            });
            Chart.TrendLines.Last().MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Chart.TrendLines.Remove((TrendLine)sender);
        }

    }
}
