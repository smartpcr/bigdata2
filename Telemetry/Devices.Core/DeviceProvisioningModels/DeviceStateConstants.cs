using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices.Core.DeviceProvisioningModels
{
    public static class DeviceStateConstants
    {
        public static readonly string RegisteredState = "registered";
        public static readonly string ProvisionedState = "provisioned";
        public static readonly string RevokedState = "revoked";
    }
}
