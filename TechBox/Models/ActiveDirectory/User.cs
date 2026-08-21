using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using TechBox.Enums;
using TechBox.Statics;

namespace TechBox.Models.ActiveDirectory
{
    public class User : IDisposable
    {
        private string _username;
        private DirectoryEntry _directoryEntry;
        private PrincipalContext _domainContext = new PrincipalContext(ContextType.Domain, Environment.GetEnvironmentVariable("USERDOMAIN"));

        private DirectoryEntry _directoryUser;
        private UserPrincipal _userPrincipal;

        public User(string username, DirectoryEntry directoryEntry)
        {
            _username = username;
            _directoryEntry = directoryEntry;
        }

        #region Lazy loaders

        private DirectoryEntry DirectoryUser
        {
            get
            {
                if (_directoryUser is not null)
                    return _directoryUser;

                using DirectorySearcher searcher = new DirectorySearcher(_directoryEntry)
                {
                    CacheResults = false,
                    Filter = $"(&(objectCategory=Person)(samAccountName={_username}))"
                };

                searcher.PropertiesToLoad.AddRange(new string[]
                {
                    "samAccountName",
                    "gecos",
                    "memberOf",
                    "displayName",
                    "description",
                    "mail",
                    "telephoneNumber",
                    "mobile",
                    "homeDirectory",
                    "lockoutTime",
                    "userAccountControl",
                    "lastLogon",
                    "pwdLastSet",
                    "accountExpires"
                });

                SearchResult result = searcher.FindOne() ?? throw new KeyNotFoundException($"User '{_username}' not found."); ;
                _directoryUser = result.GetDirectoryEntry();

                return _directoryUser;
            }
        }

        private UserPrincipal UserPrincipal => _userPrincipal ??= UserPrincipal.FindByIdentity(_domainContext, _username) ?? throw new KeyNotFoundException($"UserPrincipal '{_username}' not found.");

        #endregion


        #region Helpers

        private string GetStringProperty(string propertyName)
        {
            try
            {
                return DirectoryUser.Properties[propertyName].Value?.ToString()?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{propertyName} | {ex.Message}", "ERROR");
                return string.Empty;
            }
        }

        private DateTime? GetDateTimeProperty(string propertyName)
        {
            try
            {
                object? value = DirectoryUser.Properties[propertyName].Value;
                if (value is null) return null;

                long? ticks = Converter.ActiveDirectoryTimeStampToInt64(value);
                
                if (ticks is null || ticks <= 0 || ticks == long.MaxValue) return null;

                return DateTime.FromFileTime(ticks.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{propertyName} | {ex.Message}", "ERROR");
                return null;
            }
        }

        #endregion

        #region Properties

        public string SID
        {
            get
            {
                NTAccount obj = new NTAccount(Environment.GetEnvironmentVariable("USERDOMAIN"), _username);
                return (string)obj.Translate(typeof(SecurityIdentifier)).Value;
            }
        }
        public string samAccountName => GetStringProperty("samAccountName");
        public string FullName => GetStringProperty("displayName");
        public string DN => DirectoryUser.Path;
        public string GecosID => GetStringProperty("gecos");
        public string Description => GetStringProperty("description");
        public string Mail => GetStringProperty("mail");
        public string Phone => GetStringProperty("telephoneNumber");

        public string Mobile => GetStringProperty("mobile");

        public string HomeDirectory => GetStringProperty("homeDirectory");
        public bool? IsLocked
        {
            get
            {
                try
                {
                    return UserPrincipal.IsAccountLockedOut();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"IsLocked | {ex.Message}", "ERROR");
                    return null;
                }
            }
        }
        public bool? IsDisabled
        {
            get
            {
                try
                {
                    return !UserPrincipal.Enabled;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"IsDisabled | {ex.Message}", "ERROR");
                    return null;
                }
            }
        }

        public DateTime? LastLogon => GetDateTimeProperty("lastLogon");

        public DateTime? PasswordSet => GetDateTimeProperty("pwdLastSet");

        public DateTime? PasswordExpire
        {
            get
            {
                try
                {
                    return UserPrincipal.PasswordNeverExpires ? null : UserPrincipal.AccountExpirationDate;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PasswordExpire | {ex.Message}", "ERROR");
                    return null;
                }
            }
        }
        public DateTime? AccountExpire => GetDateTimeProperty("accountExpires");

        public IEnumerable<Group> MemberOf
        {
            get
            {
                List<Group> result = new List<Group>();
              

                foreach (string groupDN in DirectoryUser.Properties["memberOf"])
                {
                    using DirectoryEntry group = new DirectoryEntry($"LDAP://{groupDN}");
                    try
                    {
                        if (group.Properties["groupType"].Value is null)
                            continue;

                        int groupType = (int)group.Properties["groupType"].Value;
                        string groupName = group.Properties["name"].Value?.ToString() ?? string.Empty;

                        result.Add((groupType & unchecked((int)0x80000000)) != 0 ? 
                            new Group { Name = groupName, Type = GroupType.Security } : 
                            new Group { Name = groupName, Type = GroupType.Distribution, Email = group.Properties["mail"].Value?.ToString() ?? string.Empty }
                        );
                    }
                    catch
                    {
                        continue;
                    }
                }

                return result;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _directoryUser?.Dispose();
            _userPrincipal?.Dispose();
            _domainContext.Dispose();
        }

        #endregion
    }
}
