using NeosPreCacherLibrary.Aria;
using NeosPreCacherLibrary.Models;
using NeosPreCacherLibrary.NeosHelpers;
using NeosPreCacherLibrary.NeosHelpers.Model;

internal class Program
{
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("NeosPreCacher URL [--force]");
    }

    private static NPCSettings? LoadSettings()
    {
        var file = "settings.json";
        var settings = NPCSettings.Load(file);

        if (settings == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(settings.NeosCacheDir))
        {
            settings.NeosCacheDir = NPCSettings.GetDefaultNeosCacheDir();
            settings.Save(file);
        }
        if (string.IsNullOrEmpty(settings.NeosDataDir))
        {
            settings.NeosDataDir = NPCSettings.GetDefaultNeosDataDir();
            settings.Save(file);
        }

        return settings;
    }

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No download URL provided");
            Console.WriteLine();
            PrintUsage();
            return -1;
        }
        bool force = args.Length > 1 && args.Contains("--force");

        var npcSettings = LoadSettings();
        if (npcSettings == null)
        {
            Console.WriteLine("settings.json not found");
            return -1;
        }

        var downloadUrl = args[0];
        var neosDataDir = npcSettings.NeosDataDir;
        var neosCacheDir = npcSettings.NeosCacheDir;
        //var neosDataDir = "C:\\NeosCache\\Data";
        //var neosCacheDir = "C:\\NeosCache\\Cache\\Cache";

        var dbPath = Path.Combine(neosDataDir, "Neos.litedb");
        var settingsPath = Path.Combine(neosDataDir, "Settings.json");


        var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));
        var neosdb = new NeosDBHelper(dbPath, settings.MachineID);

        if (!neosdb.ContainsCacheEntry(downloadUrl) || force)
        {
            Console.WriteLine(force ? "Force download..." : "URL not found in cache, downloading...");
            var file = Guid.NewGuid().ToString();
            var client = new AriaHelper(downloadUrl, new FileInfo(file), npcSettings.NumberOfDownloadConnections);
            client.OnPrintLine += Client_OnPrintLine;

            if (!await client.TryDownloadAria(npcSettings.Aria2cDownloadUrl))
            {
                Console.WriteLine("Could not download aria, aborting...");
                return -1;
            }

            if (await client.Download())
            {
                var targetFile = Path.Combine(neosCacheDir, file);
                File.Move(file, targetFile);
                neosdb.AddCacheEntry(downloadUrl, targetFile);

                Console.WriteLine();
                Console.WriteLine("> Added to Neos cache. Use the same URL in Neos to load the cached version");
                Console.WriteLine("The URL:");
                Console.WriteLine();
                Console.WriteLine(downloadUrl);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Download failed!");
                return -1;
            }
        }
        else
        {
            Console.WriteLine("URL already cached");
        }

        return 0;
    }

    private static void Client_OnPrintLine(string line)
    {
        Console.WriteLine(line);
    }
}
