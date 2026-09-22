using System.Management;

namespace TechBox.Models.CCM
{
    internal class SmsClient
    {
        public SmsClient(string computerName, string clientNamespacePath, ConnectionOptions connection)
        {
            ComputerName = computerName;
            ClientNamespacePath = $@"\\{computerName}\{clientNamespacePath}";
            //ClientNamespacePath = clientNamespacePath;
            SmsClientClassPath = clientNamespacePath + ":SMS_Client";
            Connection = connection;
            IsClientInstalled = IsInstalled();

            ManagementScope scope = new ManagementScope(ClientNamespacePath, Connection);
            ManagementPath path = new ManagementPath("SMS_Client");
            SmsClientClass = new ManagementClass(scope, path, null);
        }

        public string ComputerName { get; set; }

        public string ClientNamespacePath { get; set; }

        public ConnectionOptions Connection { get; set; }

        public bool IsClientInstalled { get; set; }

        public bool IsConnected { get; set; }

        public ManagementClass SmsClientClass { get; set; }

        public string SmsClientClassPath { get; set; }

        public bool IsInstalled()
        {
            const string queryString = "SELECT * FROM meta_class WHERE __Class = 'SMS_Client'";

            ManagementScope scope = new ManagementScope(ClientNamespacePath, Connection);
            ObjectQuery query = new ObjectQuery(queryString);
            EnumerationOptions eOption = new EnumerationOptions();
            ManagementObjectSearcher queryClientSearcher = new ManagementObjectSearcher(scope, query, eOption);

            ManagementObject ccmClient = (from ManagementClass mClass in queryClientSearcher.Get()
                                          orderby mClass.Path.ClassName
                                          select mClass).FirstOrDefault();

            return ccmClient != null;
        }
    }
}
