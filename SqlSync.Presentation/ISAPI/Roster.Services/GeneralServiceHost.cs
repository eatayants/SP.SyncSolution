using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client.Services;
using Microsoft.SharePoint.Client.Utilities;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public class GeneralServiceHost : MultipleBaseAddressWebServiceHost
    {
        public GeneralServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
            {
            }

        protected override void OnOpening()
        {
            base.OnOpening();

            foreach (ServiceEndpoint endpoint in base.Description.Endpoints)
            {
                if (((endpoint.Binding != null) &&
                     (endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>() != null)) &&
                    (endpoint.Behaviors.Find<WebScriptEnablingBehavior>() == null))
                {
                    while (endpoint.Behaviors.Count > 0)
                    {
                        endpoint.Behaviors.RemoveAt(0);
                    }
                    endpoint.Behaviors.Add(new WebScriptEnablingBehavior());
                }

            }
            var debug = this.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debug == null)
            {
                this.Description.Behaviors.Add(new ServiceDebugBehavior {IncludeExceptionDetailInFaults = true});
            }
            else
            { 
                if (!debug.IncludeExceptionDetailInFaults)
                {
                    debug.IncludeExceptionDetailInFaults = true;
                }
            }

            var metadata = this.Description.Behaviors.Find<ServiceMetadataBehavior>(); 
            if (metadata == null)
            {
                this.Description.Behaviors.Add(
                    new ServiceMetadataBehavior() {HttpGetEnabled = true, HttpsGetEnabled = true});
            }
            else
            {    
                if (!metadata.HttpGetEnabled)
                {
                    metadata.HttpGetEnabled = true;
                }
                if (!metadata.HttpsGetEnabled)
                {
                    metadata.HttpsGetEnabled = true;
                }
            }
        }
    }

}
