using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TechBox.Statics
{
    internal static class Security
    {
        private static readonly Lazy<bool> _isAdmin = new(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        });

        public static bool IsAdmin() => _isAdmin.Value;
    }
}
