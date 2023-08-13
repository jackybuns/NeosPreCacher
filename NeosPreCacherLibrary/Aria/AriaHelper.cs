using FrooxEngine.UIX;
using NeosPreCacherLibrary.Models;
using NeosPreCacherLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
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

        public delegate void PrintLineHandler(string line);
        public event PrintLineHandler OnPrintLine;

        public Process Process { get; private set; }

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

        public static bool CheckAria()
        {
            return File.Exists("aria2c.exe");
        }

        public static async Task TryDownloadAria(string downloadUrl)
        {
            if (!AriaHelper.CheckAria())
            {
                Console.WriteLine("aria2c not found, downloading...");
                var client = new HttpClientDownloadWithProgress(downloadUrl, "aria.zip");
                client.ProgressChanged += Utils.Utils.ProgressChanged;
                await client.StartDownload();

                Console.WriteLine("Unzipping aria2c.exe");
                using (var zip = ZipFile.OpenRead("aria.zip"))
                {
                    zip.Entries.First(x => x.Name == "aria2c.exe").ExtractToFile("aria2c.exe");
                }

                File.Delete("aria.zip");

                Console.WriteLine("Finished downloading aria2c");
            }
        }

        public async Task<bool> Download()
        {
            try
            {
                Process = new Process();
                Process.StartInfo = processStartInfo;
                Process.OutputDataReceived += Process_OutputDataReceived;
                Process.Start();
                Process.BeginOutputReadLine();
                await Process.WaitForExitAsync();
                return Process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                OnPrintLine?.Invoke(e.Data);
        }
    }
}
