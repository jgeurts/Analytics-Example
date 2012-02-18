using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnalyticsExample.ViewModels.Home
{
	public class Config
	{
		[Display(Name = "Site")]
		[Required(ErrorMessage="Select a site to work with")]
		public string SiteId { get; set; }

		public IDictionary<string, IDictionary<string, string>> Sites { get; set; }

		public Config()
		{
			Sites = new Dictionary<string, IDictionary<string, string>>();
		}
	}
}