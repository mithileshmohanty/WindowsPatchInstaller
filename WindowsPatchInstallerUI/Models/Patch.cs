using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WindowsPatchInstallerUI.Models
{
        public class Patch
        {
            public ObjectId _id { get; set; }
            public ObjectId securityRelease_id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            public DateTime ReleaseDate { get; set; }
            public string SupportLink { get; set; }
            public ClassificationType Classification { get; set; }
            public SeverityType Severity { get; set; }
            public bool IsSecurityPatch { get; set; }
            public ProductType ProductType { get; set; }
        }
}
