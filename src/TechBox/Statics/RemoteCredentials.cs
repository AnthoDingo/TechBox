using System.Management;
using System.Security;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using TechBox.Databases;

namespace TechBox.Statics
{
    /// <summary>
    /// Remote admin credentials configured in the settings page, in the form WMI wants them.
    /// </summary>
    internal static class RemoteCredentials
    {
        /// <summary>
        /// Username and password to authenticate against remote computers with, or empty strings
        /// when none is configured - in which case the current Windows session is used, as before.
        /// </summary>
        public static (string Username, string Password) Get()
        {
            using SQLiteContext db = new();

            string username = db.Settings.FirstOrDefault(s => s.Name == "remote_admin_username")?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                return (string.Empty, string.Empty);
            }

            string encryptedPassword = db.Settings.FirstOrDefault(s => s.Name == "remote_admin_password")?.Value ?? string.Empty;

            return (username, Security.Unprotect(encryptedPassword));
        }

        /// <summary>
        /// Connection options for a WMI query against <paramref name="computerName"/>, carrying the
        /// configured remote admin credentials when there are any.
        /// </summary>
        /// <remarks>
        /// Credentials are deliberately left out for the local machine: WMI rejects a local
        /// connection that carries any ("User credentials cannot be used for local connections").
        /// </remarks>
        public static ConnectionOptions CreateConnectionOptions(string computerName)
        {
            ConnectionOptions options = new()
            {
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true,
            };

            if (IsLocalComputer(computerName))
            {
                return options;
            }

            (string username, string password) = Get();

            if (string.IsNullOrWhiteSpace(username))
            {
                return options;
            }

            options.Username = username;
            options.Password = password;

            return options;
        }

        /// <summary>
        /// The same credentials for the MI stack (<see cref="CimSession"/>), which takes them as a
        /// <see cref="CimCredential"/> rather than through connection options, or
        /// <see langword="null"/> when none is configured or the target is this machine.
        /// </summary>
        public static CimCredential? CreateCimCredential(string computerName)
        {
            if (IsLocalComputer(computerName))
            {
                return null;
            }

            (string username, string password) = Get();

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            // CimCredential wants the domain apart from the user name, unlike ConnectionOptions
            // which takes "DOMAIN\user" whole. A bare name, or a UPN, goes through with no domain.
            string? domain = null;
            int separator = username.IndexOf('\\');
            if (separator > 0)
            {
                domain = username[..separator];
                username = username[(separator + 1)..];
            }

            SecureString securePassword = new();
            foreach (char character in password)
            {
                securePassword.AppendChar(character);
            }

            securePassword.MakeReadOnly();

            return new CimCredential(PasswordAuthenticationMechanism.Default, domain, username, securePassword);
        }

        private static bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return true;
            }

            string name = computerName.Trim().TrimStart('\\');

            // A FQDN still points at this machine: compare on the host part only.
            int dot = name.IndexOf('.');
            if (dot > 0)
            {
                name = name[..dot];
            }

            return name is "." or "localhost"
                || string.Equals(name, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
