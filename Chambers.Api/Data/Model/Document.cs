using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chambers.Api.Data.Model
{
    public class Document
    {
        [JsonProperty("id")]
        public Guid Id { get; internal set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public int Order { get; set; }

        public byte[] Content { get; set; }
    }
}
