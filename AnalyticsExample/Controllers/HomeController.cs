using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalyticsExample.Infrastructure.Extensions;
using AnalyticsExample.Models;
using AnalyticsExample.ViewModels.Home;
using AutoMapper;
using Google.GData.Analytics;
using Google.GData.Client;

namespace AnalyticsExample.Controllers
{
	public class HomeController : RavenController
	{
		private const string ApplicationName = "AnalyticsExample";
		public ActionResult Index()
		{
			Settings settings;
			using (RavenSession.Advanced.DocumentStore.DisableAggressiveCaching())
			{
				settings = RavenSession.Load<Settings>(Settings.DefaultId);
			}
			if (settings == null)
				return RedirectToAction("Auth");

			if (string.IsNullOrEmpty(settings.SiteId))
				return RedirectToAction("Config");

			return View();
		}

		public ActionResult Config()
		{
			Settings settings;
			using (RavenSession.Advanced.DocumentStore.DisableAggressiveCaching())
			{
				settings = RavenSession.Load<Settings>(Settings.DefaultId);
			}
			if (settings == null)
				return RedirectToAction("Auth");

			var model = Mapper.Map<Config>(settings);

			return ConfigView(settings, model);
		}

		[HttpPost]
		public ActionResult Config(Config model)
		{
			Settings settings;
			using (RavenSession.Advanced.DocumentStore.DisableAggressiveCaching())
			{
				settings = RavenSession.Load<Settings>(Settings.DefaultId);
			}
			if (settings == null)
				return RedirectToAction("Auth");

			if (!ModelState.IsValid)
				return ConfigView(settings, model);

			Mapper.Map(model, settings);
			RavenSession.Store(settings);

			Response.RemoveOutputCacheItem(Url.Content("~/Widgets/AnalyticsSummary"));

			return RedirectToAction("Index");
		}

		private ActionResult ConfigView(Settings settings, Config model)
		{
			// Grab all of the available sites that the authorized account has access to
			var authFactory = new GAuthSubRequestFactory("analytics", ApplicationName)
								{
									Token = settings.SessionToken
								};
			var analytics = new AnalyticsService(authFactory.ApplicationName) { RequestFactory = authFactory };
			
			foreach (AccountEntry entry in analytics.Query(new AccountQuery()).Entries)
			{
				var account = entry.Properties.First(x => x.Name == "ga:accountName").Value;
				if (!model.Sites.ContainsKey(account))
					model.Sites.Add(account, new Dictionary<string, string>());
				model.Sites[account].Add(entry.ProfileId.Value, entry.Title.Text);
			}

			return View("Config", model);
		}

		public ActionResult Auth()
		{
			const string scope = "https://www.google.com/analytics/feeds/";
			var next = Url.FullPath("~/home/authresponse");
			var url = AuthSubUtil.getRequestUrl(next, scope, false, true);
			return Redirect(url);
		}

		public ActionResult AuthResponse(string token)
		{
			var sessionToken = AuthSubUtil.exchangeForSessionToken(token, null);
			Settings settings;
			using (RavenSession.Advanced.DocumentStore.DisableAggressiveCaching())
			{
				settings = RavenSession.Load<Settings>(Settings.DefaultId);
			}
			if (settings == null)
				settings = new Settings
				{
					Id = Settings.DefaultId
				};

			settings.SessionToken = sessionToken;
			RavenSession.Store(settings);

			return RedirectToAction("Config");
		}
	}
}
