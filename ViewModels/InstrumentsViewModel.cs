using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class InstrumentsViewModel : AViewModel
    {
        private ObservableCollection<Instrument> _instruments;
        private ObservableCollection<Client> _clients;
        private Instrument _selectedInstrument;
        private Instrument _editingInstrument;
        private string _searchText;

        public ObservableCollection<Instrument> Instruments
        {
            get => _instruments;
            set => SetField(ref _instruments, value);
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetField(ref _clients, value);
        }

        public Instrument SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                SetField(ref _selectedInstrument, value);
                if (value != null)
                {
                    EditingInstrument = new Instrument
                    {
                        InstrumentID = value.InstrumentID,
                        ClientID = value.ClientID,
                        Type = value.Type,
                        Manufacturer = value.Manufacturer,
                        Model = value.Model,
                        SerialNumber = value.SerialNumber,
                        Year = value.Year,
                        Color = value.Color,
                        SpecialFeatures = value.SpecialFeatures
                    };
                }
            }
        }

        public Instrument EditingInstrument
        {
            get => _editingInstrument;
            set => SetField(ref _editingInstrument, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterInstruments();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public InstrumentsViewModel()
        {
            Instruments = new ObservableCollection<Instrument>();
            Clients = new ObservableCollection<Client>();
            EditingInstrument = new Instrument();
            
            AddCommand = new RelayCommand(_ => AddInstrument());
            SaveCommand = new RelayCommand(_ => SaveInstrument());
            DeleteCommand = new RelayCommand(_ => DeleteInstrument());
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                
                // Загрузка клиентов
                Clients.Clear();
                var clients = _context.Clients
                    .OrderBy(c => c.LastName)
                    .ToList();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                // Загрузка инструментов
                Instruments.Clear();
                var instruments = _context.Instruments
                    .Include(i => i.Client)
                    .OrderBy(i => i.Manufacturer)
                    .ThenBy(i => i.Model)
                    .ToList();
                
                foreach (var instrument in instruments)
                {
                    Instruments.Add(instrument);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddInstrument()
        {
            EditingInstrument = new Instrument
            {
                RegistrationDate = DateTime.Now,
                ClientID = Clients.FirstOrDefault()?.ClientID ?? 0
            };
        }

        private void SaveInstrument()
        {
            if (EditingInstrument.ClientID == 0)
            {
                ShowError("Выберите клиента");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingInstrument.Type))
            {
                ShowError("Укажите тип инструмента");
                return;
            }

            if (EditingInstrument.Year.HasValue && 
                (EditingInstrument.Year < 1900 || EditingInstrument.Year > DateTime.Now.Year + 1))
            {
                ShowError($"Год должен быть между 1900 и {DateTime.Now.Year + 1}");
                return;
            }

            try
            {
                if (EditingInstrument.InstrumentID == 0)
                {
                    _context.Instruments.Add(EditingInstrument);
                    Instruments.Add(EditingInstrument);
                    ShowInfo("Инструмент добавлен");
                }
                else
                {
                    var existing = _context.Instruments.Find(EditingInstrument.InstrumentID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingInstrument);
                        var index = Instruments.IndexOf(SelectedInstrument);
                        if (index >= 0)
                        {
                            Instruments[index] = existing;
                        }
                        ShowInfo("Инструмент обновлен");
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

        private void DeleteInstrument()
        {
            if (SelectedInstrument == null)
            {
                ShowError("Выберите инструмент для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить инструмент {SelectedInstrument.FullDescription}?"))
                return;

            try
            {
                _context.Instruments.Remove(SelectedInstrument);
                Instruments.Remove(SelectedInstrument);
                _context.SaveChanges();
                ShowInfo("Инструмент удален");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterInstruments()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Instruments.Where(i =>
                i.Type.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.Manufacturer.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (i.SerialNumber != null && i.SerialNumber.Contains(SearchText)) ||
                i.Client.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Instruments.Clear();
            foreach (var instrument in filtered)
            {
                Instruments.Add(instrument);
            }
        }
    }
}