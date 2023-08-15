using NeosPreCacherLibrary.Utils;
using System.Diagnostics;
using System.IO.Compression;

namespace NeosPreCacherLibrary.Aria
{
    public class AriaHelper
    {
        public delegate void PrintLineDelegate(string text);

        private string url;
        private FileInfo targetFile;
        private ProcessStartInfo processStartInfo;

        public delegate void PrintLineHandler(string line);
        public event PrintLineHandler OnPrintLine;

        public Process Process { get; private set; }

        public AriaHelper(string url, FileInfo targetfile, int numberOfConnections = 4)
        {
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

        public async Task<bool> TryDownloadAria(string downloadUrl)
        {
            if (!AriaHelper.CheckAria())
            {
                try
                {
                    OnPrintLine?.Invoke("aria2c not found, downloading...");
                    var client = new HttpClientDownloadWithProgress(downloadUrl, "aria.zip");
                    client.ProgressChanged += Client_ProgressChanged;
                    await client.StartDownload();

                    OnPrintLine?.Invoke("Unzipping aria2c.exe");
                    using (var zip = ZipFile.OpenRead("aria.zip"))
                    {
                        zip.Entries.First(x => x.Name == "aria2c.exe").ExtractToFile("aria2c.exe");
                    }

                    File.Delete("aria.zip");
                    OnPrintLine?.Invoke("Finished downloading aria2c");
                    return true;
                }
                catch (Exception ex)
                {
                    OnPrintLine?.Invoke("Error while downloading aria");
                    OnPrintLine?.Invoke(ex.Message);
                    return false;
                }
            }
            return true;
        }

        private void Client_ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            if (progressPercentage.HasValue)
            {
                var p = Utils.Utils.GenerateProgressBar(progressPercentage.Value / 100.0);
                OnPrintLine?.Invoke($"{p} {totalBytesDownloaded}/{totalFileSize} - {progressPercentage}%");
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
                OnPrintLine?.Invoke(ex.Message);
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
