using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class OrderItem : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemID { get; set; }

        [Required]
        public int OrderID { get; set; }

        public int? ServiceID { get; set; }

        public int? MaterialID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }

        [ForeignKey("MaterialID")]
        public virtual Material Material { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;

        [NotMapped]
        public string ItemName => Service != null ? Service.ServiceName : Material?.MaterialName;
    }
}