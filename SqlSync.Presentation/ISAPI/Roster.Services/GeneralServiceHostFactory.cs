using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public class GeneralServiceHostFactory : WebServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var webServiceAddress = baseAddresses[0];
            if (HttpContext.Current != null)
            {
                var host = HttpContext.Current.Request.Url.Host;
                var scheme = HttpContext.Current.Request.Url.Scheme;
                var port = HttpContext.Current.Request.Url.Port;
                var hostedAddress = baseAddresses.FirstOrDefault(a => a.Host == host && a.Scheme == scheme);
                if (hostedAddress != null)
                {
                    webServiceAddress = hostedAddress;
                }
                Type[] sslServices = { typeof(DataService) };
                if (sslServices.Any(s => s == serviceType))
                {
                    var builder = new UriBuilder(webServiceAddress) { Scheme = scheme, Port = port };
                    webServiceAddress = builder.Uri;
                }
            }
            return new GeneralServiceHost(serviceType, webServiceAddress);
        }
    }
}
