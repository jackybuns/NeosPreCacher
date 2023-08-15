using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NeosPreCacherLibrary.Aria;
using NeosPreCacherLibrary.Models;
using NeosPreCacherLibrary.NeosHelpers;
using NeosPreCacherLibrary.NeosHelpers.Model;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace NeosPreCacherUI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private const string settingsFile = "settings.json";

    private NPCSettings npcSettings;

    public string DataPath
    {
        get => npcSettings.NeosDataDir;
        set
        {
            if (npcSettings.NeosDataDir != value)
            {
                npcSettings.NeosDataDir = value;
                npcSettings.Save(settingsFile);
                this.RaisePropertyChanged();
            }
        }
    }
    public string CachePath
    {
        get => npcSettings.NeosCacheDir;
        set
        {
            if (npcSettings.NeosCacheDir != value)
            {
                npcSettings.NeosCacheDir = value;
                npcSettings.Save(settingsFile);
                this.RaisePropertyChanged();
            }
        }
    }
    public int NumConnections
    {
        get => npcSettings.NumberOfDownloadConnections;
        set
        {
            if (npcSettings.NumberOfDownloadConnections != value)
            {
                npcSettings.NumberOfDownloadConnections = value;
                npcSettings.Save(settingsFile);
                this.RaisePropertyChanged();
            }
        }
    }

    private string output;
    public string Output
    {
        get { return output; }
        set { this.RaiseAndSetIfChanged(ref output, value); }
    }

    private string downloadUrl;
    public string DownloadUrl
    {
        get { return downloadUrl; }
        set { this.RaiseAndSetIfChanged(ref downloadUrl, value); }
    }

    private bool forceDownload;
    public bool ForceDownload
    {
        get => forceDownload;
        set
        {
            this.RaiseAndSetIfChanged(ref forceDownload, value);
        }
    }


    private int caretIndex;
    public int CaretIndex
    {
        get { return caretIndex; }
        set { this.RaiseAndSetIfChanged(ref caretIndex, value); }
    }


    public ReactiveCommand<Unit, Unit> DownloadCommand { get; }

    public ReactiveCommand<Unit, Unit> SelectCacheDirCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectDataDirCommand { get; }

    public TopLevel TopLevel { get; set; }

    public MainViewModel()
    {
        this.npcSettings = LoadSettings();
        DownloadCommand = ReactiveCommand.CreateFromTask(OnDownloadCommand);
        SelectCacheDirCommand = ReactiveCommand.CreateFromTask(OnSelectCacheDir);
        SelectDataDirCommand = ReactiveCommand.CreateFromTask(OnSelectDataDir);
        Output = "";


        PrintLine("Waiting for Download");
        PrintLine();

    }

    private async Task OnSelectCacheDir()
    {
        if (TopLevel != null)
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = Uri.UriSchemeFile;
            builder.Path = CachePath;
            builder.Host = "";

            var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Pick NeosVR Cache Directory",
                SuggestedStartLocation = await TopLevel.StorageProvider.TryGetFolderFromPathAsync(builder.Uri)
            });
            if (folders.Any())
            {
                CachePath = folders.First().Path.AbsolutePath;
            }
        }
    }
    private async Task OnSelectDataDir()
    {
        if (TopLevel != null)
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = Uri.UriSchemeFile;
            builder.Path = DataPath;
            builder.Host = "";

            var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Pick NeosVR Data Directory",
                SuggestedStartLocation = await TopLevel.StorageProvider.TryGetFolderFromPathAsync(builder.Uri)
            });
            if (folders.Any())
            {
                DataPath = folders.First().Path.AbsolutePath;
            }
        }
    }

    private async Task OnDownloadCommand()
    {
        if (string.IsNullOrEmpty(downloadUrl))
        {
            PrintLine("No URL provided!");
            return;
        }

        var neosDataDir = npcSettings.NeosDataDir;
        var neosCacheDir = npcSettings.NeosCacheDir;

        var dbPath = Path.Combine(neosDataDir, "Neos.litedb");
        if (!File.Exists(dbPath))
        {
            PrintLine("Neos database file could not be found");
            return;
        }
        var settingsPath = Path.Combine(neosDataDir, "Settings.json");
        if (!File.Exists(settingsPath))
        {
            PrintLine("Neos settings file could not be found");
            return;
        }
        if (!Directory.Exists(neosCacheDir))
        {
            PrintLine("Neos cache directory could not be found");
            return;
        }

        var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));
        if (settings == null)
        {
            PrintLine("Could not load the NeosVR settings file");
            return;
        }
        var neosdb = new NeosDBHelper(dbPath, settings.MachineID);

        if (!neosdb.ContainsCacheEntry(downloadUrl) || ForceDownload)
        {
            PrintLine(ForceDownload ? "Forcing download..." : "URL not found in cache, downloading...");
            var file = Guid.NewGuid().ToString();
            var client = new AriaHelper(downloadUrl, new FileInfo(file), npcSettings.NumberOfDownloadConnections);
            client.OnPrintLine += PrintLine;

            // download aria
            if (!AriaHelper.CheckAria())
            {
                PrintLine("aria2 not found, downloading...");
                if (!await client.TryDownloadAria(npcSettings.Aria2cDownloadUrl))
                {
                    PrintLine("abort");
                    return;
                }
                PrintLine();
            }

            var targetFile = Path.Combine(neosCacheDir, file);
            try
            {
                await client.Download();

                File.Move(file, targetFile);
                neosdb.AddCacheEntry(downloadUrl, targetFile);

                PrintLine();
                PrintLine("> Added to Neos cache. Use the same URL in Neos to load the cached version");
                PrintLine("The URL:");
                PrintLine();
                PrintLine(downloadUrl);
                PrintLine();
            }
            catch (Exception e)
            {
                PrintLine(e.Message);
                PrintLine("An error occurred while downloading");
            }
            finally
            {
                client.OnPrintLine -= PrintLine;
            }
        }
        else
        {
            PrintLine("URL already cached.");
            PrintLine("Use 'Force Download' to download again and replace the cache entry.");
        }
    }

    private void PrintLine(string message = "")
    {
        Output += message + "\n";
        CaretIndex = Output.Length;
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
}
