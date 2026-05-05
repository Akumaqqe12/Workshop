using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicRepairShop.Models
{
    public class TimeTracking : AModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TimeTrackingID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public int MasterID { get; set; }

        public DateTime WorkDate { get; set; } = DateTime.Now.Date;

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("MasterID")]
        public virtual Master Master { get; set; }

        [NotMapped]
        public decimal Duration
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue && EndTime > StartTime)
                {
                    return (decimal)(EndTime.Value - StartTime.Value).TotalHours;
                }
                return 0;
            }
        }
    }
}