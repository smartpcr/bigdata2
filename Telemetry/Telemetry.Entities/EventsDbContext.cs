using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Entities
{
    using System.Data.Entity;
    using Telemetry.Entities.Models;

    public class EventsDbContext : DbContext
    {
        public EventsDbContext()
        {
        }

        public EventsDbContext(string connStr):base(connStr)
        {
        }

        public virtual DbSet<EventMetric> EventMetrics { get; set; }
    }
}
