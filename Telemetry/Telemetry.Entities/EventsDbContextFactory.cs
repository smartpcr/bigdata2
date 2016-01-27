using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Entities
{
    public class EventsDbContextFactory
    {
        private readonly string _connStr;

        public EventsDbContextFactory(string connStr)
        {
            _connStr = connStr;
        }

        public virtual EventsDbContext GetContext()
        {
            return new EventsDbContext(_connStr);
        }
    }
}
