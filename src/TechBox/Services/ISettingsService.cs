using TechBox.Models;

namespace TechBox.Services;

public interface ISettingsService
{
    AppSettings Load();

    void Save(AppSettings settings);
}
