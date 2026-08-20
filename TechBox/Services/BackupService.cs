using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Databases;
using TechBox.Enums;

namespace TechBox.Services
{
    internal class BackupService : CopyDataService
    {
        internal async Task BackupProfile(string computer, string username, UserFolder folder)
        {
            string backupPath;
            using (SQLiteContext db = new SQLiteContext())
            {
                backupPath = db.Settings.FirstOrDefault(s => s.Name == "backup_path")?.Value ?? string.Empty;
            }

            await CopyFolder($@"\\{computer}\c$\Users\{username}", $@"{backupPath}\{username}", folder);
        }

    }
}
