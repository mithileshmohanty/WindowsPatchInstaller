using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace WindowsPatchInstallerUI.Models
{
    public class SecurityRelease
    {
        public ObjectId _id { get; set; }
        public IEnumerable<ObjectId> Patch_id { get; set; }
        public ObjectId os_id { get; set; }
        public string Description { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ReleaseDate { get; set; }
    }
}
