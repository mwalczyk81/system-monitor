using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemMonitor
{
    public class Constants
    {
        public class Polling
        {
            public const int DefaultIntervalSeconds = 1;
            public const int MaxDataPoints = 100;
            public static readonly int[] AvailableIntervals = [1, 5, 10];
        }

        public class ChartFormats
        {
            public class YAxis
            {
                public const string Percent = "function(val) { return val + '%'; }";
                public const string BytesPerSecond = @"
                                                        function(val) {
                                                            if (val >= 1048576) {
                                                                // Convert to megabytes per second
                                                                return (val / 1048576).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' MB/s';
                                                            } else if (val >= 1024) {
                                                                // Convert to kilobytes per second
                                                                return (val / 1024).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' KB/s';
                                                            } else {
                                                                // Bytes per second
                                                                return val.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' bytes/s';
                                                            }
                                                        }";

            }
        }
    }
}
