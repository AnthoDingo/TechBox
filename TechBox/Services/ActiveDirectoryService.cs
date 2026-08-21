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

        private T? GetValue<T>(SearchResult result, string name)
        {
            if (!result.Properties.Contains(name) || result.Properties[name].Count == 0)
                return default;

            object rawValue = result.Properties[name][0];

            return typeof(T).Name switch
            {
                "Int64" => (T?)Convert.ChangeType(
                    Converter.ActiveDirectoryTimeStampToInt64(rawValue), typeof(T)),
                _ => rawValue is T val ? val : default
            };
        }

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

            await _computerLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!forceRefresh && IsCacheValid(_cacheComputersExpiry))
                    return _cachedComputers;

                _cachedComputers = SearchDirectory(
                    _directoryEntry,
                    "(objectCategory=Computer)",
                    ["objectCategory", "cn"],
                    result => GetValue<string>(result, "cn")
                ).Order().ToList();
                _cacheComputersExpiry = DateTime.UtcNow.Add(CacheDuration);
            }
            finally
            {
                _computerLock.Release();
            }
            return _cachedComputers;
        }

        public IEnumerable<string> GetAllComputers(bool forceRefresh = false) =>
            GetAllComputersAsync(forceRefresh).GetAwaiter().GetResult();

        public IEnumerable<string> SearchComputers(string computerName)
        {
            return SearchDirectory(
                _directoryEntry,
                $"(&(objectCategory=Computer)(cn=*{computerName}*))",
                ["objectCategory", "cn"],
                result => GetValue<string>(result, "cn")
            ).Order();
        }

        public Computer GetComputer(string computerName)
        {
            using DirectorySearcher searcher = new DirectorySearcher(_directoryEntry)
            {
                Filter = $"(&(objectCategory=Computer)(cn={computerName}*))",
                CacheResults = false
            };
            searcher.PropertiesToLoad.AddRange(["cn"]);

            SearchResult result = searcher.FindOne() ?? throw new KeyNotFoundException($"Computer '{computerName}' not found.");

            return new Computer { Name = GetValue<string>(result, "cn"), DN = result.Path };
        }
        #endregion

        #region Users

        public async Task<IEnumerable<string>> GetAllUsersAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && IsCacheValid(_cacheUsersExpiry))
                return _cachedUsers;

            await _userLock.WaitAsync().ConfigureAwait(false);

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

        public IEnumerable<string> GetAllUsers(bool forceRefresh = false) =>
            GetAllUsersAsync(forceRefresh).GetAwaiter().GetResult();

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
