using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Inside_MMA.Annotations;

namespace Inside_MMA.ViewModels
{


    public class SecurityChooseViewModel : INotifyPropertyChanged, ICloseable
    {
        public Action CloseAction { get; set; }
        public bool ResultOk;
        private string _board = "TQBR";
        public string Board
        {
            get
            {
                return _board;
            }

            set
            {
                _board = value;
                OnPropertyChanged();
            }
        }
        private string _seccode = "GAZP";
        public string Seccode
        {
            get
            {
                return _seccode;
            }

            set
            {
                _seccode = value;
                OnPropertyChanged();
            }
        }
        private string _seccodeSecond = "SBER";
        public string SeccodeSecond
        {
            get => _seccodeSecond;
            set
            {
                _seccodeSecond = value;
                OnPropertyChanged();
            }
        }
        public string Window { get; set; }
        public ICommand OkCommand { get; set; }

        public SecurityChooseViewModel()
        {
            OkCommand = new Command(arg => Ok());
        }

        private void Ok()
        {
            ResultOk = true;
            CloseAction();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
