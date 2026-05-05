using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class ServicesViewModel : AViewModel
    {
        private ObservableCollection<Service> _services;
        private Service _selectedService;
        private Service _editingService;
        private string _searchText;

        public ObservableCollection<Service> Services
        {
            get => _services;
            set => SetField(ref _services, value);
        }

        public Service SelectedService
        {
            get => _selectedService;
            set
            {
                SetField(ref _selectedService, value);
                if (value != null)
                {
                    EditingService = new Service
                    {
                        ServiceID = value.ServiceID,
                        ServiceName = value.ServiceName,
                        Category = value.Category,
                        Description = value.Description,
                        Price = value.Price,
                        StandardTime = value.StandardTime,
                        IsActive = value.IsActive
                    };
                }
            }
        }

        public Service EditingService
        {
            get => _editingService;
            set => SetField(ref _editingService, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterServices();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public ServicesViewModel()
        {
            Services = new ObservableCollection<Service>();
            EditingService = new Service();
            
            AddCommand = new RelayCommand(_ => AddService());
            SaveCommand = new RelayCommand(_ => SaveService());
            DeleteCommand = new RelayCommand(_ => DeleteService());
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                Services.Clear();
                
                var services = _context.Services
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.ServiceName)
                    .ToList();
                
                foreach (var service in services)
                {
                    Services.Add(service);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки услуг: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddService()
        {
            EditingService = new Service
            {
                IsActive = true
            };
        }

        private void SaveService()
        {
            if (string.IsNullOrWhiteSpace(EditingService.ServiceName))
            {
                ShowError("Укажите название услуги");
                return;
            }

            if (EditingService.Price <= 0)
            {
                ShowError("Цена должна быть больше 0");
                return;
            }

            if (EditingService.StandardTime.HasValue && EditingService.StandardTime < 0)
            {
                ShowError("Время не может быть отрицательным");
                return;
            }

            try
            {
                if (EditingService.ServiceID == 0)
                {
                    _context.Services.Add(EditingService);
                    Services.Add(EditingService);
                    ShowInfo("Услуга добавлена");
                }
                else
                {
                    var existing = _context.Services.Find(EditingService.ServiceID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingService);
                        var index = Services.IndexOf(SelectedService);
                        if (index >= 0)
                        {
                            Services[index] = existing;
                        }
                        ShowInfo("Услуга обновлена");
                    }
                }

                _context.SaveChanges();
                LoadData();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteService()
        {
            if (SelectedService == null)
            {
                ShowError("Выберите услугу для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить услугу \"{SelectedService.ServiceName}\"?"))
                return;

            try
            {
                _context.Services.Remove(SelectedService);
                Services.Remove(SelectedService);
                _context.SaveChanges();
                ShowInfo("Услуга удалена");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterServices()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Services.Where(s =>
                s.ServiceName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.Category != null && s.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.Description != null && s.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Services.Clear();
            foreach (var service in filtered)
            {
                Services.Add(service);
            }
        }
    }
}