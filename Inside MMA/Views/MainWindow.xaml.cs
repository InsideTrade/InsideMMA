using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using InsideDB;
using Inside_MMA.DataHandlers;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Inside_MMA.ViewModels;
using Microsoft.AspNet.SignalR.Client;
using SciChart.Core.Extensions;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _sleep;
        private bool _isCollapsed;

        private MainWindowViewModel _vm;

        public MainWindow(bool hideAdmin, HubConnection connection, IHubProxy hub, User user)
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            var vm = new MainWindowViewModel(DialogCoordinator.Instance, hub, connection, this)
            {
                HideAdmin = hideAdmin
            };
            MainWindowViewModel.IsAdmin = !hideAdmin;
            //if (licType == "Standard")
            //{
            //    vm.AllTradesProVisible = false;
            //    vm.AllTradesSimpleVisible = true;
            //}
            //else
            //{
            //    vm.AllTradesProVisible = true;
            //    vm.AllTradesSimpleVisible = false;
            //}
            DataContext = vm;
            _vm = vm;
            _sleep = user.Sleep;
            Closing += vm.OnWindowClosing;
            if (user.Role == "free")
                MainWindowViewModel.WindowAvailabilityManager.SetFreeVersion();
            else
                MainWindowViewModel.WindowAvailabilityManager.SelectWindows(InsideDB.Helpers.GetUserWindows(user));
        }


        private void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_sleep != "-")
                ((MainWindowViewModel) DataContext).Sleep();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void Broker_OnClick(object sender, RoutedEventArgs e)
        {
            BrokerFlyout.IsOpen = true;
        }

        private void CollapseExpandClick(object sender, RoutedEventArgs e)
        {
            var rect = CollapseExpandButton.FindChild<Rectangle>("Rectangle");
            var brush = rect.OpacityMask as VisualBrush;
            if (_isCollapsed)
            {
                ExpandWindows();
                brush.Visual = FindResource("appbar_arrow_collapsed") as Visual;
                _isCollapsed = false;
                CollapseExpandButton.ToolTip = "Collapse windows";
            }
            else
            {
                CollapseWindows();
                brush.Visual = FindResource("appbar_arrow_expand") as Visual;
                _isCollapsed = true;
                CollapseExpandButton.ToolTip = "Expand windows";
            }
        }

        private void CollapseWindows()
        {
            Application.Current.Windows.ForEachDo<Window>(w =>
            {
                if (w.GetType() != typeof(MainWindow)) w.WindowState = WindowState.Minimized;
            });
        }

        private void ExpandWindows()
        {
            Application.Current.Windows.ForEachDo<Window>(w =>
            {
                if (w.GetType() != typeof(MainWindow)) w.WindowState = WindowState.Normal;
            });
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsMenu.IsSubmenuOpen = true;
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/playlist?list=PL-QaGavytb4Ccs7_xnFW-pJk8E4w17ghw");
        }
    }
}
