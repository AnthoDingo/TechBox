using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Enums;

namespace TechBox.Services
{
    internal class BackupService : CopyDataService
    {
        internal async Task BackupProfile(string computer, string username, UserFolder folder)
        {
            await CopyFolder($@"\\{computer}\c$\Users\{username}", $@"\\REDACTED-SERVER\REDACTED-SHARE\{username}", folder);
        }

    }
}
