using System;
using System.Diagnostics;
using System.Management;
using System.Net.Mail;
using System.Transactions;
using TechBox.Models.CCM;

namespace TechBox.Statics
{
    internal static class Management
    {

        public static bool InvokeMethod(string computerName, string namespacePath, string className, string methodName, Dictionary<string, object> methodArgs)
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.EnablePrivileges = true;

            ManagementScope scope = new ManagementScope($@"\\{computerName}\{namespacePath}", options);

            scope.Connect();

            ManagementPath path = new ManagementPath(className);
            using ManagementClass wmiClass = new ManagementClass(scope, path, null);

            try
            {
                using ManagementBaseObject inParams = wmiClass.GetMethodParameters(methodName);

                foreach (KeyValuePair<string, object> arg in methodArgs)
                {
                    inParams[arg.Key] = arg.Value;
                }

                using ManagementBaseObject outParams = wmiClass.InvokeMethod(methodName, inParams, null);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            //Console.WriteLine("Return Value: " + outParams["ReturnValue"]);

            return true;
        }

        public static bool InvokeMethodeFirst(string computerName, string namespacePath, string className, string methodName, object methodArgument)
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.EnablePrivileges = true;

            ManagementScope scope = new ManagementScope($@"\\{computerName}\{namespacePath}", options);

            scope.Connect();

            ObjectQuery query = new ObjectQuery($"SELECT * FROM {className}");
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            using ManagementObject firstInstance = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            if (firstInstance != null)
            {
                // Créer les arguments pour la méthode
                using ManagementBaseObject inParams = firstInstance.GetMethodParameters(methodName);
                Debug.WriteLine(inParams);
                inParams["ArgumentList"] = methodArgument;

                try
                {
                    using ManagementBaseObject outParams = firstInstance.InvokeMethod(methodName, inParams, null);

                    // Afficher les résultats
                    Debug.WriteLine("Return Value: " + outParams["ReturnValue"]);

                    return true;
                } catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool InvokeCCMAction(string computerName, string id)
        {

            // Source wmi explorer source code : https://github.com/vinaypamnani/wmie2

            // Carries the remote admin credentials from the settings page when they are configured,
            // so triggering a schedule works even when the technician's own account isn't an admin
            // on the target. Falls back to the current Windows session when none is set.
            ConnectionOptions options = RemoteCredentials.CreateConnectionOptions(computerName);

            try
            {
                SmsClient smsClient = new SmsClient(computerName, @"ROOT\ccm", options);

                using ManagementBaseObject inParams = smsClient.SmsClientClass.GetMethodParameters("TriggerSchedule");
                inParams["sScheduleId"] = id;
                using ManagementBaseObject outParams = smsClient.SmsClientClass.InvokeMethod("TriggerSchedule", inParams, null);

                return (outParams != null) ? true : false;

            } catch (Exception ex)
            {
                // Card only reports a count of failures, so the reason - a refused logon with the
                // configured credentials, above all - is only ever visible here.
                Debug.WriteLine($"CCM action {id} on {computerName} failed : {ex.Message}");
                return false;
            }
        }
    }
}
