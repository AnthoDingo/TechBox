using System.Diagnostics;
using System.Numerics;

namespace TechBox.Statics
{
    public static class Converter
    {

        public static string BytesToString(BigInteger byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            BigInteger bytes = BigInteger.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(BigInteger.Log(bytes, 1024)));
            double num = Math.Round((double)bytes / Math.Pow(1024, place), 0);
            return $"{(Math.Sign((long)byteCount) * num).ToString()} {suf[place]}";
        }

        public static Int64? ActiveDirectoryTimeStampToInt64(Object timestamp)
        {
            if(timestamp == null)
                return null;

            try
            {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                int highPart = (Int32)timestamp.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, timestamp, null);

                int lowPart = (Int32)timestamp.GetType().InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, timestamp, null);
#pragma warning restore CS8605 // Unboxing a possibly null value.

                return ((long)highPart << 32) | (uint)lowPart;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ActiveDirectoryTimeStampToInt64 | {ex.Message}", "ERROR");
                return null;
            }

            //var largeInt = (IADsLargeInteger)timestamp;
            //long value = ((long)largeInt.HighPart << 32) + largeInt.LowPart;
            //return value;

            ////Old method
            //var highPart = (Int32)timestamp.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, timestamp, null);

            //var lowPart = (Int32)timestamp.GetType().InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, timestamp, null);

            //return (highPart * ((Int64)UInt32.MaxValue + 1)) + lowPart;
        }
    }
}
