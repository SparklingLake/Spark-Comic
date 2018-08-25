using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark_Comic.Models
{
    [Serializable]
    class PermissionsToken
    {
        public String id { get; set; }
        public String name { get; set; }
        public String token { get; set; }
        public String path { get; set; }
    }
}
