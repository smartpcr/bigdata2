using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices.Core.DeviceProvisioningModels
{
    public class DeviceEndpoint
    {
        public string Uri { get; set; }
        public string EventHubName { get; set; }
        public string AccessToken { get; set; }
    }
}
