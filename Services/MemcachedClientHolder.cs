using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Contrib.Cache.Memcached.Models;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Settings;
using CacheItem = Contrib.Cache.Models.CacheItem;

namespace Contrib.Cache.Memcached.Services {
    /// <summary>
    /// Provides a singleton implementation of the memcached client
    /// as it is thread safe
    /// </summary>
    public interface IMemcachedClientHolder : ISingletonDependency {
        MemcachedClient GetClient();

        void Set(string key, CacheItem cacheItem);
        void Remove(string key);
        void RemoveAll();
        CacheItem GetCacheItem(string key);
        IEnumerable<CacheItem> GetCacheItems(int skip, int count);
        int GetCacheItemsCount();
    }

    public class MemcachedClientHolder : IMemcachedClientHolder, IDisposable {
        private readonly Work<ISiteService> _service;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        private MemcachedClient _client;
        private readonly object _synLock = new object();
        private readonly ConcurrentDictionary<string, object> _keys = new ConcurrentDictionary<string, object>();  

        public MemcachedClientHolder(
            Work<ISiteService> service, 
            ICacheManager cacheManager,
            ISignals signals) {
            _service = service;
            _service = service;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public MemcachedClient GetClient() {
            // caches the ignored urls to prevent a query to the settings
            var servers = _cacheManager.Get("MemcachedSettingsPart.Servers",
                context => {
                    context.Monitor(_signals.When(MemcachedSettingsPart.CacheKey));

                    var part = _service.Value.GetSiteSettings().As<MemcachedSettingsPart>();

                    // initializes the client to notify it has to be constructed again
                    lock (_synLock) {
                        if (_client != null) {
                            _client.Dispose();
                        }
                        _client = null;
                    }
                    
                    return part.Servers;
                }
            );

            if (_client == null  && !String.IsNullOrEmpty(servers)) {
                var configuration = new MemcachedClientConfiguration();
                using (var urlReader = new StringReader(servers)) {
                    string server;
                    // ignore empty lines and comments (#)
                    while (null != (server = urlReader.ReadLine())) {
                        if (String.IsNullOrWhiteSpace(server) || server.Trim().StartsWith("#")) {
                            continue;
                        }

                        var values = server.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int port = 11211;

                        if (values.Length == 2) {
                            Int32.TryParse(values[1], out port);
                        }

                        if (values.Length > 0) {
                            configuration.AddServer(values[0], port);
                        }
                    }

                    lock (_synLock) {
                        _client = new MemcachedClient(configuration);
                    }
                }

            }

            return _client;
        }

        public void Set(string key, CacheItem cacheItem) {
            GetClient().Store(StoreMode.Set, key, cacheItem, cacheItem.ValidUntilUtc);
            _keys[key] = new object();
        }

        public void Remove(string key) {
            GetClient().Remove(key);
            object o;
            _keys.TryRemove(key, out o);
        }

        public void RemoveAll() {
            GetClient().FlushAll();
            _keys.Clear();
        }

        public CacheItem GetCacheItem(string key) {
            return GetClient().Get<CacheItem>(key);
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            return GetClient().Get(_keys.Keys.AsEnumerable().Skip(skip).Take(count)).Values.Cast<CacheItem>();
        }

        public int GetCacheItemsCount() {
            return _keys.Keys.Count;
        }

        public void Dispose() {
            if (_client != null) {
                _client.Dispose();
            }
        }
    }
}