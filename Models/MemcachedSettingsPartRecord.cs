using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Contrib.Cache.Memcached.Models {
    public class MemcachedSettingsPartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string Servers { get; set; }
    }
}