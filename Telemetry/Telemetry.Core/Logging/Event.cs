using System.Collections.Generic;

namespace Telemetry.Core.Logging
{
	public class Event
	{
		public Event(string name, IEnumerable<Facet> facets)
		{
			this.Name = name;

			// flatten the facets:
			this.Facets = new Dictionary<string, object>();
			foreach (var facet in facets)
			{
				this.Facets.Add(facet.Name, facet.Value);
			}
		}

		public string Name { get; private set; }

		public Dictionary<string, object> Facets { get; private set; }
	}
}
