using System;
using System.Collections.Generic;
using System.Dynamic;
using System.ServiceModel;
using System.ServiceModel.Web;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
	[ServiceContract]
	public interface IDataService
	{
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        RosterEventResult RosterEvents(RosterEventQuery query);

	    [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.ListResult List(Roster.Services.ListQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.LookupResult Lookup(Roster.Services.LookupQuery query);

	    [OperationContract]
	    [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	    RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.LookupFieldResult LookupField(Roster.Services.LookupFieldQuery query);

	    [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
	    Roster.Services.PopupContentResult PopupContent(Roster.Services.PopupContentQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void PublishPlannedRoster(Roster.Services.PublishQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SaveRosterItemData(Roster.Services.SaveItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.ItemDataResult RosterItemData(Roster.Services.ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.ExecuteActionResult ExecuteAction(Roster.Services.ExecuteActionQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SaveRosterAsTemplate(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void RemoveRoster(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Roster.Services.ListTemplatesResult ListTemplates(ListTemplatesQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SaveMasterAsTemplate(SaveAsTemplateQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult SubmitTimesheet(SubmitTimesheetQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        GetContentTypeResult GetContentTypeByRosterId(GetContentTypeQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult EndorseRoster(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult RejectRoster(RejectRosterQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult ApproveRoster(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult CancelRoster(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ExecuteActionResult SubmitSingleTimesheet(ItemDataQuery query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        EnsureUserResult EnsureUser(EnsureUserQuery query);
	}
}
