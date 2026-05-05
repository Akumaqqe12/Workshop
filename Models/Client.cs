using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class Client : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClientID { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Навигационные свойства
        public virtual ICollection<Instrument> Instruments { get; set; } = new List<Instrument>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}