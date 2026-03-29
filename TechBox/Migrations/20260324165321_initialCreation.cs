using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechBox.Migrations
{
    /// <inheritdoc />
    public partial class initialCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SCCMActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ClientAction = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCCMActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SCCMActions",
                columns: new[] { "Id", "ClientAction", "IsEnabled", "Name" },
                values: new object[,]
                {
                    { 1, "{00000000-0000-0000-0000-000000000001}", true, "Hardware Inventory Cycle" },
                    { 2, "{00000000-0000-0000-0000-000000000002}", true, "Software Inventory Cycle" },
                    { 3, "{00000000-0000-0000-0000-000000000003}", true, "Discovery Data Collection Cycle" },
                    { 4, "{00000000-0000-0000-0000-000000000010}", true, "File Collection Cycle" },
                    { 5, "{00000000-0000-0000-0000-000000000011}", false, "IDMIF Collection" },
                    { 6, "{00000000-0000-0000-0000-000000000012}", false, "Client Machine Authentication" },
                    { 7, "{00000000-0000-0000-0000-000000000021}", true, "Machine Policy Retrieval & Evaluation Cycle" },
                    { 8, "{00000000-0000-0000-0000-000000000022}", false, "Evaluate Machine Policies" },
                    { 9, "{00000000-0000-0000-0000-000000000023}", false, "Refresh Default MP Task" },
                    { 10, "{00000000-0000-0000-0000-000000000024}", false, "Refresh Locations Task" },
                    { 11, "{00000000-0000-0000-0000-000000000025}", false, "Timeout Refresh Task" },
                    { 12, "{00000000-0000-0000-0000-000000000026}", false, "Policy Agent Request Assignment (User)" },
                    { 13, "{00000000-0000-0000-0000-000000000027}", false, "User Policy Retrieval & Evaluation Cycle" },
                    { 14, "{00000000-0000-0000-0000-000000000031}", true, "Software Metering Generating Usage Report" },
                    { 15, "{00000000-0000-0000-0000-000000000032}", true, "Windows Installer Source List Update Cycle" },
                    { 16, "{00000000-0000-0000-0000-000000000037}", false, "Clearing Proxy Settings Cache" },
                    { 17, "{00000000-0000-0000-0000-000000000040}", false, "Machine Policy Agent Cleanup" },
                    { 18, "{00000000-0000-0000-0000-000000000041}", false, "User Policy Agent Cleanup" },
                    { 19, "{00000000-0000-0000-0000-000000000042}", false, "Policy Agent Validate Machine Policy/Assignment" },
                    { 20, "{00000000-0000-0000-0000-000000000043}", false, "Policy Agent Validate User Policy/Assignment" },
                    { 21, "{00000000-0000-0000-0000-000000000051}", false, "Retrying/Refreshing Certificates in AD on MP" },
                    { 22, "{00000000-0000-0000-0000-000000000061}", false, "Peer DP Status Reporting" },
                    { 23, "{00000000-0000-0000-0000-000000000062}", false, "Peer DP Pending Package Check Schedule" },
                    { 24, "{00000000-0000-0000-0000-000000000063}", false, "SUM Updates Install Schedule" },
                    { 25, "{00000000-0000-0000-0000-000000000071}", false, "NAP action" },
                    { 26, "{00000000-0000-0000-0000-000000000101}", false, "Hardware Inventory Collection Cycle" },
                    { 27, "{00000000-0000-0000-0000-000000000102}", false, "Software Inventory Collection Cycle" },
                    { 28, "{00000000-0000-0000-0000-000000000103}", false, "Discovery Data Collection Cycle" },
                    { 29, "{00000000-0000-0000-0000-000000000104}", false, "File Collection Cycle" },
                    { 30, "{00000000-0000-0000-0000-000000000105}", false, "IDMIF Collection Cycle" },
                    { 31, "{00000000-0000-0000-0000-000000000106}", false, "Software Metering Usage Report Cycle" },
                    { 32, "{00000000-0000-0000-0000-000000000107}", false, "Windows Installer Source List Update Cycle" },
                    { 33, "{00000000-0000-0000-0000-000000000108}", true, "Software Updates Deployment Evaluation Cycle" },
                    { 34, "{00000000-0000-0000-0000-000000000109}", false, "Branch Distribution Point Maintenance Task" },
                    { 35, "{00000000-0000-0000-0000-000000000110}", false, "DCM Policy" },
                    { 36, "{00000000-0000-0000-0000-000000000111}", false, "Send Unsent State Message" },
                    { 37, "{00000000-0000-0000-0000-000000000112}", false, "State System Policy Cache Cleanout" },
                    { 38, "{00000000-0000-0000-0000-000000000113}", true, "Software Updates Scan Cycle" },
                    { 39, "{00000000-0000-0000-0000-000000000114}", false, "Update Store Policy" },
                    { 40, "{00000000-0000-0000-0000-000000000115}", false, "State System Policy Bulk Send High" },
                    { 41, "{00000000-0000-0000-0000-000000000116}", false, "State System Policy Bulk Send Low" },
                    { 42, "{00000000-0000-0000-0000-000000000120}", false, "AMT Status Check Policy" },
                    { 43, "{00000000-0000-0000-0000-000000000121}", true, "Application Deployment Evaluation Cycle" },
                    { 44, "{00000000-0000-0000-0000-000000000122}", false, "Application Manager User Policy Action" },
                    { 45, "{00000000-0000-0000-0000-000000000123}", false, "Application Manager Global Evaluation Action" },
                    { 46, "{00000000-0000-0000-0000-000000000131}", false, "Power Management Start Summarizer" },
                    { 47, "{00000000-0000-0000-0000-000000000221}", false, "Endpoint Deployment Reevaluate" },
                    { 48, "{00000000-0000-0000-0000-000000000222}", false, "Endpoint AM Policy Reevaluate" },
                    { 49, "{00000000-0000-0000-0000-000000000223}", false, "External Event Detection" }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[] { 1, "isInitialSetup", "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SCCMActions");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
