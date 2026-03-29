using TechBox.Enums;
using Wpf.Ui.Controls;

namespace TechBox.Models.ActiveDirectory
{
    public class Group
    {
        public string Name { get; set; } = string.Empty;

        public GroupType Type { get; set; }

        public string Email { get; set; } = string.Empty;

        public SymbolIcon Icon
        {
            get
            {
                SymbolRegular icon;
                switch (Type)
                {
                    case GroupType.Security:
                        icon = SymbolRegular.Shield24;
                        break;
                    case GroupType.Distribution:
                        icon = SymbolRegular.MailRead24;
                        break;
                    default:
                        throw new Exception("Invalid type");
                }

                return new SymbolIcon(icon);
            }
        }
    }
}
