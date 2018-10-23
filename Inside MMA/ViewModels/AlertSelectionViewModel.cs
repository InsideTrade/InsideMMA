using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.Models.Alerts;
using SciChart.Core.Extensions;

namespace Inside_MMA.ViewModels
{
    public class AlertSelectionViewModel : INotifyPropertyChanged
    {
        private List<string> _seccodes;
        private string _board;
        private string _selectedType;
        private BaseAlert _alert;
        private bool _enableTypeSelection = true;
        private bool _editMode;
        public bool EditMode
        {
            get { return _editMode; }
            set
            {
                if (value == _editMode) return;
                _editMode = value;
                OnPropertyChanged();
            }
        }
        public bool Confirmed;
        private double _lastRowHeight = 500;

        public List<string> Boards => MainWindowViewModel.SecVm.SecList
            .Select(x => x.Board).Distinct().ToList();
        public List<string> Seccodes
        {
            get { return _seccodes; }
            set
            {
                _seccodes = value;
                OnPropertyChanged();
            }
        }
        public List<string> Types => new List<string> {"Delta OI >=", "Eaten size >=", "Trade size =", "Trade size >", "Trade price >=", "Trade price <=", "True"};
        public string Name { get; set; }
        public string Board
        {
            get { return _board; }
            set
            {
                _board = value;
                Seccodes = MainWindowViewModel.SecVm.SecList
                    .Where(i => i.Board == _board)
                    .Select(x => x.Seccode).Distinct().ToList();
                OnPropertyChanged();
            }
        }
        public string Seccode { get; set; }
        public double Size { get; set; }
        public bool EnableTypeSelection
        {
            get { return _enableTypeSelection; }
            set
            {
                if (value == _enableTypeSelection) return;
                _enableTypeSelection = value;
                OnPropertyChanged();
            }
        }

        public double LastRowHeight
        {
            get { return _lastRowHeight; }
            set
            {
                if (value.Equals(_lastRowHeight)) return;
                _lastRowHeight = value;
                OnPropertyChanged();
            }
        }

        public BaseAlert Alert
        {
            get { return _alert; }
            set
            {
                if (Equals(value, _alert)) return;
                _alert = value;
                OnPropertyChanged();
            }
        }

        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (value == _selectedType) return;
                _selectedType = value;
                if (EditMode) return;
                switch (_selectedType)
                {
                    case "Delta OI >=":
                        Alert = new GreaterThanDeltaOIAlert
                        {
                            Board = _alert.Board,
                            Seccode = _alert.Seccode,
                            Name = _alert.Name,
                            Type = "Delta OI >="
                        };
                        Board = "FUT";
                        break;
                    case "Eaten size >=":
                        Alert = new GreaterThanEatenSize
                        {
                            Board = _alert.Board,
                            Seccode = _alert.Seccode,
                            Name = _alert.Name,
                            Type = "Eaten size >="
                        };
                        break;
                    case "Trade size =":
                        Alert = new EqualsSizeAlert
                        {
                            Board = _alert.Board,
                            Seccode = _alert.Seccode,
                            Name = _alert.Name,
                            Type = "Trade size ="
                        };
                        break;
                    case "Trade size >":
                        Alert = new GreaterThanSizeAlert
                        {
                            Board = _alert.Board,
                            Seccode = _alert.Seccode,
                            Name = _alert.Name,
                            Type = "Trade size >"
                        };
                        break;
                    case "Trade price >=":
                        Alert = new GreaterThanPriceAlert(_alert.Board, _alert.Seccode)
                        {
                            Name = _alert.Name,
                            Type = "Trade price >="
                        };
                        break;
                    case "Trade price <=":
                        Alert = new SmallerThanPriceAlert(_alert.Board, _alert.Seccode)
                        {
                            Name = _alert.Name,
                            Type = "Trade price <="
                        };
                        break;
                    case "True":
                        Alert = new TrueAlert(_alert.Board, _alert.Seccode, _alert.Name) {Type = "True"};
                        break;
                }
                OnPropertyChanged();
            }
        }

        public ICommand Ok { get; set; }
        

        public AlertSelectionViewModel(bool editMode = false)
        {
            Ok = new Command(arg => Confirmed = true);
            if (editMode)
            {
                EditMode = true;
                EnableTypeSelection = false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}