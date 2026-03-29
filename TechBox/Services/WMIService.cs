using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Diagnostics.Metrics;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;

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

            CimSession mySession = CimSession.Create(Computer.Name, DComOptions);
            IEnumerable<CimInstance> queryInstance = mySession.QueryInstances(NameSpace, "WQL", $"SELECT * FROM {ClassName} {WhereClause}");

            return queryInstance;
        }

        public T? GetValue<T>(CimInstance instance, string Name)
        {
            if (instance.CimInstanceProperties[Name] == null)
                return default(T); ;

            return (T)instance.CimInstanceProperties[Name].Value;
        }
    }
}
