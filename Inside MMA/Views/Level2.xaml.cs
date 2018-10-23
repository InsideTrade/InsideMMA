using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.Models;
using Inside_MMA.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Level2.xaml
    /// </summary>
    public partial class Level2 : INotifyPropertyChanged
    {
        private bool _rmbClicked;
        private int _alertSize;
        private int _alertTwoSize;
        private Level2ViewModel _context;
        private bool _firstBoot;
        private Visibility _visibility = Visibility.Visible;

        public Visibility GridVisibility
        {
            get
            {
                return _visibility;
            }

            set
            {
                _visibility = value;
                OnPropertyChanged();
            }
        }
        public Level2()
        {
            InitializeComponent();
            if (!MainWindowViewModel.WindowAvailabilityManager.SettingsEnabled)
                Grid.RowDefinitions[0].Height = new GridLength(0);
            Level2Buy.SelectedCells.Clear();
            DataContextChanged += OnDataContextChanged;
            HookToCvsSourceChanged();
            
        }
        public Level2(Level2Args args)
        {
            InitializeComponent();
            if (!MainWindowViewModel.WindowAvailabilityManager.SettingsEnabled)
                Grid.RowDefinitions[0].Height = new GridLength(0);
            Level2Buy.SelectedCells.Clear();
            DataContextChanged += OnDataContextChanged;
            if (args.Type == "usa")
            {
                _firstBoot = true;
                TypeSelector.SelectedIndex = 1;
            }
            _alertSize = args.AlertSize;
            _alertTwoSize = args.AlertTwoSize;
            HookToCvsSourceChanged();
        }
        //refresh collectionviewsource sorting after changing source data
        private void HookToCvsSourceChanged()
        {
            var cvs = (CollectionViewSource)FindResource("Source2");
            var dpd = DependencyPropertyDescriptor.FromProperty(CollectionViewSource.SourceProperty, typeof(CollectionViewSource));
            dpd?.AddValueChanged(cvs, delegate
            {
                using (cvs.DeferRefresh())
                {
                    cvs.SortDescriptions.Clear();
                    cvs.IsLiveSortingRequested = true;
                    cvs.SortDescriptions.Add(new SortDescription { Direction = ListSortDirection.Ascending, PropertyName = "Price" });
                }
            });
        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            _context = (Level2ViewModel) DataContext;
            _context.Dialog = DialogCoordinator.Instance;
            var vis = _context.IsUSA ? Visibility.Visible : Visibility.Collapsed;
            _context.IsUsaChanged += OnIsUsaChanged;
            if (_alertSize != 0)
                _context.AlertSize = _alertSize;
            if (_alertTwoSize != 0)
                _context.AlertTwoSize = _alertTwoSize;
            ColumnSource1.Visibility = vis;
            ColumnSource2.Visibility = vis;
        }


        private void OnIsUsaChanged(bool isUsa)
        {
            var vis = isUsa ? Visibility.Visible : Visibility.Collapsed;
            ColumnSource1.Visibility = vis;
            ColumnSource2.Visibility = vis;
        }
        ~Level2()
        {
            Debug.WriteLine("Level2 disposed");
        }

        private void SelectedRus(object sender, RoutedEventArgs e)
        {
            if (Level2Sell == null) return;
            Level2Sell.Visibility = Visibility.Collapsed;
            Level2Grid.ColumnDefinitions.RemoveAt(1);
            Grid.SetColumn(Level2Buy, 0);
            GridVisibility = Visibility.Visible;
            SizeToContent = SizeToContent.Manual;
            Width /= 1.5;
            SizeToContent = SizeToContent.Height;
            _context?.UpdateLevel2Args(Level2ArgsType.Type, "rus");
        }

        private void SelectedUsa(object sender, RoutedEventArgs e)
        {
            Level2Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(Level2Buy, 0);
            Grid.SetColumn(Level2Sell, 1);
            Level2Sell.Visibility = Visibility.Visible;
            GridVisibility = Visibility.Collapsed;
            if (!_firstBoot)
            {
                //I have no idea why it has to be like that
                SizeToContent = SizeToContent.Manual;
                Width *= 1.5;
                SizeToContent = SizeToContent.Height;
            }
            _context?.UpdateLevel2Args(Level2ArgsType.Type, "usa");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Expander_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Expander.IsExpanded = false;
        }

        private void Level2_OnDeactivated(object sender, EventArgs e)
        {
            Level2Buy.UnselectAll();
            Level2Sell.UnselectAll();
        }


        private void ViewHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://youtu.be/28ZlgzR_s08");
        }

        //private void Row_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var row = sender as DataGridRow;
        //    row?.InputBindings.Add(new MouseBinding(_context.PlaceMktOrder,
        //        new MouseGesture { MouseAction = MouseAction.LeftClick }));
        //}

        private void RightMouseOnRow(object sender, MouseButtonEventArgs e)
        {
            ((DataGridRow)sender).IsSelected = true;
        }

        private void MouseEnterRow(object sender, MouseEventArgs e)
        {
            var dgr = ((DataGridRow)sender);
            //dgr.IsSelected = true;
            dgr.Focus();
        }

        private void MouseLeaveRow(object sender, MouseEventArgs e)
        {
            if (_rmbClicked)
                _rmbClicked = false;
            else
                ((DataGridRow)sender).IsSelected = false;
        }

        private void RowClicked(object sender, MouseButtonEventArgs e)
        {
            ((DataGridRow) sender).IsSelected = true;
            if (OneClickTrade.IsChecked == false) return;
            if (Keyboard.Modifiers == ModifierKeys.None)
                _context.LeftClickMktOrder();
            if (Keyboard.Modifiers == ModifierKeys.Control)
                _context.CtrlLeftClickLimitOrder();
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                _context.ShiftLeftClickStopOrder();
            if (Keyboard.Modifiers == ModifierKeys.Alt)
                _context.SetFastOrderManualStopPrice();
        }

        private void Level2_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Activate();
            Grid.Focus();
        }
    }
}
