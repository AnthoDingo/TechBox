using System.Runtime.InteropServices;

namespace TechBox.Statics
{
    internal static class NetworkShare
    {
        private const int RESOURCETYPE_DISK = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string? lpLocalName;
            public string? lpRemoteName;
            public string? lpComment;
            public string? lpProvider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE netResource, string? password, string? username, int flags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        // Authenticates the current logon session against \\server (via IPC$, no drive
        // mapped) so subsequent UNC access - e.g. Explorer opened via Process.Start - reuses
        // this session instead of prompting for credentials or being denied.
        public static bool Connect(string server, string username, string password)
        {
            string remoteName = $@"\\{server}\IPC$";

            // Drop any existing session first - WNetAddConnection2 fails with
            // ERROR_SESSION_CREDENTIAL_CONFLICT if one is already established under
            // different credentials.
            WNetCancelConnection2(remoteName, 0, true);

            NETRESOURCE resource = new()
            {
                dwType = RESOURCETYPE_DISK,
                lpRemoteName = remoteName,
            };

            int result = WNetAddConnection2(ref resource, password, username, 0);
            return result == 0;
        }
    }
}
