using System.Windows;
using System.Windows.Controls;
using MusicRepairShop.ViewModels;

namespace MusicRepairShop.Views
{
    public partial class MastersView : UserControl
    {
        public MastersView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MastersViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.EditingMaster.PasswordHash = passwordBox.Password;
            }
        }
    }
}