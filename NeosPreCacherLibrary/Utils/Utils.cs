using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosPreCacherLibrary.Utils
{
    public class Utils
    {
        public static string GenerateProgressBar(double percent, int barLength = 50)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < barLength; i++)
            {
                double barperc = (double)i / barLength;
                sb.Append(barperc <= percent ? "=" : " ");
            }

            sb.Append("]");

            return sb.ToString();
        }

        public static void ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            var p = GenerateProgressBar(progressPercentage.Value / 100.0);
            Console.WriteLine($"{p} {totalBytesDownloaded}/{totalFileSize} - {progressPercentage}%");
        }
    }
}
