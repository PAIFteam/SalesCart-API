using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SalesCart.Core.Domain.Entities.Base
{
    public class OutPutBase
    {
        
        public bool Result { get; set; }
        public string Message { get; set; }

        [JsonIgnore]
        public Exception? Exception { get; set; }


    }
}
