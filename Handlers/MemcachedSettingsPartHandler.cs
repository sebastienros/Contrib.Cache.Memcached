using Contrib.Cache.Memcached.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Cache.Memcached.Handlers {
    public class MemcachedSettingsPartHandler : ContentHandler {

        public MemcachedSettingsPartHandler(IRepository<MemcachedSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<MemcachedSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}