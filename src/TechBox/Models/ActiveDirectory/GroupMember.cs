using Wpf.Ui.Controls;

namespace TechBox.Models.ActiveDirectory
{
    public class GroupMember
    {
        public string Name { get; set; } = string.Empty;

        public string SamAccountName { get; set; } = string.Empty;

        public bool IsGroup { get; set; }

        public SymbolRegular Icon => IsGroup ? SymbolRegular.PeopleTeam24 : SymbolRegular.Person24;
    }
}
