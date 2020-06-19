using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
	public class BaseService
	{
		protected  void HandleException(Exception ex)
		{
		    if (WebOperationContext.Current == null) return;
		    WebOperationContext.Current.OutgoingResponse.StatusDescription = "Internal error. " + ex.Message;
		    WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
		}

		protected void DisableCaching()
		{
			HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
			HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			HttpContext.Current.Response.Cache.SetNoStore();
		}

		protected string GetCurrentUserLogin()
		{
			return ServiceSecurityContext.Current.PrimaryIdentity.IsAuthenticated ? 
				ServiceSecurityContext.Current.PrimaryIdentity.Name : string.Empty;
		}
	}
}
