using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AnalyticsExample.Controllers;
using AnalyticsExample.Models;
using AutoMapper;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Client.MvcIntegration;

namespace AnalyticsExample
{
	public class MvcApplication : HttpApplication
	{
		public MvcApplication()
		{
			BeginRequest += (sender, args) =>
			                	{
			                		HttpContext.Current.Items["CurrentRequestRavenSession"] = RavenController.DocumentStore.OpenSession();
			                	};
			EndRequest += (sender, args) =>
			              	{
								using (var session = (IDocumentSession)HttpContext.Current.Items["CurrentRequestRavenSession"])
								{
									if (session == null)
										return;

									if (Server.GetLastError() != null)
										return;

									session.SaveChanges();
								}
			              	};
		}
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);

			ConfigureAutomapper();

			InitializeDocumentStore();
			RavenController.DocumentStore = DocumentStore;
		}

		private static void ConfigureAutomapper()
		{
			Mapper.CreateMap<Settings, ViewModels.Home.Config>();
			Mapper.CreateMap<ViewModels.Home.Config, Settings>();
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		public static IDocumentStore DocumentStore { get; private set; }
		private static void InitializeDocumentStore()
		{
			if (DocumentStore != null) return; // prevent misuse

			DocumentStore = new EmbeddableDocumentStore
			                	{
			                		ConnectionStringName = "RavenDB"
			                	}.Initialize();

			TryCreatingIndexesOrRedirectToErrorPage();

			RavenProfiler.InitializeFor(DocumentStore,
			                            //Fields to filter out of the output
			                            "GoogleAnalyticsKey");
		}

		private static void TryCreatingIndexesOrRedirectToErrorPage()
		{
			try
			{
				IndexCreation.CreateIndexes(typeof (HomeController).Assembly, DocumentStore);
			}
			catch (WebException e)
			{
				var socketException = e.InnerException as SocketException;
				if(socketException == null)
					throw;

				switch (socketException.SocketErrorCode)
				{
					case SocketError.AddressNotAvailable:
					case SocketError.NetworkDown:
					case SocketError.NetworkUnreachable:
					case SocketError.ConnectionAborted:
					case SocketError.ConnectionReset:
					case SocketError.TimedOut:
					case SocketError.ConnectionRefused:
					case SocketError.HostDown:
					case SocketError.HostUnreachable:
					case SocketError.HostNotFound:
						HttpContext.Current.Response.Redirect("~/RavenNotReachable.htm");
						break;
					default:
						throw;
				}
			}
		}	}
}