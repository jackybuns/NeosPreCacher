﻿using FrooxEngine;
using FrooxEngine.LogiX.Math.Binary;
using LiteDB;
using NeosPreCacher;
using NeosPreCacher.Aria;
using NeosPreCacher.NeosHelpers;
using NeosPreCacher.NeosHelpers.Model;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using static NeosAssets;

internal class Program
{
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("NeosPreCacher URL [--force]");
    }
    private static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No download URL provided");
            Console.WriteLine();
            PrintUsage();
            return;
        }
        bool force = args.Length > 1 && args.Contains("--force");

        var downloadUrl = args[0];
        var neosDataDir = "C:\\NeosCache\\Data";
        var neosCacheDir = "C:\\NeosCache\\Cache\\Cache";

        var dbPath = Path.Combine(neosDataDir, "Neos.litedb");
        var settingsPath = Path.Combine(neosDataDir, "Settings.json");


        var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));
        var neosdb = new NeosDBHelper(dbPath, settings.MachineID);

        if (!neosdb.ContainsCacheEntry(downloadUrl) || force)
        {
            Console.WriteLine(force ? "Force download..." : "URL not found in cache, downloading...");
            var file = Guid.NewGuid().ToString();
            var client = new AriaHelper(downloadUrl, new FileInfo(file));
            if (client.Download())
            {

                var targetFile = Path.Combine(neosCacheDir, file);
                File.Move(file, targetFile);
                neosdb.AddCacheEntry(downloadUrl, targetFile);

                Console.WriteLine("Added to Neos cache. Use the same URL in Neos to load the cached version");
            }
            else
            {
                Console.WriteLine("Download failed!");
            }
        }
        else
        {
            Console.WriteLine("URL already cached");
        }

    }

    private static string generateProgressBar(double percent, int barLength = 50)
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

    private static void Client_ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        var p = generateProgressBar(progressPercentage.Value / 100.0);
        Console.WriteLine($"{p} {totalBytesDownloaded}/{totalFileSize} - {progressPercentage}%");
    }
}