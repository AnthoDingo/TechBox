using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechBox.Enums
{
    public static class WMI_ClassName
    {
        public static readonly string ComputerSystem = "win32_ComputerSystem";
        public static readonly string Processor = "win32_Processor";
        public static readonly string Bios = "win32_SystemEnclosure";
        public static readonly string PhysicalDisk = "win32_DiskDrive";
        public static readonly string LogicalDisk = "win32_LogicalDisk";
        public static readonly string Volume = "win32_Volume";
        public static readonly string UserAccount = "win32_UserAccount";
        public static readonly string OperatingSystem = "win32_OperatingSystem";
        public static readonly string Application = "Win32_Product";
        public static readonly string CCM_Application = "CCM_Application";
    }
}
