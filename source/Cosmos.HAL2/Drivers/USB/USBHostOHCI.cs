using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.Drivers.USB
{
    public class USBHostOHCI
    {
      static Device devices;
        public static void ScanDevices()
        {
            foreach (PCIDevice pci in Cosmos.HAL.PCI.Devices)
            {
           
                if (pci.ClassCode == 0x0c && //bus
                    pci.Subclass == 0x03 && //usb
                    pci.ProgIF == 0x10) //ohci :D 
                {
                    Console.WriteLine("Detected USB Bus: " + pci.ClassCode.ToString());
                    Console.WriteLine("Detected USB Device: " + pci.Subclass.ToString());
                    //(as this is an open standard, vendor/device specific implementations should all work the same)
                    //  Device.Add(new USBHostOHCI());
                    // PCIDevice.
                }
            }
        }

        private PCIDeviceNormal mydevice;
       // private USBHostOHCIRegisters regs;
        public USBHostOHCI(PCIDevice pcidev)
        {
            mydevice = pcidev as PCIDeviceNormal;
         //   regs = new USBHostOHCIRegisters(pcidev.GetAddressSpace(0) as MemoryAddressSpace);
        }


        public static string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
