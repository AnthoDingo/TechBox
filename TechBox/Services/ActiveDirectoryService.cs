using System.DirectoryServices;
using TechBox.Databases;
using TechBox.Models.ActiveDirectory;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;
using TechBox.Statics;

namespace TechBox.Services
{
    public class ActiveDirectoryService : IActiveDirectory, IDisposable
    {
        private DirectoryEntry _directoryEntry;
        private DirectoryEntry _globalDirectoryEntry;

        //private PrincipalContext _domainContext = new PrincipalContext(ContextType.Domain, Environment.GetEnvironmentVariable("USERDOMAIN"));

        private List<string> _cachedComputers = new List<string>();
        private List<string> _cachedUsers = new List<string>();
        private DateTime? _cacheComputersExpiry;
        private DateTime? _cacheUsersExpiry;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private readonly SemaphoreSlim _computerLock = new(1, 1);
        private readonly SemaphoreSlim _userLock = new(1, 1);

        public ActiveDirectoryService()
        {
            using(SQLiteContext db = new SQLiteContext())
            {
                string? customLdapPath = db.Settings.FirstOrDefault(s => s.Name.Equals("ldap_path"))?.Value;
                if (!string.IsNullOrEmpty(customLdapPath))
                {
                    _directoryEntry = new DirectoryEntry(customLdapPath);
                    string baseDn = "LDAP://" + string.Join(",", customLdapPath.Replace("LDAP://", "").Split(',').Where(p => p.StartsWith("DC=", StringComparison.OrdinalIgnoreCase)));
                    _globalDirectoryEntry = new DirectoryEntry(baseDn);
                }
                //else
                //{
                //    throw new Exception("LDAP path is not configured in the database. Please set the 'ldap_path' setting to a valid LDAP path.");
                //}
            }
        }

        public void SetLdapPath(string customLdapPath)
        {
            _directoryEntry = new DirectoryEntry(customLdapPath);
            string baseDn = "LDAP://" + string.Join(",", customLdapPath.Replace("LDAP://", "").Split(',').Where(p => p.StartsWith("DC=", StringComparison.OrdinalIgnoreCase)));
            _globalDirectoryEntry = new DirectoryEntry(baseDn);
        }

        #region Cache

        private bool IsCacheValid(DateTime? expiry) => expiry.HasValue && DateTime.UtcNow < expiry.Value;

        public void InvalidateCache()
        {
            InvalidateComputersCache();
            InvalidateUsersCache();
        }

        public void InvalidateComputersCache()
        {
            _cachedComputers.Clear();
            _cacheComputersExpiry = null;
        }

        public void InvalidateUsersCache()
        {
            _cachedUsers.Clear();
            _cacheUsersExpiry = null;
        }

        #endregion

        #region Helpers

        private List<T> SearchDirectory<T>(DirectoryEntry root,string filter,string[] properties, Func<SearchResult, T?> selector)
        {
            using DirectorySearcher searcher = new DirectorySearcher(root)
            {
                Filter = filter,
                CacheResults = false
            };
            searcher.PropertiesToLoad.AddRange(properties);

            List<T> results = new List<T>();
            using SearchResultCollection searchResults = searcher.FindAll();

            foreach (SearchResult result in searchResults)
            {
                T? value = selector(result);
                if (value is not null)
                    results.Add(value);
            }

            return results;
        }

        private T? GetValue<T>(SearchResult result, string name) => GetValue<T>(result.GetDirectoryEntry(), name);

        private T? GetValue<T>(DirectoryEntry entry, string name)
        {
            if (entry.Properties[name] == null)
                return default;

            return typeof(T).Name switch
            {
                "Int64" => (T?)Convert.ChangeType(
                    Converter.ActiveDirectoryTimeStampToInt64(entry.Properties[name].Value), typeof(T)),
                _ => entry.Properties[name].Value is T val ? val : default
            };
        }

        #endregion

        #region Computers

        public async Task<IEnumerable<string>> GetAllComputersAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && IsCacheValid(_cacheComputersExpiry))
                return _cachedComputers;

            await _computerLock.WaitAsync();
            try
            {
                if (!forceRefresh && IsCacheValid(_cacheComputersExpiry))
                    return _cachedComputers;

                _cachedComputers = SearchDirectory(
                    _directoryEntry, 
                    "(objectCategory=Computer)", 
                    ["objectCategory", "cn"], 
                    result =>
                    {
                        using DirectoryEntry entry = result.GetDirectoryEntry();
                        return entry.Properties["cn"].Value?.ToString();
                    }
                ).Order().ToList();
                _cacheComputersExpiry = DateTime.UtcNow.Add(CacheDuration);
            }
            finally
            {
                _computerLock.Release();
            }
            return _cachedComputers;
        }

        public IEnumerable<string> GetAllComputers(bool forceRefresh = false)
        {
            if (!forceRefresh && IsCacheValid(_cacheComputersExpiry))
                return _cachedComputers;

            DirectorySearcher searcher = new DirectorySearcher(_directoryEntry);
            searcher.PropertiesToLoad.AddRange(new string[] { "objectCategory", "cn" });
            searcher.CacheResults = false;
            searcher.Filter = $"(objectCategory=Computer)";

            List<string> computers = new List<string>();
            foreach (SearchResult result in searcher.FindAll())
            {
                computers.Add(result.GetDirectoryEntry().Properties["cn"].Value.ToString());
            }

            _cachedComputers = computers.Order().ToList();
            _cacheComputersExpiry = DateTime.UtcNow.Add(CacheDuration);

            //return computers.Order();
            return _cachedComputers;
        }

        public IEnumerable<string> SearchComputers(string computerName)
        {
            return SearchDirectory(
                _directoryEntry,
                $"(&(objectCategory=Computer)(cn=*{computerName}*))",
                ["objectCategory", "cn"],
                result =>
                {
                    using DirectoryEntry entry = result.GetDirectoryEntry();
                    return entry.Properties["cn"].Value?.ToString();
                }
            ).Order();
        }

        public Computer GetComputer(string computerName)
        {
            using DirectorySearcher searcher = new DirectorySearcher(_directoryEntry)
            {
                Filter = $"(&(objectCategory=Computer)(cn={computerName}*))",
                CacheResults = false
            };

            SearchResult result = searcher.FindOne() ?? throw new KeyNotFoundException($"Computer '{computerName}' not found.");

            using DirectoryEntry entry = result.GetDirectoryEntry();
            return new Computer { Name = entry.Properties["cn"].Value?.ToString(), DN = entry.Path };
        }
        #endregion

        #region Users

        public async Task<IEnumerable<string>> GetAllUsersAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && IsCacheValid(_cacheUsersExpiry))
                return _cachedUsers;

            await _userLock.WaitAsync();

            try
            {
                if (!forceRefresh && IsCacheValid(_cacheUsersExpiry))
                    return _cachedUsers;

                _cachedUsers = SearchDirectory(
                    _directoryEntry,
                    "(&(objectCategory=Person))",
                    ["objectCategory", "samAccountName"],
                    result => GetValue<string>(result, "samAccountName")
                ).Order().ToList();

                _cacheUsersExpiry = DateTime.UtcNow.Add(CacheDuration);
            }
            finally
            {
                _userLock.Release();
            }
            return _cachedUsers;
        }

        public IEnumerable<string> GetAllUsers(bool forceRefresh = false)
        {
            if (_cachedUsers.Count > 0)
                return _cachedUsers;

            DirectorySearcher searcher = new DirectorySearcher(_directoryEntry);
            searcher.PropertiesToLoad.AddRange(new string[] { "objectCategory", "samAccountName" });

            searcher.CacheResults = false;
            searcher.Filter = $"(&(objectCategory=Person))";

            List<string> users = new List<string>();
            foreach (SearchResult result in searcher.FindAll())
            {
                //users.Add(result.GetDirectoryEntry().Properties["cn"].Value.ToString());
                users.Add(GetValue<string>(result, "samAccountName"));
            }

            _cachedUsers = users.Order().ToList();
            _cacheUsersExpiry = DateTime.UtcNow.Add(CacheDuration);

            return _cachedUsers;
        }

        public IEnumerable<string> SearchUsers(string username)
        {
            return SearchDirectory(
                _directoryEntry,
                $"(&(objectCategory=Person)(samAccountName=*{username}*))",
                ["objectCategory", "samAccountName"],
                result => GetValue<string>(result, "samAccountName")
            ).Order();
        }

        public User GetUser(string username, bool globalDirectory = false) => new User(username, globalDirectory ? _globalDirectoryEntry : _directoryEntry);

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _directoryEntry.Dispose();
            _globalDirectoryEntry.Dispose();
            //_domainContext.Dispose();
            _computerLock.Dispose();
            _userLock.Dispose();
        }

        #endregion
    }
}
