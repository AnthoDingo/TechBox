using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Models.ActiveDirectory;
using TechBox.Models.Hardware;

namespace TechBox.Services.Contracts
{
    public interface IActiveDirectory
    {
        // Computers
        Task<IEnumerable<string>> GetAllComputersAsync(bool forceRefresh = false);
        IEnumerable<string> SearchComputers(string computerName);
        Computer GetComputer(string computerName);

        // Users
        Task<IEnumerable<string>> GetAllUsersAsync(bool forceRefresh = false);
        IEnumerable<string> SearchUsers(string username);
        User GetUser(string username, bool globalDirectory = false);

        // Groups
        IEnumerable<GroupMember> GetGroupMembers(string groupDistinguishedName);

        // Cache
        void InvalidateCache();
        void InvalidateComputersCache();
        void InvalidateUsersCache();

        // Configuration
        void SetLdapPath(string customLdapPath);
    }
}
