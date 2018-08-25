using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Spark_Comic.Models
{
    [Serializable]
    class GlobalVar
    {
        public static StorageFolder localFolder;

        public static List<PermissionsToken> permissionsTokens = new List<PermissionsToken>();

        public static List<BookLibrary> bookLibLibraries = new List<BookLibrary>();
        
    }
}
