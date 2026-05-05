using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MusicRepairShop.Models;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class OrdersViewModel : AViewModel
    {
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Client> _clients;
        private ObservableCollection<Instrument> _instruments;
        private ObservableCollection<Master> _masters;
        private ObservableCollection<Service> _services;
        private ObservableCollection<Material> _materials;
        private Order _selectedOrder;
        private Order _editingOrder;
        private OrderItem _editingOrderItem;
        private string _searchText;

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetField(ref _orders, value);
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetField(ref _clients, value);
        }

        public ObservableCollection<Instrument> Instruments
        {
            get => _instruments;
            set => SetField(ref _instruments, value);
        }

        public ObservableCollection<Master> Masters
        {
            get => _masters;
            set => SetField(ref _masters, value);
        }

        public ObservableCollection<Service> Services
        {
            get => _services;
            set => SetField(ref _services, value);
        }

        public ObservableCollection<Material> Materials
        {
            get => _materials;
            set => SetField(ref _materials, value);
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetField(ref _selectedOrder, value);
                if (value != null)
                {
                    EditingOrder = new Order
                    {
                        OrderID = value.OrderID,
                        OrderNumber = value.OrderNumber,
                        ClientID = value.ClientID,
                        InstrumentID = value.InstrumentID,
                        MasterID = value.MasterID,
                        OrderDate = value.OrderDate,
                        PlannedDate = value.PlannedDate,
                        ActualDate = value.ActualDate,
                        Status = value.Status,
                        Prepayment = value.Prepayment,
                        TotalAmount = value.TotalAmount,
                        Notes = value.Notes
                    };
                }
            }
        }

        public Order EditingOrder
        {
            get => _editingOrder;
            set => SetField(ref _editingOrder, value);
        }

        public OrderItem EditingOrderItem
        {
            get => _editingOrderItem;
            set => SetField(ref _editingOrderItem, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetField(ref _searchText, value);
                FilterOrders();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ChangeStatusCommand { get; }

        public OrdersViewModel()
        {
            Orders = new ObservableCollection<Order>();
            Clients = new ObservableCollection<Client>();
            Instruments = new ObservableCollection<Instrument>();
            Masters = new ObservableCollection<Master>();
            Services = new ObservableCollection<Service>();
            Materials = new ObservableCollection<Material>();
            EditingOrder = new Order();
            EditingOrderItem = new OrderItem();
            
            AddCommand = new RelayCommand(_ => AddOrder());
            SaveCommand = new RelayCommand(_ => SaveOrder());
            DeleteCommand = new RelayCommand(_ => DeleteOrder());
            RefreshCommand = new RelayCommand(_ => LoadData());
            AddItemCommand = new RelayCommand(_ => AddOrderItem());
            RemoveItemCommand = new RelayCommand(RemoveOrderItem);
            ChangeStatusCommand = new RelayCommand(ChangeStatus);

            LoadData();
        }

        protected override void LoadData()
        {
            try
            {
                IsLoading = true;
                
                LoadReferences();
                
                Orders.Clear();
                var orders = _context.Orders
                    .Include(o => o.Client)
                    .Include(o => o.Instrument)
                    .Include(o => o.Master)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                
                foreach (var order in orders)
                {
                    Orders.Add(order);
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

        private void LoadReferences()
        {
            Clients.Clear();
            foreach (var client in _context.Clients.OrderBy(c => c.LastName).ToList())
                Clients.Add(client);

            Instruments.Clear();
            foreach (var instrument in _context.Instruments.OrderBy(i => i.Manufacturer).ToList())
                Instruments.Add(instrument);

            Masters.Clear();
            foreach (var master in _context.Masters.Where(m => m.IsActive).OrderBy(m => m.LastName).ToList())
                Masters.Add(master);

            Services.Clear();
            foreach (var service in _context.Services.Where(s => s.IsActive).OrderBy(s => s.ServiceName).ToList())
                Services.Add(service);

            Materials.Clear();
            foreach (var material in _context.Materials.OrderBy(m => m.MaterialName).ToList())
                Materials.Add(material);
        }

        private void AddOrder()
        {
            EditingOrder = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Принят",
                Prepayment = 0,
                TotalAmount = 0
            };

            if (Clients.Any())
                EditingOrder.ClientID = Clients.First().ClientID;
            if (Instruments.Any())
                EditingOrder.InstrumentID = Instruments.First().InstrumentID;
            if (Masters.Any())
                EditingOrder.MasterID = Masters.First().MasterID;
        }

        private void SaveOrder()
        {
            if (EditingOrder.ClientID == 0)
            {
                ShowError("Выберите клиента");
                return;
            }

            if (EditingOrder.InstrumentID == 0)
            {
                ShowError("Выберите инструмент");
                return;
            }

            if (EditingOrder.MasterID == 0)
            {
                ShowError("Выберите мастера");
                return;
            }

            if (EditingOrder.PlannedDate.HasValue && EditingOrder.PlannedDate < EditingOrder.OrderDate)
            {
                ShowError("Планируемая дата не может быть раньше даты заказа");
                return;
            }

            if (EditingOrder.Prepayment < 0)
            {
                ShowError("Предоплата не может быть отрицательной");
                return;
            }

            try
            {
                if (EditingOrder.OrderID == 0)
                {
                    EditingOrder.OrderNumber = $"{DateTime.Now:yyMMdd}-{Orders.Count + 1:000}";
                    
                    _context.Orders.Add(EditingOrder);
                    Orders.Add(EditingOrder);
                    ShowInfo("Заказ создан");
                }
                else
                {
                    var existing = _context.Orders.Find(EditingOrder.OrderID);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(EditingOrder);
                        var index = Orders.IndexOf(SelectedOrder);
                        if (index >= 0)
                        {
                            Orders[index] = existing;
                        }
                        ShowInfo("Заказ обновлен");
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

        private void DeleteOrder()
        {
            if (SelectedOrder == null)
            {
                ShowError("Выберите заказ для удаления");
                return;
            }

            if (!ShowConfirmation($"Удалить заказ №{SelectedOrder.OrderNumber}?"))
                return;

            try
            {
                _context.Orders.Remove(SelectedOrder);
                Orders.Remove(SelectedOrder);
                _context.SaveChanges();
                ShowInfo("Заказ удален");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления: {ex.Message}");
            }
        }

        private void AddOrderItem()
        {
            if (EditingOrderItem.ServiceID == 0 && EditingOrderItem.MaterialID == 0)
            {
                ShowError("Выберите услугу или материал");
                return;
            }

            if (EditingOrderItem.Quantity <= 0)
            {
                ShowError("Количество должно быть больше 0");
                return;
            }

            if (EditingOrderItem.UnitPrice <= 0)
            {
                ShowError("Цена должна быть больше 0");
                return;
            }

            if (EditingOrder.OrderID == 0)
            {
                ShowError("Сначала сохраните заказ");
                return;
            }

            try
            {
                EditingOrderItem.OrderItemID = EditingOrder.OrderID;
                
                if (EditingOrderItem.ServiceID != 0)
                {
                    var service = _context.Services.Find(EditingOrderItem.ServiceID);
                    if (service != null && EditingOrderItem.UnitPrice == 0)
                    {
                        EditingOrderItem.UnitPrice = service.Price;
                    }
                }
                else if (EditingOrderItem.MaterialID != 0)
                {
                    var material = _context.Materials.Find(EditingOrderItem.MaterialID);
                    if (material != null)
                    {
                        if (EditingOrderItem.UnitPrice == 0 && material.SalePrice.HasValue)
                        {
                            EditingOrderItem.UnitPrice = material.SalePrice.Value;
                        }
                        
                        if (material.CurrentStock >= EditingOrderItem.Quantity)
                        {
                            material.CurrentStock -= EditingOrderItem.Quantity;
                        }
                        else
                        {
                            ShowError($"Недостаточно материала на складе. Доступно: {material.CurrentStock}");
                            return;
                        }
                    }
                }

                _context.OrderItems.Add(EditingOrderItem);
                
                EditingOrder.TotalAmount += EditingOrderItem.Quantity * EditingOrderItem.UnitPrice;
                
                _context.SaveChanges();
                EditingOrderItem = new OrderItem();
                ShowInfo("Позиция добавлена");
                LoadData();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления позиции: {ex.Message}");
            }
        }

        private void RemoveOrderItem(object parameter)
        {
            if (parameter is OrderItem item)
            {
                if (!ShowConfirmation("Удалить позицию?"))
                    return;

                try
                {
                    if (item.MaterialID.HasValue)
                    {
                        var material = _context.Materials.Find(item.MaterialID);
                        if (material != null)
                        {
                            material.CurrentStock += item.Quantity;
                        }
                    }

                    _context.OrderItems.Remove(item);
                    EditingOrder.TotalAmount -= item.Quantity * item.UnitPrice;
                    _context.SaveChanges();
                    ShowInfo("Позиция удалена");
                    LoadData();
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ChangeStatus(object parameter)
        {
            if (SelectedOrder == null || parameter is not string status)
                return;

            try
            {
                SelectedOrder.Status = status;
                if (status == "Выдан")
                {
                    SelectedOrder.ActualDate = DateTime.Now;
                }

                _context.SaveChanges();
                ShowInfo($"Статус изменен на: {status}");
                LoadData();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка изменения статуса: {ex.Message}");
            }
        }

        private void FilterOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filtered = Orders.Where(o =>
                o.OrderNumber.Contains(SearchText) ||
                o.Client.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                o.Instrument.FullDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                o.Master.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                o.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Orders.Clear();
            foreach (var order in filtered)
            {
                Orders.Add(order);
            }
        }
    }
}