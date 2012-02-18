using System;
using System.Text;

namespace AnalyticsExample.Infrastructure.Extensions
{
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Returns the specified datetime in the format of 7pm or 6:30am
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static string ToSmallTime(this DateTime dateTime)
		{
			if (dateTime.Minute == 0)
			{
				return dateTime.ToString("htt").Replace("AM", "am").Replace("PM", "pm");
			}
			return dateTime.ToString("h:mmtt").Replace("AM", "am").Replace("PM", "pm");
		}

		/// <summary>
		/// Returns the specified time in the format of 5 days, 2 hours
		/// </summary>
		/// <param name="ts"></param>
		/// <returns></returns>
		public static string ToSmallTime(this TimeSpan ts)
		{
			return ts.ToSmallTime(false);
		}

		/// <summary>
		/// Returns the specified time in the format of 5 days, 2 hours
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="includeMilliseconds">Include milliseconds with the output</param>
		/// <returns></returns>
		public static string ToSmallTime(this TimeSpan ts, bool includeMilliseconds)
		{
			var text = new StringBuilder();

			if (ts.Days > 0)
			{
				text.Append(ts.Days.ToString("N0"));
				text.Append(" days");
			}
			if (ts.Hours > 0)
			{
				if (text.Length > 0)
				{
					text.Append(", ");
				}
				text.Append(ts.Hours.ToString("N0"));
				text.Append(" hours");
			}
			if (ts.Minutes > 0)
			{
				if (text.Length > 0)
				{
					text.Append(", ");
				}
				text.Append(ts.Minutes.ToString("N0"));
				text.Append(" min");
			}
			if (ts.Seconds > 0)
			{
				if (text.Length > 0)
				{
					text.Append(", ");
				}
				text.Append(ts.Seconds.ToString("N0"));
				text.Append(" sec");
			}
			if (includeMilliseconds && ts.Milliseconds > 0)
			{
				if (text.Length > 0)
				{
					text.Append(", ");
				}
				text.Append(ts.Milliseconds.ToString("N0"));
				text.Append(" ms");
			}
			return text.ToString();
		}
	}
}