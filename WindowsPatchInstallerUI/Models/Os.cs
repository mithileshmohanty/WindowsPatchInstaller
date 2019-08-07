using MongoDB.Bson;
using System.Collections.Generic;

namespace WindowsPatchInstallerUI.Models
{
    public class Os
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Edition { get; set; }
        public string Architecture { get; set; }
        public string Build { get; set; }
    }
}
