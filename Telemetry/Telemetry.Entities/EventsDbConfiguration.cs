using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Entities
{
    using System.Data.Entity;
    using System.Data.Entity.SqlServer;

    public class EventsDbConfiguration : DbConfiguration
    {
        public EventsDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () =>
            new SqlAzureExecutionStrategy(10, TimeSpan.FromSeconds(5)));
        }
    }
}
