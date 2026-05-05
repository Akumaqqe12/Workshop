using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class ClientsViewModel : AViewModel
    {
        private ObservableCollection<Client> _clients;
        private Client _selectedClient;
        private Client _editingClient;
        private string _searchText;

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetField(ref _clients, value);
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                SetField(ref _selectedClient, value);
                if (value != null)
                {
                    EditingClient = new Client
                    {
                        ClientID = value.ClientID,
                        LastName = value.LastName,
                        FirstName = value.FirstName,
                        MiddleName = value.MiddleName,
                        Phone = value.Phone,
                        Email = value.Email,
                        RegistrationDate = value.RegistrationDate,
                        Notes = value.Notes
                    };
                }
            }
        }

        public Client EditingClient
        {
            get => _editingClient;
            set => SetField(ref _editingClient, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterClients();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public ClientsViewModel()
        {
            Clients = new ObservableCollection<Client>();
            EditingClient = new Client();
            
            AddCommand = new RelayCommand(_ => AddClient());
            SaveCommand = new RelayCommand(_ => SaveClient());
            DeleteCommand = new RelayCommand(_ => DeleteClient());
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                Clients.Clear();
                
                var clients = _context.Clients
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();
                
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки клиентов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddClient()
        {
            EditingClient = new Client
            {
                RegistrationDate = DateTime.Now
            };
        }

        private void SaveClient()
        {
            var errors = ValidationService.Validate(EditingClient);
            if (errors.Any())
            {
                ShowError(string.Join("\n", errors));
                return;
            }

            if (!ValidationService.ValidatePhone(EditingClient.Phone))
            {
                ShowError("Некорректный формат телефона");
                return;
            }

            if (!ValidationService.ValidateEmail(EditingClient.Email))
            {
                ShowError("Некорректный формат email");
                return;
            }

            try
            {
                if (EditingClient.ClientID == 0)
                {
                    _context.Clients.Add(EditingClient);
                    Clients.Add(EditingClient);
                    ShowInfo("Клиент добавлен");
                }
                else
                {
                    var existing = _context.Clients.Find(EditingClient.ClientID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingClient);
                        var index = Clients.IndexOf(SelectedClient);
                        if (index >= 0)
                        {
                            Clients[index] = existing;
                        }
                        ShowInfo("Клиент обновлен");
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

        private void DeleteClient()
        {
            if (SelectedClient == null)
            {
                ShowError("Выберите клиента для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить клиента {SelectedClient.FullName}?"))
                return;

            try
            {
                _context.Clients.Remove(SelectedClient);
                Clients.Remove(SelectedClient);
                _context.SaveChanges();
                ShowInfo("Клиент удален");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterClients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Clients.Where(c =>
                c.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(SearchText) ||
                (c.Email != null && c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Clients.Clear();
            foreach (var client in filtered)
            {
                Clients.Add(client);
            }
        }
    }
}