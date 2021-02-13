using System;
using System.Collections.Generic;
using System.Text;

namespace Chambers.Api.Data.Model
{
    public class DocumentOrderRequestItem
    {
        public Guid DocumentId { get; set; }

        public int Order { get; set; }
    }
}
