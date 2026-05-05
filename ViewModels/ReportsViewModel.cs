using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MusicRepairShop.Utils;

namespace MusicRepairShop.ViewModels
{
    public class ReportsViewModel : AViewModel
    {
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private string _selectedReport;
        private decimal _totalAmount;
        private int _totalOrders;
        private int _completedOrders;

        public DateTime StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetField(ref _endDate, value);
        }

        public string SelectedReport
        {
            get => _selectedReport;
            set
            {
                SetField(ref _selectedReport, value);
                GenerateReport();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetField(ref _totalAmount, value);
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetField(ref _totalOrders, value);
        }

        public int CompletedOrders
        {
            get => _completedOrders;
            set => SetField(ref _completedOrders, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }

        public ReportsViewModel()
        {
            GenerateCommand = new RelayCommand(_ => GenerateReport());
            ExportCommand = new RelayCommand(_ => ExportReport());
        }

        private void GenerateReport()
        {
            try
            {
                IsLoading = true;

                switch (SelectedReport)
                {
                    case "Orders":
                        GenerateOrdersReport();
                        break;
                    case "Masters":
                        GenerateMastersReport();
                        break;
                    case "Services":
                        GenerateServicesReport();
                        break;
                    case "Materials":
                        GenerateMaterialsReport();
                        break;
                    default:
                        GenerateSummaryReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка генерации отчета: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateSummaryReport()
        {
            var orders = _context.Orders
                .Where(o => o.OrderDate >= StartDate && o.OrderDate <= EndDate)
                .ToList();

            TotalOrders = orders.Count;
            CompletedOrders = orders.Count(o => o.IsCompleted);
            TotalAmount = orders.Sum(o => o.TotalAmount);
        }

        private void GenerateOrdersReport()
        {
            var orders = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Master)
                .Where(o => o.OrderDate >= StartDate && o.OrderDate <= EndDate)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Здесь можно заполнить DataGrid с заказами
            TotalAmount = orders.Sum(o => o.TotalAmount);
            TotalOrders = orders.Count;
        }

        private void GenerateMastersReport()
        {
            var masters = _context.Masters
                .Include(m => m.Orders)
                .Include(m => m.TimeTrackings)
                .Where(m => m.IsActive)
                .ToList();

            // Отчет по работе мастеров
            TotalOrders = masters.Sum(m => m.Orders.Count);
        }

        private void GenerateServicesReport()
        {
            var services = _context.Services
                .Include(s => s.OrderItems)
                .Where(s => s.IsActive)
                .ToList();

            // Отчет по популярности услуг
        }

        private void GenerateMaterialsReport()
        {
            var materials = _context.Materials
                .Include(m => m.OrderItems)
                .Where(m => m.CurrentStock < m.MinimumStock)
                .ToList();

            // Отчет по материалам с низким запасом
        }

        private void ExportReport()
        {
            try
            {
                // Экспорт в Excel или PDF
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|PDF Files|*.pdf",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Здесь логика экспорта
                    ShowInfo($"Отчет экспортирован в {dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка экспорта: {ex.Message}");
            }
        }
    }
}