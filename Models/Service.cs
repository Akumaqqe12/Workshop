using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class Service : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceID { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? StandardTime { get; set; }

        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}