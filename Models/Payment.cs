using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class Payment : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentID { get; set; }

        [Required]
        public int OrderID { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentType { get; set; }

        [StringLength(50)]
        public string TransactionID { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
    }
}