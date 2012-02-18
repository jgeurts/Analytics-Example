using System;

using System.Web.Mvc;

namespace AnalyticsExample.Infrastructure.Extensions
{
	public static class UrlExtensions
	{
		public static string FullPath(this UrlHelper url, string path)
		{
			var requestUrl = url.RequestContext.HttpContext.Request.Url;

			var fullUrl = string.Format("{0}{1}",
												  requestUrl.GetLeftPart(UriPartial.Authority),
												  url.Content(path));

			return fullUrl;
		}
	}
}