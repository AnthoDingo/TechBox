using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Attributes;
using TechBox.Enums;

namespace TechBox.Statics
{
    public static class UserFolderExtensions
    {
        public static string GetStringValue(this UserFolder folder)
        {
            var fieldInfo = folder.GetType().GetField(folder.ToString());
            var attribute = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribute?.Length > 0 ? attribute[0].Value : folder.ToString();
        }
    }
}
