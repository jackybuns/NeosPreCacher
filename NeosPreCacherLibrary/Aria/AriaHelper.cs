using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosPreCacherLibrary.Aria
{
    public class AriaHelper
    {
        private string url;
        private FileInfo targetFile;
        private ProcessStartInfo processStartInfo;

        public AriaHelper(string url, FileInfo targetfile, int numberOfConnections = 4)
        {
            if (!CheckAria())
                throw new Exception("aria2c.exe could not be found");

            this.url = url;
            this.targetFile = targetfile;
            processStartInfo = new ProcessStartInfo()
            {
                FileName = "aria2c.exe",
                Arguments = $"-x{numberOfConnections} {url} -d {targetfile.Directory.FullName} -o {targetFile.Name}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
        }

        public bool CheckAria()
        {
            return File.Exists("aria2c.exe");
        }

        public bool Download()
        {
            try
            {
                var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
