using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MusicRepairShop.Models
{
    public class Order : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; }

        [Required]
        public int ClientID { get; set; }

        [Required]
        public int InstrumentID { get; set; }

        [Required]
        public int MasterID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? PlannedDate { get; set; }

        public DateTime? ActualDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Принят";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Prepayment { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [StringLength(1000)]
        public string Notes { get; set; }

        // Навигационные свойства
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }

        [ForeignKey("InstrumentID")]
        public virtual Instrument Instrument { get; set; }

        [ForeignKey("MasterID")]
        public virtual Master Master { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();

        [NotMapped]
        public bool IsCompleted => Status == "Выдан";
        
        [NotMapped]
        public decimal RemainingAmount => TotalAmount - Prepayment - (Payments?.Sum(p => p.Amount) ?? 0);
    }
}