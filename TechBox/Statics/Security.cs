using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        // DPAPI-encrypts with CurrentUser scope, so the stored value can only be
        // decrypted by the same Windows account on the same machine.
        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string Unprotect(string protectedText)
        {
            if (string.IsNullOrEmpty(protectedText))
                return string.Empty;

            try
            {
                byte[] decrypted = ProtectedData.Unprotect(Convert.FromBase64String(protectedText), null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (CryptographicException)
            {
                // Stored value belongs to a different user/machine (e.g. restored profile) - treat as unset.
                return string.Empty;
            }
        }
    }
}
