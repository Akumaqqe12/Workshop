using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class MaterialsViewModel : AViewModel
    {
        private ObservableCollection<Material> _materials;
        private Material _selectedMaterial;
        private Material _editingMaterial;
        private string _searchText;

        
        private bool _showOnlyLowStock;
        public bool ShowOnlyLowStock
        {
            get => _showOnlyLowStock;
            set
            {
                SetField(ref _showOnlyLowStock, value);
                FilterMaterials();
            }
        }

        public int LowStockCount => Materials.Count(m => m.IsLowStock);

        public decimal Margin
        {
            get
            {
                if (SelectedMaterial != null && 
                    SelectedMaterial.PurchasePrice.HasValue && 
                    SelectedMaterial.PurchasePrice.Value > 0 &&
                    SelectedMaterial.SalePrice.HasValue)
                {
                    return (SelectedMaterial.SalePrice.Value - SelectedMaterial.PurchasePrice.Value) / 
                        SelectedMaterial.PurchasePrice.Value * 100;
                }
                return 0;
            }
        }
        
        public ObservableCollection<Material> Materials
        {
            get => _materials;
            set => SetField(ref _materials, value);
        }

        public Material SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                SetField(ref _selectedMaterial, value);
                if (value != null)
                {
                    EditingMaterial = new Material
                    {
                        MaterialID = value.MaterialID,
                        MaterialName = value.MaterialName,
                        Category = value.Category,
                        Description = value.Description,
                        Unit = value.Unit,
                        CurrentStock = value.CurrentStock,
                        MinimumStock = value.MinimumStock,
                        PurchasePrice = value.PurchasePrice,
                        SalePrice = value.SalePrice,
                        SupplierID = value.SupplierID
                    };
                }
            }
        }

        public Material EditingMaterial
        {
            get => _editingMaterial;
            set => SetField(ref _editingMaterial, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterMaterials();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public MaterialsViewModel()
        {
            Materials = new ObservableCollection<Material>();
            EditingMaterial = new Material();
            
            AddCommand = new RelayCommand(_ => AddMaterial());
            SaveCommand = new RelayCommand(_ => SaveMaterial());
            DeleteCommand = new RelayCommand(_ => DeleteMaterial());
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                Materials.Clear();
                
                var materials = _context.Materials
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.MaterialName)
                    .ToList();
                
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки материалов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddMaterial()
        {
            EditingMaterial = new Material
            {
                Unit = "шт.",
                CurrentStock = 0,
                MinimumStock = 0
            };
        }

        private void SaveMaterial()
        {
            if (string.IsNullOrWhiteSpace(EditingMaterial.MaterialName))
            {
                ShowError("Укажите название материала");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingMaterial.Unit))
            {
                ShowError("Укажите единицу измерения");
                return;
            }

            if (EditingMaterial.CurrentStock < 0)
            {
                ShowError("Текущий запас не может быть отрицательным");
                return;
            }

            if (EditingMaterial.MinimumStock < 0)
            {
                ShowError("Минимальный запас не может быть отрицательным");
                return;
            }

            if (EditingMaterial.PurchasePrice.HasValue && EditingMaterial.PurchasePrice < 0)
            {
                ShowError("Закупочная цена не может быть отрицательной");
                return;
            }

            if (EditingMaterial.SalePrice.HasValue && EditingMaterial.SalePrice < 0)
            {
                ShowError("Цена продажи не может быть отрицательной");
                return;
            }

            if (EditingMaterial.PurchasePrice.HasValue && EditingMaterial.SalePrice.HasValue &&
                EditingMaterial.SalePrice < EditingMaterial.PurchasePrice)
            {
                ShowError("Цена продажи не может быть меньше закупочной цены");
                return;
            }

            try
            {
                if (EditingMaterial.MaterialID == 0)
                {
                    _context.Materials.Add(EditingMaterial);
                    Materials.Add(EditingMaterial);
                    ShowInfo("Материал добавлен");
                }
                else
                {
                    var existing = _context.Materials.Find(EditingMaterial.MaterialID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingMaterial);
                        var index = Materials.IndexOf(SelectedMaterial);
                        if (index >= 0)
                        {
                            Materials[index] = existing;
                        }
                        ShowInfo("Материал обновлен");
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

        private void DeleteMaterial()
        {
            if (SelectedMaterial == null)
            {
                ShowError("Выберите материал для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить материал \"{SelectedMaterial.MaterialName}\"?"))
                return;

            try
            {
                _context.Materials.Remove(SelectedMaterial);
                Materials.Remove(SelectedMaterial);
                _context.SaveChanges();
                ShowInfo("Материал удален");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterMaterials()
        {
            try
            {
                IsLoading = true;
                
                var query = _context.Materials.AsQueryable();

                if (ShowOnlyLowStock)
                {
                    query = query.Where(m => m.CurrentStock < m.MinimumStock);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(m =>
                        m.MaterialName.Contains(SearchText) ||
                        (m.Category != null && m.Category.Contains(SearchText)) ||
                        (m.Description != null && m.Description.Contains(SearchText)));
                }

                var materials = query
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.MaterialName)
                    .ToList();

                Materials.Clear();
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }

                OnPropertyChanged(nameof(LowStockCount));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка фильтрации: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}