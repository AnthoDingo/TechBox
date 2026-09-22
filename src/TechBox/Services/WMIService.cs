using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Diagnostics.Metrics;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;
using TechBox.Statics;

namespace TechBox.Services
{
    public class WMIService : IWMI
    {
        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer)
        {
            return this.GetInstances(ClassName, Computer, string.Empty, @"root\cimv2", false);
        }

        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer, string WhereClause)
        {
            return this.GetInstances(ClassName, Computer, WhereClause, @"root\cimv2", false);
        }
        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer, string WhereClause, string NameSpace)
        {
            return this.GetInstances(ClassName, Computer, WhereClause, NameSpace, false);
        }

        public IEnumerable<CimInstance> GetInstances(
            string ClassName, 
            Computer Computer,
            string? WhereClause = "",
            string? NameSpace = @"root\cimv2", 
            bool? Impersonate = false
            )
        {
            DComSessionOptions DComOptions = new DComSessionOptions();
            DComOptions.Timeout = new TimeSpan(0,3,0);

            if(Impersonate == true)
                DComOptions.Impersonation = ImpersonationType.Impersonate;

            // Queries the remote machine under the admin credentials from the settings page when
            // they are configured, so an inventory read works even when the technician's own account
            // has no rights there. Null - none configured, or the target is this machine, which MI
            // refuses to connect to with explicit credentials - leaves the current session in place.
            CimCredential? credential = RemoteCredentials.CreateCimCredential(Computer.Name);
            if (credential is not null)
                DComOptions.AddDestinationCredentials(credential);

            using CimSession mySession = CimSession.Create(Computer.Name, DComOptions);
            // Materialize while the session is open: QueryInstances is lazily evaluated
            // and would otherwise be enumerated after the session (and its DCOM
            // connection) has already been disposed.
            return mySession.QueryInstances(NameSpace, "WQL", $"SELECT * FROM {ClassName} {WhereClause}").ToList();
        }

        public T? GetValue<T>(CimInstance instance, string Name)
        {
            if (instance.CimInstanceProperties[Name] == null)
                return default(T); ;

            return (T)instance.CimInstanceProperties[Name].Value;
        }
    }
}
