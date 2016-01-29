using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices.Core.DeviceProvisioningModels
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public DeviceMetadata Metadata { get; set; }
    }
}
