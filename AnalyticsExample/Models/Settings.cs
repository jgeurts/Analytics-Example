namespace AnalyticsExample.Models
{
	public class Settings
	{
		public static string DefaultId
		{
			get
			{
				return "Site/Settings";
			}
		}
		public string Id { get; set; }
		public string SiteId { get; set; }
		public string SessionToken { get; set; }
	}
}