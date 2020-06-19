using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    class GeneralErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (fault == null) throw new ArgumentNullException("fault");
            var ex = new GeneralErrorFault { Message = error.Message };
            fault = Message.CreateMessage(MessageVersion.None, "", ex, new DataContractJsonSerializer(ex.GetType()));
            var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);
            if (WebOperationContext.Current == null) return;
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        }
    }

}
