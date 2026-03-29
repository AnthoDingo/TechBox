using Microsoft.Management.Infrastructure;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using TechBox.Enums;
using TechBox.Models.ActiveDirectory;
using TechBox.Services;
using TechBox.Statics;
using System.IO;
using System.Collections;

namespace TechBox.Models.Hardware
{
    public class Computer
    {
        public string Name { get; set; }
        public string DN { get; set; }

        public bool IsOnline()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;

            string data = "abcdefghijklmnopqrstuvwxyz";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            PingReply reply = pingSender.Send(Name, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }
        public async Task<bool> IsOnlineAsync()
        {
            return IsOnline();
        }

        #region Hardware
        public string Manufacturer { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public string SerialNumber { get; private set; } = string.Empty;
        public string AssetTag { get; private set; } = string.Empty;

        public string Processor { get; private set; } = string.Empty;
        public BigInteger Memory { get; private set; }
        public string MemoryAsHuman { get; private set; }

        public async Task GetHardware()
        {
            WMIService wmi = new WMIService();
            //IEnumerable<CimInstance>  pc = wmi.GetValue("win32_ComputerSystem", this);
            CimInstance bios = wmi.GetInstances(WMI_ClassName.Bios, this).First();

            Manufacturer = bios.CimInstanceProperties["Manufacturer"].Value.ToString();
            SerialNumber = bios.CimInstanceProperties["SerialNumber"].Value.ToString();
            AssetTag = bios.CimInstanceProperties["SMBiosAssetTag"].Value.ToString();

            CimInstance pc = wmi.GetInstances(WMI_ClassName.ComputerSystem, this).First();

            Model = pc.CimInstanceProperties["Model"].Value.ToString();
            Memory = Convert.ToInt64(pc.CimInstanceProperties["TotalPhysicalMemory"].Value.ToString());
            MemoryAsHuman = Converter.BytesToString(Memory);

            CimInstance proc = wmi.GetInstances(WMI_ClassName.Processor, this).First();

            Processor = proc.CimInstanceProperties["Name"].Value.ToString();

        }

        #endregion

        #region Disks
        public IEnumerable<LogicalDisk> LogicalDisks { get; private set; } = new List<LogicalDisk>();

        public async Task GetLogicalDisks()
        {
            List<LogicalDisk> logicalDisks = new List<LogicalDisk>();

            WMIService wmi = new WMIService();
            IEnumerable<CimInstance> disks = wmi.GetInstances(WMI_ClassName.Volume, this, "WHERE DriveType = 3 AND DriveLetter != null");
            //disks = disks
            //    .Where(d => (bool)d.CimInstanceProperties["SystemVolume"].Value == false)
            //    .Where(d => (int)d.CimInstanceProperties["DriveType"].Value == 3);
            //disks = disks.Where(d => (int)d.CimInstanceProperties["DriveType"].Value == 3);
            //Debug.WriteLine($"Logical disks : {disks.Count()}");
                        
            foreach(CimInstance disk in disks)
            {
                logicalDisks.Add(new LogicalDisk()
                {
                    Name = disk.CimInstanceProperties["Caption"].Value.ToString(),
                    FS = disk.CimInstanceProperties["FileSystem"].Value.ToString(),
                    Size = Convert.ToInt64(disk.CimInstanceProperties["Capacity"].Value.ToString()),
                    FreeSpace = Convert.ToInt64(disk.CimInstanceProperties["FreeSpace"].Value.ToString()),
                });
            }

            LogicalDisks = logicalDisks;
        }

        #endregion

        #region Software

        public IEnumerable<Software> Softwares { get; private set; } = new List<Software>();

        public async Task GetSoftwares()
        {
            List<Software> softs = new List<Software>();
                
            //TODO Paralize request
            WMIService wmi = new WMIService();
            
            IEnumerable<CimInstance> softwares = wmi.GetInstances(WMI_ClassName.Application, this, "WHERE Name != null");
            Debug.WriteLine(softwares.Count());
            foreach(CimInstance soft in softwares)
            {
                try
                {
                    softs.Add(new Software()
                    {
                        Name = (string)soft.CimInstanceProperties["Name"].Value ?? string.Empty,
                        Version = (string)soft.CimInstanceProperties["Version"].Value ?? string.Empty,
                        //Vendor = (string)soft.CimInstanceProperties["Vendor"].Value ?? string.Empty,
                        //InstallDate = (string)soft.CimInstanceProperties["InstallDate"].Value ?? string.Empty
                    });
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                
            }

            //IEnumerable<CimInstance> ccmSoftwares = wmi.GetValue(WMI_ClassName.CCM_Application, this, @"root\ccm\clientSDK");
            //ccmSoftwares = ccmSoftwares.Where(s => s.CimInstanceProperties["InstallSate"].Value.ToString() == "Installed");
            //Debug.WriteLine(ccmSoftwares.Count());
            //foreach (CimInstance soft in ccmSoftwares)
            //{
            //    softs.Add(new Software()
            //    {
            //        Name = soft.CimInstanceProperties["Name"].Value.ToString() ?? string.Empty,
            //        Version = soft.CimInstanceProperties["Version"].Value.ToString() ?? string.Empty,
            //        Vendor = soft.CimInstanceProperties["Publisher"].Value.ToString() ?? string.Empty,
            //        //InstallDate = soft.CimInstanceProperties["InstallDate"].Value.ToString() ?? string.Empty
            //    });
            //}

            Softwares = softs.Order();
        }

        #endregion

        #region User

        public string CurrentUser { get; private set; } = string.Empty;

        public async Task GetConnectedUser()
        {
            WMIService wmi = new WMIService();
            try
            {
                CimInstance user = wmi.GetInstances(WMI_ClassName.ComputerSystem, this).First();
                CurrentUser = user.CimInstanceProperties["UserName"].Value.ToString().Remove(0, 9);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.Message}");
                return;
            }

            //if (CurrentUser == string.Empty)
            //    return;

            try
            {
                ActiveDirectoryService ad = new ActiveDirectoryService();
                User aduser = ad.GetUser(CurrentUser, true);
                CurrentUser = $"{CurrentUser} ({aduser.FullName}) <{aduser.Mail}>";
            }
            catch
            {

            }
            
        }

        public List<string> UserProfiles
        {
            get
            {
                List<string> UnknowProiles = new List<string>
                {
                    "Default",
                    "Public",
                    "Default User",
                    "All Users",
                    "DefaultAppPool",
                    $"{Name}$"
                };
                string computerPath = $"\\\\{Name}\\c$\\Users";  
                List<string> profiles = Directory.EnumerateDirectories(computerPath)
                    .Select(p => new DirectoryInfo(p).Name)
                    .Except(UnknowProiles)
                    .ToList();
                return profiles.Order().ToList();
            }
        }
        #endregion
    }
}
