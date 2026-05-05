using Microsoft.EntityFrameworkCore;
using MusicRepairShop.Models;
using System.Linq;

namespace MusicRepairShop.Api
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Master> Masters { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TimeTracking> TimeTrackings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-PTH3OC7\\SQLEXPRESS;" +
                                        "Integrated Security=True;" +
                                        "Persist Security Info=False;" +
                                        "Pooling=False;" +
                                        "MultipleActiveResultSets=False;" +
                                        "Connect Timeout=30;" +
                                        "Encrypt=True;" +
                                        "TrustServerCertificate=True;" +
                                        "Packet Size=4096;" +
                                        "Command Timeout=0;" +
                                        "Database=MRShop");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client constraints
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .Property(c => c.Phone)
                .HasAnnotation("RegularExpression", @"^(\+|\d).*$");

            // Instrument constraints
            modelBuilder.Entity<Instrument>()
                .HasIndex(i => i.SerialNumber)
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");

            modelBuilder.Entity<Instrument>()
                .HasCheckConstraint("CK_Instruments_Year", "[Year] BETWEEN 1900 AND YEAR(GETDATE()) + 1");

            // Service constraints
            modelBuilder.Entity<Service>()
                .HasIndex(s => s.ServiceName)
                .IsUnique();

            modelBuilder.Entity<Service>()
                .HasCheckConstraint("CK_Services_Price", "[Price] > 0");

            modelBuilder.Entity<Service>()
                .HasCheckConstraint("CK_Services_Time", "[StandardTime] >= 0");

            // Material constraints
            modelBuilder.Entity<Material>()
                .HasIndex(m => m.MaterialName)
                .IsUnique();

            modelBuilder.Entity<Material>()
                .HasCheckConstraint("CK_Materials_Stock", "[CurrentStock] >= 0 AND [MinimumStock] >= 0");

            modelBuilder.Entity<Material>()
                .HasCheckConstraint("CK_Materials_Prices", "[SalePrice] >= [PurchasePrice]");

            // Master constraints
            modelBuilder.Entity<Master>()
                .HasIndex(m => m.Username)
                .IsUnique();

            modelBuilder.Entity<Master>()
                .HasCheckConstraint("CK_Masters_Commission", "[CommissionRate] BETWEEN 0 AND 100");

            modelBuilder.Entity<Master>()
                .HasCheckConstraint("CK_Masters_Salary", "[SalaryRate] >= 0");

            // Order constraints
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => v);

            modelBuilder.Entity<Order>()
                .HasCheckConstraint("CK_Orders_Status", "[Status] IN ('Принят','В работе','Готов','Выдан','Отменен')");

            modelBuilder.Entity<Order>()
                .HasCheckConstraint("CK_Orders_Prepayment", "[Prepayment] >= 0 AND [Prepayment] <= [TotalAmount]");

            // OrderItem constraints
            modelBuilder.Entity<OrderItem>()
                .HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");

            modelBuilder.Entity<OrderItem>()
                .HasCheckConstraint("CK_OrderItems_Price", "[UnitPrice] > 0");

            // Payment constraints
            modelBuilder.Entity<Payment>()
                .HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion(
                    v => v.ToString(),
                    v => v);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentType)
                .HasConversion(
                    v => v.ToString(),
                    v => v);

            modelBuilder.Entity<Payment>()
                .HasCheckConstraint("CK_Payments_Method", "[PaymentMethod] IN ('Наличные','Карта','Перевод')");

            modelBuilder.Entity<Payment>()
                .HasCheckConstraint("CK_Payments_Type", "[PaymentType] IN ('Предоплата','Окончательная оплата')");

            // Relationships
            modelBuilder.Entity<Instrument>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Instruments)
                .HasForeignKey(i => i.ClientID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Instrument)
                .WithMany(i => i.Orders)
                .HasForeignKey(o => o.InstrumentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Master)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Service)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(oi => oi.ServiceID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Material)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MaterialID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeTracking>()
                .HasOne(tt => tt.Order)
                .WithMany(o => o.TimeTrackings)
                .HasForeignKey(tt => tt.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeTracking>()
                .HasOne(tt => tt.Master)
                .WithMany(m => m.TimeTrackings)
                .HasForeignKey(tt => tt.MasterID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AModel && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((AModel)entityEntry.Entity).ModifiedDate = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((AModel)entityEntry.Entity).CreatedDate = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }
}