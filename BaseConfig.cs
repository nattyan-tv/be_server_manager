using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bedrock_server_manager
{
    internal class BaseConfig
    {
        public string name { get; set; }
        public string location { get; set; }
        public string seed { get; set; }
        public string update { get; set; }
        public string backup { get; set; }
        public string backupTime { get; set; }
        public bool autoupdate { get; set; }
        public bool autobackup { get; set; }
        public string botToken { get; set; }
        public string botPrefix { get; set; }
    }
}
