using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    #region RosterEvents

    [DataContract]
    public class RosterEventQuery
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public int Page { get; set; }
    }

    [DataContract]
    public class RosterEventResult
    {
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public ICollection<RosterEventResultItem> Items { get; set; }
    }

    [DataContract]
    public class RosterEventResultItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public object id { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string text { get; set; }
    }
    #endregion

    #region List

    [DataContract]
    public class ListQuery
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public Guid ViewId { get; set; }
        [DataMember]
        public Guid FieldId { get; set; }
        [DataMember]
        public string DisplayField { get; set; }
        [DataMember]
        public int Page { get; set; }
    }

    [DataContract]
    public class ListResult
    {
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public ICollection<ListResultItem> Items { get; set; }
    }

    [DataContract]
    public class ListResultItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public object id { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string text { get; set; }
    }
    #endregion

    #region LookupField
    [DataContract]
    public class LookupFieldQuery
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public string ListId { get; set; }
        [DataMember]
        public string FieldId { get; set; }
        [DataMember]
        public int Page { get; set; }
    }

    [DataContract]
    public class LookupFieldResult
    {
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public ICollection<LookupFieldResultItem> Items { get; set; }
    }


    [DataContract]
    public class LookupFieldResultItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public int id { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string name { get; set; }
    }

    #endregion

    #region Lookup

    [DataContract]
    public class LookupQuery
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Fields { get; set; }
        [DataMember]
        public int ListType { get; set; }
        [DataMember]
        public string MetadataId { get; set; }
        [DataMember]
        public string ParentKeyValue { get; set; }
        [DataMember]
        public int Page { get; set; }
    }

    [DataContract]
    public class LookupResult
    {
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public ICollection<LookupResultItem> Items { get; set; }
    }
    
    [DataContract]
    public class LookupResultItemProperty
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string Name { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string Value { get; set; }
    }
    [DataContract]
    public class LookupResultItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public int id { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string name { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string description { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public List<LookupResultItemProperty> property { get; set; }
    }

    #endregion

    #region PopupContent

    [DataContract]
    public class PopupContentQuery
    {
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid ViewId { get; set; }
        [DataMember]
        public Guid ItemId { get; set; }
    }

    [DataContract]
    public class PopupContentResult
    {
        [DataMember]
        public ICollection<PopupContentItem> Items { get; set; }
    }

    [DataContract]
    public class PopupContentItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string Name { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string Text { get; set; }
    }

    #endregion

    #region PublishQuery

    [DataContract]
    public class PublishQuery
    {
        [DataMember]
        public Guid ItemId { get; set; }
        [DataMember]
        public int DaysAhead { get; set; }
    }

    #endregion

    #region ExecuteAction

    [DataContract]
    public class ExecuteActionQuery
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ICollection<ExecuteActionParameter> Parameters { get; set; }
    }

    [DataContract]
    public class ExecuteActionParameter
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class ExecuteActionResult
    {
        [DataMember]
        public string Message { get; set; }
    }

    #endregion

    #region SaveItemData

    [DataContract]
    public class SaveItemDataQuery
    {
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid ItemId { get; set; }
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    #endregion SaveItemData

    #region Roster properties

    public class ItemDataQuery
    {
        [DataMember]
        public Guid ItemId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public string Fields { get; set; }
    }

    [DataContract]
    public class ItemDataResult
    {
        [DataMember]
        public ICollection<ItemDataElem> Props { get; set; }
    }

    [DataContract]
    public class ItemDataElem
    {
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public string FieldValue { get; set; }
        [DataMember]
        public bool FilterLookupId { get; set; }
    }

    #endregion

    #region ListTemplates

    [DataContract]
    public class ListTemplatesQuery
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public int Page { get; set; }
    }

    [DataContract]
    public class ListTemplatesResult
    {
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public ICollection<ListTemplatesResultItem> Items { get; set; }
    }

    [DataContract]
    public class ListTemplatesResultItem
    {
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public object id { get; set; }
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string text { get; set; }
    }
    #endregion

    #region SaveMasterAsTemplate

    [DataContract]
    public class SaveAsTemplateQuery
    {
        [DataMember]
        public int MasterId { get; set; }
    }

    #endregion

    #region SubmitTimesheet

    [DataContract]
    public class SubmitTimesheetQuery
    {
        [DataMember]
        public string StoredProcedureName { get; set; }
        [DataMember]
        public int WorkerId { get; set; }
        [DataMember]
        public string PeriodStart { get; set; }
        [DataMember]
        public string PeriodEnd { get; set; }
        [DataMember]
        public string RosterIDs { get; set; }
    }

    #endregion

    #region GetContentTypeInfo

    [DataContract]
    public class GetContentTypeQuery
    {
        [DataMember]
        public Guid RosterId { get; set; }
    }

    [DataContract]
    public class GetContentTypeResult
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string NewItemUrl { get; set; }
        [DataMember]
        public string EditItemUrl { get; set; }
        [DataMember]
        public string DispItemUrl { get; set; }
        [DataMember]
        public bool IsOnNewAction { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
    }

    #endregion

    #region Reject roster

    public class RejectRosterQuery
    {
        [DataMember]
        public Guid ItemId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public string Reason { get; set; }
    }
    
    #endregion

    #region EnsureUser

    [DataContract]
    public class EnsureUserQuery
    {
        [DataMember]
        public string FieldMetedataId { get; set; }
        [DataMember]
        public string XmlText { get; set; }
    }

    [DataContract]
    public class EnsureUserResult
    {
        [DataMember]
        public int RosterLookupId { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string DisplayText { get; set; }
    }

    #endregion
}
