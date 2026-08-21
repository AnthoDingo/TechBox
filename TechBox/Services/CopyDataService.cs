using System.Collections.Concurrent;
using RoboSharp;
using TechBox.Enums;
using TechBox.Services.Contracts;
using TechBox.Statics;

namespace TechBox.Services
{
    public class CopyDataService : ICopyData
    {
        private readonly ConcurrentDictionary<UserFolder, double> Percents = new();
        private readonly ConcurrentDictionary<UserFolder, long> CopiedFiles = new();
        private readonly ConcurrentDictionary<UserFolder, long> TotalFiles = new();


        public Task CopyFolder(string source, string destination, List<UserFolder> folders)
        {
            return Task.WhenAll(folders.Select(folder => CopyFolder(source, destination, folder)));
        }

        public async Task CopyFolder(string source, string destination, UserFolder folder)
        {
            RoboCommand _roboCommand = new RoboCommand();
            _roboCommand.CopyOptions.Source = $"{source}\\{folder.GetStringValue()}";
            _roboCommand.CopyOptions.Destination = $"{destination}\\{folder.GetStringValue()}";
            _roboCommand.CopyOptions.Mirror = true;
            _roboCommand.CopyOptions.CopySubdirectories = true;
            _roboCommand.LoggingOptions.VerboseOutput = true;


            CopiedFiles[folder] = 0;
            Percents[folder] = 0;

            _roboCommand.OnCommandCompleted += (sender, e) =>
            {

            };

            _roboCommand.OnProgressEstimatorCreated += (sender, e) =>
            {
                e.ResultsEstimate.FilesStatistic.OnTotalChanged += (sender, e) =>
                {
                    TotalFiles[folder] = e.Difference;
                };
            };

            _roboCommand.OnCopyProgressChanged += (sender, e) =>
            {
                long copied = CopiedFiles.AddOrUpdate(folder, 1, (_, current) => current + 1);
                if (TotalFiles.TryGetValue(folder, out long total) && total > 0)
                    Percents[folder] = (double)copied / total * 100;
            };

            await _roboCommand.Start();
        }
    }
}
