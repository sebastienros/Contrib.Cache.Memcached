using Orchard.Data.Migration;

namespace Contrib.Cache.Memcached {
    public class Migrations : DataMigrationImpl {
        public int Create() {

            SchemaBuilder.CreateTable("MemcachedSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Servers", c => c.Unlimited())
                );

            return 1;
        }
    }
}