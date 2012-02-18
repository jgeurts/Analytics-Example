using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using AnalyticsExample.Models;
using AnalyticsExample.ViewModels.Widgets;
using Google.GData.Analytics;
using Google.GData.Client;

namespace AnalyticsExample.Controllers
{
	public class WidgetsController : RavenController
	{
		private const string ApplicationName = "AnalyticsExample";

		public ActionResult Index()
		{			
			return Redirect("~/");
		}

		[OutputCache(Duration = 3600, Location = OutputCacheLocation.Server, VaryByParam = "none")]
		public ActionResult AnalyticsSummary()
		{
			var to = DateTime.Today.AddDays(-1);
			var from = to.AddDays(-30);

			var model = new AnalyticsSummary
			            	{
			            		Visits = new List<int>(),
								PageViews = new Dictionary<string, int>(),
								PageTitles = new Dictionary<string, string>(),
								TopReferrers = new Dictionary<string, int>(),
								TopSearches = new Dictionary<string, int>()
			            	};

			var settings = RavenSession.Load<Settings>(Settings.DefaultId);

			var authFactory = new GAuthSubRequestFactory("analytics", ApplicationName)
								{
									Token = settings.SessionToken
								};


			var analytics = new AnalyticsService(authFactory.ApplicationName) { RequestFactory = authFactory };

			// Get from All Visits
			var visits = new DataQuery(settings.SiteId, from, to)
							{
								Metrics = "ga:visits",
								Dimensions = "ga:date",
								Sort = "ga:date"
							};
			foreach (DataEntry entry in analytics.Query(visits).Entries)
			{
				var value = entry.Metrics.First().IntegerValue;

				model.Visits.Add(value);
			}

			// Get Site Usage
			var siteUsage = new DataQuery(settings.SiteId, from, to)
							{
								Metrics = "ga:visits,ga:pageviews,ga:percentNewVisits,ga:avgTimeOnSite,ga:entranceBounceRate,ga:exitRate,ga:pageviewsPerVisit,ga:avgPageLoadTime"
							};
			var siteUsageResult = (DataEntry)analytics.Query(siteUsage).Entries.FirstOrDefault();
			if (siteUsageResult != null)
			{
				foreach (var metric in siteUsageResult.Metrics)
				{
					switch (metric.Name)
					{
						case "ga:visits":
							model.TotalVisits = metric.IntegerValue;
							break;
						case "ga:pageviews":
							model.TotalPageViews = metric.IntegerValue;
							break;
						case "ga:percentNewVisits":
							model.PercentNewVisits = metric.FloatValue;
							break;
						case "ga:avgTimeOnSite":
							model.AverageTimeOnSite = TimeSpan.FromSeconds(metric.FloatValue);
							break;
						case "ga:entranceBounceRate":
							model.EntranceBounceRate = metric.FloatValue;
							break;
						case "ga:exitRate":
							model.PercentExitRate = metric.FloatValue;
							break;
						case "ga:pageviewsPerVisit":
							model.PageviewsPerVisit = metric.FloatValue;
							break;
						case "ga:avgPageLoadTime":
							model.AveragePageLoadTime = TimeSpan.FromSeconds(metric.FloatValue);
							break;
					}
				}
			}

			// Get Top Pages
			var topPages = new DataQuery(settings.SiteId, from, to)
							{
								Metrics = "ga:pageviews",
								Dimensions = "ga:pagePath,ga:pageTitle",
								Sort = "-ga:pageviews",
								NumberToRetrieve = 20
							};
			foreach (DataEntry entry in analytics.Query(topPages).Entries)
			{
				var value = entry.Metrics.First().IntegerValue;
				var url = entry.Dimensions.Single(x => x.Name == "ga:pagePath").Value.ToLowerInvariant();
				var title = entry.Dimensions.Single(x => x.Name == "ga:pageTitle").Value;

				if (!model.PageViews.ContainsKey(url))
					model.PageViews.Add(url, 0);
				model.PageViews[url] += value;

				if (!model.PageTitles.ContainsKey(url))
					model.PageTitles.Add(url, title);
			}

			// Get Top Referrers
			var topReferrers = new DataQuery(settings.SiteId, from, to)
							{
								Metrics = "ga:visits",
								Dimensions = "ga:source,ga:medium",
								Sort = "-ga:visits",
								Filters = "ga:medium==referral",
								NumberToRetrieve = 5
							};
			foreach (DataEntry entry in analytics.Query(topReferrers).Entries)
			{
				var visitCount = entry.Metrics.First().IntegerValue;
				var source = entry.Dimensions.Single(x => x.Name == "ga:source").Value.ToLowerInvariant();

				model.TopReferrers.Add(source, visitCount);
			}

			// Get Top Searches
			var topSearches = new DataQuery(settings.SiteId, from, to)
							{
								Metrics = "ga:visits",
								Dimensions = "ga:keyword",
								Sort = "-ga:visits",
								Filters = "ga:keyword!=(not set);ga:keyword!=(not provided)",
								NumberToRetrieve = 5
							};
			foreach (DataEntry entry in analytics.Query(topSearches).Entries)
			{
				var visitCount = entry.Metrics.First().IntegerValue;
				var source = entry.Dimensions.Single(x => x.Name == "ga:keyword").Value.ToLowerInvariant();

				model.TopSearches.Add(source, visitCount);
			}
		
			return View(model);
		}

		public ActionResult Refresh(string url)
		{
			Response.RemoveOutputCacheItem(url);

			return Redirect(url);
		}
	}
}