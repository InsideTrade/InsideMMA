using System.Windows;
using Inside_MMA.ViewModels;

namespace Inside_MMA.Views
{
    /// <summary>
    /// Логика взаимодействия для Level2Choose.xaml
    /// </summary>
    public partial class SecurityChoose
    {
        public SecurityChooseViewModel SecurityChooseViewModel = new SecurityChooseViewModel();
        public SecurityChoose()
        {
            InitializeComponent();
            DataContext = SecurityChooseViewModel;
            SecurityChooseViewModel.InitCloseAction(Close);
        }
    }
}
