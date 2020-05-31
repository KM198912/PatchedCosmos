using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL.Drivers.USB
{
    public static class USBHost
    {

        static Device USBController;
        public static void Initialize()
        {

            USB.USBHostOHCI.ScanDevices();
                
            
            }
    }
}
