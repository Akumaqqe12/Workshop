using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class Instrument : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InstrumentID { get; set; }

        [Required]
        public int ClientID { get; set; }

        [Required]
        [StringLength(30)]
        public string Type { get; set; }

        [StringLength(100)]
        public string Manufacturer { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        [StringLength(50)]
        public string SerialNumber { get; set; }

        [Range(1900, 2100)]
        public short? Year { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [StringLength(500)]
        public string SpecialFeatures { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Навигационные свойства
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [NotMapped]
        public string FullDescription => $"{Manufacturer} {Model} ({SerialNumber})";
    }
}