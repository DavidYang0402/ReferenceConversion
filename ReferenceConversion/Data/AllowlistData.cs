﻿using System;
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
        public List<Project> Projects { get; set; } = new();
    }

    public class Project
    {
        [JsonProperty("projectname")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonProperty("projectguid")]
        public string ProjectGuid { get; set; } = string.Empty;

        [JsonProperty("dllpath")]
        public string DllPath { get; set; } = string.Empty;

        [JsonProperty("Allowlist")]
        public List<Allowlist> Allowlist { get; set; } = new();
    }

    public class Allowlist
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("parent_guid")]
        public string? ParentGuid { get; set; }
    }
}
