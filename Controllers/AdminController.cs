using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Contrib.Cache.Memcached.Models;
using Contrib.Cache.Memcached.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;

namespace Contrib.Cache.Memcached.Controllers {
    [ValidateInput(false)]
    public  class AdminController : Controller, IUpdateModel {
        private readonly IRepository<MemcachedSettingsPartRecord> _repository;
        private readonly ISignals _signals;

        public AdminController(
            IOrchardServices services, 
            IRepository<MemcachedSettingsPartRecord> repository,
            ISignals signals) {
            _repository = repository;
            _signals = signals;
            Services = services;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var part = Services.WorkContext.CurrentSite.As<MemcachedSettingsPart>();

            var viewModel = new MemcachedSettingsViewModel {
                Servers = part.Servers
            };

            return View(viewModel);
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new MemcachedSettingsViewModel();
            var part = Services.WorkContext.CurrentSite.As<MemcachedSettingsPart>();

            bool valid = false;
            if (TryUpdateModel(viewModel)) {
                if (!String.IsNullOrEmpty(viewModel.Servers)) {
                    using (var urlReader = new StringReader(viewModel.Servers)) {
                        string relativeUrl;
                        // ignore empty lines and comments (#)
                        while (null != (relativeUrl = urlReader.ReadLine())) {
                            if(String.IsNullOrWhiteSpace(relativeUrl) || relativeUrl.Trim().StartsWith("#")) {
                                continue;
                            }

                            valid = true;
                        }
                    }
                }

                // invalidate the cache
                _signals.Trigger(MemcachedSettingsPart.CacheKey);

                part.Servers = viewModel.Servers;
            }

            if (!valid) {
                AddModelError("Servers", T("At least one server must be provided."));
                Services.TransactionManager.Cancel();
            }

            return Index();
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}