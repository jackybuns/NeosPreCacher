using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosPreCacher.NeosHelpers.Model
{
    public class SettingsModel
    {
        public int Version { get; set; }
        public string MachineID { get; set; }

        public SettingsModel(int version, string machineID)
        {
            Version = version;
            MachineID = machineID;
        }
    }
}
