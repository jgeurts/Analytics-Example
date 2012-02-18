using System.Web.Mvc;
using System.Xml.Linq;
using Raven.Client;

namespace AnalyticsExample.Controllers
{
	public abstract class RavenController : Controller
	{
		public static IDocumentStore DocumentStore { get; set; }

		public IDocumentSession RavenSession { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			RavenSession = (IDocumentSession)HttpContext.Items["CurrentRequestRavenSession"];
		}
	}
}