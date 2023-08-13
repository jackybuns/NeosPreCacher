using FrooxEngine;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosPreCacherLibrary.NeosHelpers
{
    public class NeosDBHelper
    {

        string dbfile;
        string machineId;
        string connectionString;

        public NeosDBHelper(string dbfile, string machineId)
        {
            this.dbfile = dbfile;
            this.machineId = machineId;
            connectionString = DbHelper.ProcessConnection(dbfile, machineId);
        }

        public bool ContainsCacheEntry(string uri)
        {
            using (var neosDB = new LiteDatabase(connectionString))
            {
                var assets = neosDB.GetCollection<AssetRecord>("Assets");
                assets.EnsureIndex((AssetRecord r) => r.url, unique: true);
                assets.EnsureIndex((AssetRecord r) => r.signature);

                var entry = assets.FindOne(r => r.url == uri);
                return entry != null;
            }
        }

        public void AddCacheEntry(string url, string path)
        {
            using (var neosDB = new LiteDatabase(connectionString))
            {
                var assets = neosDB.GetCollection<AssetRecord>("Assets");
                assets.EnsureIndex((AssetRecord r) => r.url, unique: true);
                assets.EnsureIndex((AssetRecord r) => r.signature);

                var entry = assets.FindOne(r => r.url == url);
                if (entry == null)
                {
                    entry = new AssetRecord()
                    {
                        url = url,
                        path = path,
                    };
                    assets.Insert(entry);
                }
                else
                {
                    if (File.Exists(entry.path))
                        File.Delete(entry.path);
                    entry.path = path;
                    assets.Update(entry);
                }
            }
        }
    }
}
