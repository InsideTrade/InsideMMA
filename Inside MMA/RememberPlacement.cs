using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Inside_MMA.Annotations;
using Inside_MMA.DataHandlers;
using Inside_MMA.Models;

namespace Inside_MMA
{
    public class RememberPlacement : INotifyPropertyChanged
    {
        public Window Window;
        public int Id;
        private string _seccode;
        protected string _board;

        public string Board
        {
            get { return _board; }
            set
            {
                if (value == _board) return;
                _board = value;
                OnPropertyChanged();
            }
        }

        public string Seccode
        {
            get { return _seccode; }
            set
            {
                if (value == _seccode) return;
                _seccode = value;
                OnPropertyChanged();
            }
        }

        public void SubscribeToWindowEvents()
        {
            Window.MouseLeave += UpdateWindowPosition;
            Window.SourceInitialized += RestoreWindowPosition;
        }

        public void UnsubscribeFromWindowEvents()
        {
            Window.MouseLeave -= UpdateWindowPosition;
            Window.SourceInitialized -= RestoreWindowPosition;
        }

        public void RestoreWindowPosition(object sender, EventArgs eventArgs)
        {
            Window.SetPlacement(WindowDataHandler.GetSavedPlacement(Id));
        }

        public void UpdateWindowPosition(object sender, MouseEventArgs mouseEventArgs)
        {
            WindowDataHandler.UpdateWindowPlacement(Id, Window.GetPlacement());
        }

        public void UpdateWindowInstrument()
        {
            WindowDataHandler.UpdateWindowInstrument(Id, Board, Seccode);
        }

        public void UpdateWindowBinding(bool isAnchored)
        {
            WindowDataHandler.UpdateWindowBinding(Id, isAnchored);
        }
        public void UpdateLevel2Args(Level2ArgsType type, dynamic arg)
        {
            Level2Args args = GetWindowArgs() ?? new Level2Args();
            switch (type)
            {
                case Level2ArgsType.Type:
                    args.Type = arg;
                    break;
                case Level2ArgsType.AlertSize:
                    args.AlertSize = arg;
                    break;
                case Level2ArgsType.AlertTwoSize:
                    args.AlertTwoSize = arg;
                    break;
            }
            WindowDataHandler.UpdateWindowArgs(Id, args);
        }
        public void UpdateWindowArgs(dynamic args)
        {
            WindowDataHandler.UpdateWindowArgs(Id, args);
        }
        public dynamic GetWindowArgs()
        {
            return WindowDataHandler.GetWindowArgs(Id);
        }

        public void SaveWindow()
        {
            var windowType = Window.GetType().ToString();
            Id = WindowDataHandler.SaveWindow(windowType, GetType().ToString(), Board, Seccode);
        }

        public void CloseWindow()
        {
            WindowDataHandler.CloseWindow(Id);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}