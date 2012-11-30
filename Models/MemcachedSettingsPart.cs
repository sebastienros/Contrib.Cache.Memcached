using Orchard.ContentManagement;

namespace Contrib.Cache.Memcached.Models {
    public class MemcachedSettingsPart : ContentPart<MemcachedSettingsPartRecord> {
        public const string CacheKey = "MemcachedSettingsPart";

        public string Servers {
            get { return Record.Servers; }
            set { Record.Servers = value; }
        }
    }
}