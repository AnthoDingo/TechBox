using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Enums;

namespace TechBox.Services.Contracts
{
    public interface ICopyData
    {
        public Task CopyFolder(string source, string destination, List<UserFolder> folders);
        public Task CopyFolder(string source, string destination, UserFolder folder);
    }
}
