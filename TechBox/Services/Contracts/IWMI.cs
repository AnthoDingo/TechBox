using Microsoft.Management.Infrastructure;
using TechBox.Models.Hardware;

namespace TechBox.Services.Contracts
{
    public interface IWMI
    {
        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer);
        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer, string WhereClause);
        public IEnumerable<CimInstance> GetInstances(string ClassName, Computer Computer, string WhereClause, string NameSpace);
        public IEnumerable<CimInstance> GetInstances(
            string ClassName,
            Computer Computer,
            string? WhereClause = "",
            string? NameSpace = @"root\cimv2",
            bool? Impersonate = false
        );

    }
}
