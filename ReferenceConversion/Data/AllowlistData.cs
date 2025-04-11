using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReferenceConversion.Data
{
    public class AllowlistData
    {
        [JsonProperty("projects")]
        public List<Project> Projects { get; set; }
    }

    public class Project
    {
        [JsonProperty("projectname")]
        public string ProjectName { get; set; }

        [JsonProperty("projectguid")]
        public string ProjectGuid { get; set; }

        [JsonProperty("Allowlist")]
        public List<Allowlist> Allowlist { get; set; }
    }

    public class Allowlist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("parent_guid")]
        public string? ParentGuid { get; set; }
    }
}
