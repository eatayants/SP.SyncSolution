using System;
using System.Collections.Generic;
using System.Dynamic;
using System.ServiceModel;
using System.ServiceModel.Web;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
	[ServiceContract]
    public interface IApiService
    {
        #region metadata

	    [OperationContract]
	    [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
	        UriTemplate = "api/{version}/list/metadata")]
	    Roster.Services.listView<Entity> MetadataList(string version, QueryDisplayParams query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "api/{version}/view/metadata")]
        Roster.Services.listView<Entity> MetadataView(string version, ICollection<abstractSearch> filter, QueryDisplayParams query);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/item/metadata?id={id}")]
        Roster.Services.itemView<Roster.Services.Group> MetadataItem(string version, string id);

        #endregion

        #region content-type

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/list/content-type")]
        Roster.Services.listView<Entity> ContentTypeList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query);

        #endregion

        #region dictionary

	    [OperationContract]
	    [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
	        UriTemplate = "api/{version}/dictionary/sources?type={type}")]
	    ICollection<named> DictionarySources(string version, int type);

	    [OperationContract]
	    [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
	        UriTemplate = "api/{version}/dictionary/fields?sourceId={sourceId}&type={type}")]
	    itemView<Entity> DictionaryFields(string version, string sourceId, int type);

	    [OperationContract]
	    [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
	        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
	        UriTemplate = "api/{version}/list/dictionary")]
        Roster.Services.listView<Entity> DictionaryList(string version, ICollection<abstractSearch> filter,
	        QueryDisplayParams query);

        #endregion

        #region Master Roster

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/list/templates")]
        Roster.Services.listView<Entity> ListTemplates(string version, QueryDisplayParams query);

        #endregion

        #region roster events

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/list/timesheet")]
        Roster.Services.listView<Entity> TimesheetList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/view/roster-event")]
        Roster.Services.listView<Entity> RosterEventView(string version, ICollection<abstractSearch> filter, QueryDisplayParams query);
        
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/list/roster-event")]
        Roster.Services.listView<Entity> RosterEventList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/item/roster-event?itemId={itemId}&viewId={viewId}")]
        Roster.Services.itemView<Entity> GetRosterEventItem(string version, string itemId, string viewId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/item/create/roster-event")]
        itemView<Entity> CreateRosterEventItem(string version, string listId, int eventTypeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/item/save/roster-event")]
        itemView<bool> SaveRosterEventItem(string version, string listId, int eventTypeId, Entity item);

        [OperationContract]
        [WebInvoke(Method = "DELETE", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/item/roster-event?id={id}")]
        itemView<bool> DeleteRosterEventItem(string version, string id);

        #endregion  

        #region roster operation

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/endorse-roster")]
	    itemView<string> EndorseRoster(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/reject-roster")]
	    itemView<string> RejectRoster(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/approve-roster")]
	    itemView<string> ApproveRoster(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/cancel-roster")]
	    itemView<string> CancelRoster(string version, ICollection<abstractSearch> filter);

	    #endregion roster operation

        #region roster execute

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/execute-action")]
        itemView<string> ExecuteAction(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/publish-roster")]
        itemView<bool> PublishRoster(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/submit-timesheet")]
        itemView<string> SubmitTimesheet(string version, ICollection<abstractSearch> filter);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
        RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "api/{version}/submit-timesheets")]
        itemView<string> SubmitTimesheets(string version, ICollection<abstractSearch> filter);

        #endregion
    }
}
