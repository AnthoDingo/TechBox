using Microsoft.EntityFrameworkCore;
using TechBox.Models;

namespace TechBox.Databases
{
    internal class SQLiteContext : DbContext
    {

        public DbSet<SCCMAction> SCCMActions { get; set; }
		public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbFolder = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TechBox");
			string dbPath = System.IO.Path.Join(dbFolder, "db.sqlite");

            if (!System.IO.Directory.Exists(dbFolder))
            {
                System.IO.Directory.CreateDirectory(dbFolder);
            }

			optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            List<SCCMAction> actions = new List<SCCMAction>()
			{
				new SCCMAction() {Id = 1, Name = "Hardware Inventory Cycle", ClientAction = "{00000000-0000-0000-0000-000000000001}", IsEnabled = true },
				new SCCMAction() {Id = 2, Name = "Software Inventory Cycle", ClientAction = "{00000000-0000-0000-0000-000000000002}", IsEnabled = true },
				new SCCMAction() {Id = 3, Name = "Discovery Data Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000003}", IsEnabled = true },
				new SCCMAction() {Id = 4, Name = "File Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000010}", IsEnabled = true },
				new SCCMAction() {Id = 5, Name = "IDMIF Collection", ClientAction = "{00000000-0000-0000-0000-000000000011}" },
				new SCCMAction() {Id = 6, Name = "Client Machine Authentication", ClientAction = "{00000000-0000-0000-0000-000000000012}" },
				new SCCMAction() {Id = 7, Name = "Machine Policy Retrieval & Evaluation Cycle", ClientAction = "{00000000-0000-0000-0000-000000000021}", IsEnabled = true },
				new SCCMAction() {Id = 8, Name = "Evaluate Machine Policies", ClientAction = "{00000000-0000-0000-0000-000000000022}" },
				new SCCMAction() {Id = 9, Name = "Refresh Default MP Task", ClientAction = "{00000000-0000-0000-0000-000000000023}" },
				new SCCMAction() {Id = 10, Name = "Refresh Locations Task", ClientAction = "{00000000-0000-0000-0000-000000000024}" },
				new SCCMAction() {Id = 11, Name = "Timeout Refresh Task", ClientAction = "{00000000-0000-0000-0000-000000000025}" },
				new SCCMAction() {Id = 12, Name = "Policy Agent Request Assignment (User)", ClientAction = "{00000000-0000-0000-0000-000000000026}" },
				new SCCMAction() {Id = 13, Name = "User Policy Retrieval & Evaluation Cycle", ClientAction = "{00000000-0000-0000-0000-000000000027}" },
				new SCCMAction() {Id = 14, Name = "Software Metering Generating Usage Report", ClientAction = "{00000000-0000-0000-0000-000000000031}", IsEnabled = true },
				new SCCMAction() {Id = 15, Name = "Windows Installer Source List Update Cycle", ClientAction = "{00000000-0000-0000-0000-000000000032}", IsEnabled = true },
				new SCCMAction() {Id = 16, Name = "Clearing Proxy Settings Cache", ClientAction = "{00000000-0000-0000-0000-000000000037}" },
				new SCCMAction() {Id = 17, Name = "Machine Policy Agent Cleanup", ClientAction = "{00000000-0000-0000-0000-000000000040}" },
				new SCCMAction() {Id = 18, Name = "User Policy Agent Cleanup", ClientAction = "{00000000-0000-0000-0000-000000000041}" },
				new SCCMAction() {Id = 19, Name = "Policy Agent Validate Machine Policy/Assignment", ClientAction = "{00000000-0000-0000-0000-000000000042}" },
				new SCCMAction() {Id = 20, Name = "Policy Agent Validate User Policy/Assignment", ClientAction = "{00000000-0000-0000-0000-000000000043}" },
				new SCCMAction() {Id = 21, Name = "Retrying/Refreshing Certificates in AD on MP", ClientAction = "{00000000-0000-0000-0000-000000000051}" },
				new SCCMAction() {Id = 22, Name = "Peer DP Status Reporting", ClientAction = "{00000000-0000-0000-0000-000000000061}" },
				new SCCMAction() {Id = 23, Name = "Peer DP Pending Package Check Schedule", ClientAction = "{00000000-0000-0000-0000-000000000062}" },
				new SCCMAction() {Id = 24, Name = "SUM Updates Install Schedule", ClientAction = "{00000000-0000-0000-0000-000000000063}" },
				new SCCMAction() {Id = 25, Name = "NAP action", ClientAction = "{00000000-0000-0000-0000-000000000071}" },
				new SCCMAction() {Id = 26, Name = "Hardware Inventory Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000101}" },
				new SCCMAction() {Id = 27, Name = "Software Inventory Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000102}" },
				new SCCMAction() {Id = 28, Name = "Discovery Data Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000103}" },
				new SCCMAction() {Id = 29, Name = "File Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000104}" },
				new SCCMAction() {Id = 30, Name = "IDMIF Collection Cycle", ClientAction = "{00000000-0000-0000-0000-000000000105}" },
				new SCCMAction() {Id = 31, Name = "Software Metering Usage Report Cycle", ClientAction = "{00000000-0000-0000-0000-000000000106}" },
				new SCCMAction() {Id = 32, Name = "Windows Installer Source List Update Cycle", ClientAction = "{00000000-0000-0000-0000-000000000107}" },
				new SCCMAction() {Id = 33, Name = "Software Updates Deployment Evaluation Cycle", ClientAction = "{00000000-0000-0000-0000-000000000108}", IsEnabled = true },
				new SCCMAction() {Id = 34, Name = "Branch Distribution Point Maintenance Task", ClientAction = "{00000000-0000-0000-0000-000000000109}" },
				new SCCMAction() {Id = 35, Name = "DCM Policy", ClientAction = "{00000000-0000-0000-0000-000000000110}" },
				new SCCMAction() {Id = 36, Name = "Send Unsent State Message", ClientAction = "{00000000-0000-0000-0000-000000000111}" },
				new SCCMAction() {Id = 37, Name = "State System Policy Cache Cleanout", ClientAction = "{00000000-0000-0000-0000-000000000112}" },
				new SCCMAction() {Id = 38, Name = "Software Updates Scan Cycle", ClientAction = "{00000000-0000-0000-0000-000000000113}", IsEnabled = true },
				new SCCMAction() {Id = 39, Name = "Update Store Policy", ClientAction = "{00000000-0000-0000-0000-000000000114}" },
				new SCCMAction() {Id = 40, Name = "State System Policy Bulk Send High", ClientAction = "{00000000-0000-0000-0000-000000000115}" },
				new SCCMAction() {Id = 41, Name = "State System Policy Bulk Send Low", ClientAction = "{00000000-0000-0000-0000-000000000116}" },
				new SCCMAction() {Id = 42, Name = "AMT Status Check Policy", ClientAction = "{00000000-0000-0000-0000-000000000120}" },
				new SCCMAction() {Id = 43, Name = "Application Deployment Evaluation Cycle", ClientAction = "{00000000-0000-0000-0000-000000000121}", IsEnabled = true },
				new SCCMAction() {Id = 44, Name = "Application Manager User Policy Action", ClientAction = "{00000000-0000-0000-0000-000000000122}" },
				new SCCMAction() {Id = 45, Name = "Application Manager Global Evaluation Action", ClientAction = "{00000000-0000-0000-0000-000000000123}" },
				new SCCMAction() {Id = 46, Name = "Power Management Start Summarizer", ClientAction = "{00000000-0000-0000-0000-000000000131}" },
				new SCCMAction() {Id = 47, Name = "Endpoint Deployment Reevaluate", ClientAction = "{00000000-0000-0000-0000-000000000221}" },
				new SCCMAction() {Id = 48, Name = "Endpoint AM Policy Reevaluate", ClientAction = "{00000000-0000-0000-0000-000000000222}" },
				new SCCMAction() {Id = 49, Name = "External Event Detection", ClientAction = "{00000000-0000-0000-0000-000000000223}" }
			};

			modelBuilder.Entity<SCCMAction>()
                .HasData(actions);

			modelBuilder.Entity<Setting>()
				.HasData(
					new Setting() { Id = 1, Name = "isInitialSetup", Value = "true" },
                    new Setting() { Id = 2, Name = "ldap_path", Value = string.Empty }
                );
        }
    }
}
