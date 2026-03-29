using RoboSharp;
using TechBox.Enums;
using TechBox.Services.Contracts;
using TechBox.Statics;

namespace TechBox.Services
{
    public class CopyDataService : ICopyData
    {
        private Dictionary<UserFolder, double> Percents = new Dictionary<UserFolder, double>();
        private Dictionary<UserFolder, long> CopiedFiles = new Dictionary<UserFolder, long>();
        private Dictionary<UserFolder, long> TotalFiles = new Dictionary<UserFolder, long>();


        public Task CopyFolder(string source, string destination, List<UserFolder> folders)
        {
            foreach(UserFolder folder in folders)
            {
                CopyFolder(source, destination, folder).Wait();
            }

            return Task.CompletedTask;
        }

        public Task CopyFolder(string source, string destination, UserFolder folder)
        {
            RoboCommand _roboCommand = new RoboCommand();
            _roboCommand.CopyOptions.Source = $"{source}\\{folder.GetStringValue()}";
            _roboCommand.CopyOptions.Destination = $"{destination}\\{folder.GetStringValue()}";
            _roboCommand.CopyOptions.Mirror = true;
            _roboCommand.CopyOptions.CopySubdirectories = true;
            _roboCommand.LoggingOptions.VerboseOutput = true;


            CopiedFiles.Add(folder, 0);
            Percents.Add(folder, 0);

            _roboCommand.OnCommandCompleted += (sender, e) =>
            {
                
            };

            _roboCommand.OnProgressEstimatorCreated += (sender, e) =>
            {
                e.ResultsEstimate.FilesStatistic.OnTotalChanged += (sender, e) =>
                {
                    TotalFiles.Add(folder, e.Difference);
                };
            };

            _roboCommand.OnCopyProgressChanged += (sender, e) => 
            {
                CopiedFiles[folder] += 1;
                Percents[folder] = (double)CopiedFiles[folder] / TotalFiles[folder] * 100;
            };

            _roboCommand.Start().Wait();

            return Task.CompletedTask;
        }
    }
}
