using System;
using System.Windows;
using System.Windows.Input;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class MainViewModel : AViewModel
    {
        private AViewModel _currentViewModel;
        private string _windowTitle;

        public AViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetField(ref _windowTitle, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            WindowTitle = "Мастерская по ремонту музыкальных инструментов";
            
            NavigateCommand = new RelayCommand(Navigate);
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Начальная страница - клиенты
            CurrentViewModel = new ClientsViewModel();
        }

        private void Navigate(object parameter)
        {
            var viewName = parameter as string;
            
            switch (viewName)
            {
                case "Clients":
                    CurrentViewModel = new ClientsViewModel();
                    break;
                case "Instruments":
                    CurrentViewModel = new InstrumentsViewModel();
                    break;
                case "Services":
                    CurrentViewModel = new ServicesViewModel();
                    break;
                case "Materials":
                    CurrentViewModel = new MaterialsViewModel();
                    break;
                case "Masters":
                    CurrentViewModel = new MastersViewModel();
                    break;
                case "Orders":
                    CurrentViewModel = new OrdersViewModel();
                    break;
                case "Reports":
                    CurrentViewModel = new ReportsViewModel();
                    break;
            }
        }
    }
}