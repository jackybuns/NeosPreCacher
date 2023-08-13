using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeosPreCacherLibrary.Models
{
    public class NPCSettings
    {
        public string NeosCacheDir { get; set; }
        public string NeosDataDir { get; set; }
        public int NumberOfDownloadConnections { get; set; }
        public string Aria2cDownloadUrl { get; set; }

        public NPCSettings(string neosCacheDir, string neosDataDir, int numberOfDownloadConnections, string aria2cDownloadUrl)
        {
            NeosCacheDir = neosCacheDir;
            NeosDataDir = neosDataDir;
            NumberOfDownloadConnections = numberOfDownloadConnections;
            Aria2cDownloadUrl = aria2cDownloadUrl;
        }

        public static string GetDefaultNeosDataDir()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
            return Path.Combine(appdata, "Solirax", "NeosVR");
        }

        public static string GetDefaultNeosCacheDir()
        {
            var tempdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(tempdata, "Temp", "Solirax", "NeosVR", "Cache");
        }

        public static NPCSettings? Load(string filename)
        {
            if (!File.Exists(filename))
                return null;
            return JsonSerializer.Deserialize<NPCSettings>(File.ReadAllText(filename));
        }
        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
