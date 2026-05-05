using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class MastersViewModel : AViewModel
    {
        private ObservableCollection<Master> _masters;
        private Master _selectedMaster;
        private Master _editingMaster;
        private string _searchText;

        public ObservableCollection<Master> Masters
        {
            get => _masters;
            set => SetField(ref _masters, value);
        }

        public Master SelectedMaster
        {
            get => _selectedMaster;
            set
            {
                SetField(ref _selectedMaster, value);
                if (value != null)
                {
                    EditingMaster = new Master
                    {
                        MasterID = value.MasterID,
                        LastName = value.LastName,
                        FirstName = value.FirstName,
                        MiddleName = value.MiddleName,
                        Phone = value.Phone,
                        Specialization = value.Specialization,
                        HireDate = value.HireDate,
                        SalaryRate = value.SalaryRate,
                        CommissionRate = value.CommissionRate,
                        Username = value.Username,
                        PasswordHash = value.PasswordHash,
                        IsActive = value.IsActive
                    };
                }
            }
        }

        public Master EditingMaster
        {
            get => _editingMaster;
            set => SetField(ref _editingMaster, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterMasters();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public MastersViewModel()
        {
            Masters = new ObservableCollection<Master>();
            EditingMaster = new Master();
            
            AddCommand = new RelayCommand(_ => AddMaster());
            SaveCommand = new RelayCommand(_ => SaveMaster());
            DeleteCommand = new RelayCommand(_ => DeleteMaster());
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                Masters.Clear();
                
                var masters = _context.Masters
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToList();
                
                foreach (var master in masters)
                {
                    Masters.Add(master);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки мастеров: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddMaster()
        {
            EditingMaster = new Master
            {
                HireDate = DateTime.Now,
                IsActive = true,
                PasswordHash = "default" // В реальном приложении должно быть хеширование
            };
        }

        private void SaveMaster()
        {
            if (string.IsNullOrWhiteSpace(EditingMaster.LastName) || 
                string.IsNullOrWhiteSpace(EditingMaster.FirstName))
            {
                ShowError("Укажите фамилию и имя");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingMaster.Username))
            {
                ShowError("Укажите имя пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingMaster.PasswordHash))
            {
                ShowError("Укажите пароль");
                return;
            }

            if (EditingMaster.SalaryRate.HasValue && EditingMaster.SalaryRate < 0)
            {
                ShowError("Ставка не может быть отрицательной");
                return;
            }

            if (EditingMaster.CommissionRate.HasValue && 
                (EditingMaster.CommissionRate < 0 || EditingMaster.CommissionRate > 100))
            {
                ShowError("Процент комиссии должен быть от 0 до 100");
                return;
            }

            try
            {
                if (EditingMaster.MasterID == 0)
                {
                    _context.Masters.Add(EditingMaster);
                    Masters.Add(EditingMaster);
                    ShowInfo("Мастер добавлен");
                }
                else
                {
                    var existing = _context.Masters.Find(EditingMaster.MasterID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingMaster);
                        var index = Masters.IndexOf(SelectedMaster);
                        if (index >= 0)
                        {
                            Masters[index] = existing;
                        }
                        ShowInfo("Мастер обновлен");
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

        private void DeleteMaster()
        {
            if (SelectedMaster == null)
            {
                ShowError("Выберите мастера для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить мастера {SelectedMaster.FullName}?"))
                return;

            try
            {
                _context.Masters.Remove(SelectedMaster);
                Masters.Remove(SelectedMaster);
                _context.SaveChanges();
                ShowInfo("Мастер удален");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterMasters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Masters.Where(m =>
                m.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (m.Specialization != null && m.Specialization.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (m.Phone != null && m.Phone.Contains(SearchText)) ||
                m.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Masters.Clear();
            foreach (var master in filtered)
            {
                Masters.Add(master);
            }
        }
    }
}