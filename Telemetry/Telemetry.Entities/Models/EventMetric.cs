using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Entities.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class EventMetric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Period { get; set; }

        [Required]
        [StringLength(40)]
        public string EventName { get; set; }

        [Required]
        [StringLength(5)]
        public string PartitionId { get; set; }

        public long? Count { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
