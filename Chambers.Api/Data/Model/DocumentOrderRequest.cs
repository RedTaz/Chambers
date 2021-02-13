using System;
using System.Collections.Generic;
using System.Text;

namespace Chambers.Api.Data.Model
{
    public class DocumentOrderRequest
    {
        public string DocumentId { get; set; }

        public int Order { get; set; }
    }
}
