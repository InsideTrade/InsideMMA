using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Inside_MMA.Annotations;

namespace Inside_MMA.ViewModels
{
    public delegate void AllTradesChooseHandler(string board, string seccode);
    //I am sure this is unused but I'll leave it here for the time being
    public class AllTradesChooseViewModel : INotifyPropertyChanged
    {
        public Action CloseAction { get; set; }
        public event AllTradesChooseHandler AllTradesChooseHandlerEvent;

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
        private string _board = "TQBR";

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
        public ICommand OkCommand { get; set; }

        //todo
        public AllTradesChooseViewModel()
        {
            OkCommand = new Command(arg => Ok());
        }
        
        private void Ok()
        {
            CloseAction();
            OnAllTradesChooseHandlerEvent(Board, Seccode);
        }

        protected virtual void OnAllTradesChooseHandlerEvent(string board, string seccode)
        {
            AllTradesChooseHandlerEvent?.Invoke(board, seccode);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
