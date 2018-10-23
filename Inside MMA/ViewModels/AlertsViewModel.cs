using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;
using Inside_MMA.Models.Alerts;
using Inside_MMA.Properties;
using Inside_MMA.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    //public class BoardCellTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate SelectBoardTemplate
    //    { get; set; }
    //    public DataTemplate BoardTemplate
    //    { get; set; }
    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {
    //        var obj = item as Alert;

    //        if (obj != null)
    //        {
    //            return obj.IsEdited ? SelectBoardTemplate : BoardTemplate;
    //        }
    //        return base.SelectTemplate(item, container);
    //    }
    //}
    //public class SeccodeCellTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate SelectSeccodeTemplate
    //    { get; set; }
    //    public DataTemplate SeccodeTemplate
    //    { get; set; }
    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {
    //        var obj = item as Alert;

    //        if (obj != null)
    //        {
    //            return obj.IsEdited ? SelectSeccodeTemplate : SeccodeTemplate;
    //        }
    //        return base.SelectTemplate(item, container);
    //    }
    //}
    //public class ButtonTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate SaveTemplate
    //    { get; set; }
    //    public DataTemplate EditTemplate
    //    { get; set; }
    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {
    //        var obj = item as Alert;

    //        if (obj != null)
    //        {
    //            return obj.IsEdited ? SaveTemplate : EditTemplate;
    //        }
    //        return base.SelectTemplate(item, container);
    //    }
    //}
    public class AlertsViewModel : INotifyPropertyChanged
    {
        private static XmlSerializer _xml = new XmlSerializer(typeof(ObservableCollection<BaseAlert>));
        //private MainWindowViewModel MainWindowVm => (MainWindowViewModel) Application.Current.MainWindow
        //    .DataContext;
        private ObservableCollection<BaseAlert> _alertsCollection = new ObservableCollection<BaseAlert>();
        public ObservableCollection<BaseAlert> AlertsCollection
        {
            get { return _alertsCollection; }
            set
            {
                if (Equals(value, _alertsCollection)) return;
                _alertsCollection = value;
                OnPropertyChanged();
            }
        }
        //public List<string> Boards => MainWindowViewModel.SecVm.SecList
        //    .Select(x => x.Board).Distinct().ToList();
        public Window SelectionWindow;

        private BaseAlert _selectedAlert;

        public BaseAlert SelectedAlert
        {
            get { return _selectedAlert; }
            set
            {
                if (Equals(value, _selectedAlert)) return;
                _selectedAlert = value;
                OnPropertyChanged();
            }
        }

        public ICommand Add { get; set; }
        //public ICommand Save { get; set; }
        public ICommand Edit { get; set; }
        public ICommand Delete { get; set; }
        public AlertsViewModel()
        {
            Add = new Command(arg => AddAlert());
            Edit = new Command(arg => EditAlert());
            //Save = new Command(SaveAlert);
            Delete = new Command(DeleteAlert);
            GetAlerts();
            AlertsCollection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                    SaveAlerts();
            };
        }

        private void EditAlert()
        {
            SelectedAlert.Uninitialize();
            new AlertSelection
            {
                Owner = SelectionWindow,
                DataContext = new AlertSelectionViewModel(editMode: true) { Alert = SelectedAlert, SelectedType = _selectedAlert.Type}
            }.ShowDialog();
            SelectedAlert.Initialize();
            SaveAlerts();
        }

        private void DeleteAlert(object obj)
        {
            var alert = (BaseAlert)obj;
            alert.Uninitialize();
            AlertsCollection.Remove(alert);
        }

        //private void SaveAlert(object obj)
        //{
        //    var alert = (Alert) obj;
        //    var index = AlertsCollection.IndexOf(alert);
        //    alert.IsEdited = !alert.IsEdited;
        //    AlertsCollection.RemoveAt(index);
        //    AlertsCollection.Insert(index, alert);
        //    if (!alert.IsEdited && !alert.Initialized)
        //        alert.Initialize();
        //}


        public void AddAlert(string board = null, string seccode = null, Window caller = null)
        {
            //AlertsCollection.Add(new Alert());
            var vm = new AlertSelectionViewModel
            {
                Alert = new BaseAlert
                {
                    Board = board,
                    Seccode = seccode,
                    Name = seccode
                }
            };
            new AlertSelection {Owner = SelectionWindow, DataContext = vm}.ShowDialog();
            if (vm.Alert != null && vm.Confirmed)
            {
                AlertsCollection.Add(vm.Alert);
                vm.Alert.Active = true;
            }
        }

        public void SaveAlerts()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/alerts";
            using (
                var file =
                    File.Open(path, FileMode.OpenOrCreate))
            {
                file.SetLength(0);
                _xml.Serialize(file, AlertsCollection);
                file.Close();
            }
        }

        private void GetAlerts()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/Inside MMA/settings/alerts";
            try
            {
                using (
                    var file =
                        File.Open(path, FileMode.Open))
                {
                    AlertsCollection = (ObservableCollection<BaseAlert>) _xml.Deserialize(file);
                    file.Close();
                    UninitializeAll();
                }
            }
            catch(Exception e)
            {
                
            }
        }
        public void InitializeAllActive()
        {
            AlertsCollection.Where(a => a.Active).ForEachDo(a => a.Initialize());
        }
        public void UninitializeAll()
        {
            AlertsCollection.ForEachDo(a => a.Uninitialize());
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}