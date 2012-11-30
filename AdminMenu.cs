using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Contrib.Cache.Memcached {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Cache"), "10.0", subMenu => subMenu.Action("Index", "Admin", new { area = "Contrib.Cache" }).Permission(StandardPermissions.SiteOwner)
                        .Add(T("Memcached"), "11.0", item => item.Action("Index", "Admin", new { area = "Contrib.Cache.Memcached" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                    ));
        }
    }
}
