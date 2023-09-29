using System;

namespace Snowlight
{
    public static class UnixTimestamp
    {
        public static double GetCurrent()
        {
            TimeSpan ts = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }

        public static double ConvertToUnixTimestamp(DateTime ToConvert)
        {
            TimeSpan ts = (ToConvert - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }

        public static DateTime GetDateTimeFromUnixTimestamp(double Timestamp)
        {
            DateTime DT = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DT = DT.AddSeconds(Timestamp);
            return DT;
        }

        public static TimeSpan ElapsedTime(double Timestamp)
        {
            double DoubleTS = GetCurrent() - Timestamp;
            TimeSpan TS = DateTime.Now - GetDateTimeFromUnixTimestamp(DoubleTS);
            return TS;
        }
    }
}
