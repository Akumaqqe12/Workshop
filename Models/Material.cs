using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class Material : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaterialID { get; set; }

        [Required]
        [StringLength(100)]
        public string MaterialName { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal CurrentStock { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal MinimumStock { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalePrice { get; set; }

        public int? SupplierID { get; set; }

        // Навигационные свойства
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [NotMapped]
        public bool IsLowStock => CurrentStock < MinimumStock;
    }
}