using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Contrib.Cache.Memcached.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Contrib.Cache.Memcached.Services {
    public class SettingsBanner: INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly WorkContext _workContext;

        public SettingsBanner(IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor) {
            _orchardServices = orchardServices;
            _workContext = workContextAccessor.GetContext();
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            if (String.IsNullOrWhiteSpace(_orchardServices.WorkContext.CurrentSite.As<MemcachedSettingsPart>().Servers)) {
                var urlHelper = new UrlHelper(_workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Index", "Admin", new {Area = "Contrib.Cache.Memcached"});
                yield return new NotifyEntry { Message = T("The Memcached feature needs the <a href=\"{0}\">severs settings</a> to be set.", url), Type = NotifyType.Warning };
            }
        }
    }
}
