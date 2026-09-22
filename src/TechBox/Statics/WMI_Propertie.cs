using Microsoft.Management.Infrastructure;

namespace TechBox.Statics
{
    public static class WMI_Propertie
    {
        public static T GetValue<T>(CimInstance instance, string Name) where T : Type
        {
            if (instance.CimInstanceProperties[Name] == null)
                return null;

            return (T)instance.CimInstanceProperties[Name].Value;
        }
    }
}
