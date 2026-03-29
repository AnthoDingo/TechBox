using System.Numerics;
using TechBox.Statics;

namespace TechBox.Models.Hardware
{
    public class LogicalDisk
    {
        public string Name { get; set; } = string.Empty;
        public string FS { get; set; } = string.Empty;
        public int MaximumComponentLength { get; set; } = 255;
        public BigInteger Size { get;  set; }
        public string SizeHuman
        {
            get
            {
                return Converter.BytesToString(Size);
            }
        }
        public BigInteger FreeSpace { get; set; }
        public string FreeSpaceHuman { 
            get 
            {
                return Converter.BytesToString(FreeSpace);
            }
        }
        public double UsedPercent
        {
            get
            {
                //double fSpace = ((double)FreeSpace * 100) / (double)Size;
                return (100 - FreePercent);
            }
        }
        public double FreePercent
        {
            get
            {
                double fSpace = ((double)FreeSpace * 100) / (double)Size;
                return Math.Round(fSpace, 2);
            }
        }
    }
}
