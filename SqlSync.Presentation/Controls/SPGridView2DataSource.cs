using Microsoft.SharePoint;
using Roster.BL;
using Roster.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Roster.Presentation.Extensions;
using Roster.BL.Extentions;
using Roster.Common;
using Roster.Presentation.Controls.Fields;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.ColourCoding.Recurrence;
using Microsoft.SharePoint.Utilities;
using System.Text.RegularExpressions;

namespace Roster.Presentation.Controls
{
    public class SPGridView2DataSource
    {
        #region Private variables

        private ViewMetadata m_view;
        private QueryParams m_GridFilterExpression = null;
        private Guid dynColoring_OriginalRosterId;
        private bool displayPrepopulatedRosters = false;

        #endregion

        #region Constructors

        public SPGridView2DataSource(ViewMetadata view, QueryParams filterExpression)
        {
            this.m_view = view;

            if (filterExpression != null)
                this.m_GridFilterExpression = filterExpression;
        }

        public SPGridView2DataSource(ViewMetadata view, QueryParams filterExpression, Guid origRosterId, bool displayPrepopRosters) : this(view, filterExpression)
        {
            // !!! only for Calendar View !!!

            // ONLY for Dynamic colour-coding
            this.dynColoring_OriginalRosterId = origRosterId;

            this.displayPrepopulatedRosters = displayPrepopRosters;
        }

        #endregion

        #region Get data Methods

        public DataTable SelectData(string SortExpression, int iBeginRowIndex, int iMaximumRows)
        {
            #region Init Query

            var query = new QueryParams {SkipRows = iBeginRowIndex, TakeRows = iMaximumRows};
            // add order
            if (!string.IsNullOrEmpty(SortExpression))
            {
                var order = SortDirection.Ascending;
                var values = SortExpression.Split(' ').ToList();
                if (values.Count() > 1) {
                    order = (values[1].ToUpper() == "DESC") ? SortDirection.Descending : SortDirection.Ascending;
                }
                var fieldId = values[0].ToGuid();
                var sortPart = string.Empty;
                if (fieldId == Guid.Empty)
                {
                    var ids = values[0].Split('$').ToList();
                    if (ids.Count()==2) {
                        fieldId = ids[0].ToGuid();
                        sortPart = ids[1].ToSafeString();
                    }
                }
                var sortField = m_view.ListMetadata.ListMetadataFields.FirstOrDefault(item => item.Id == fieldId);
                if (sortField == null) {
                    sortField = m_view.ListMetadata.ListMetadataFields.FirstOrDefault(item => item.InternalName == values[0]);
                }
                if (sortField != null) {
                    query.OrderCriteria.Add(new Tuple<ListMetadataField, SortDirection,string>(sortField, order, sortPart));
                }
            }

            // add filter
            if (this.m_GridFilterExpression != null && this.m_GridFilterExpression.WhereCriteria.Any()) {
                query.WhereCriteria.AddRange(this.m_GridFilterExpression.WhereCriteria);
            }

            #endregion

            // small case for STATUSES list
            if (this.m_view.ListMetadataId == Roster.Common.TableIDs.STATUS_HISTORY)
                return GetStatusesAsDataTable(this.m_view, new RosterDataService().ViewEventStatuses(this.m_view.Id, query));
            else
                return GetRostersAsDataTable(this.m_view, new RosterDataService().ViewRosterEvents(this.m_view.Id, query));
        }

        public SPCalendarItemCollection SelectDataForCalendar(short calendarType, Tuple<DateTime, DateTime> calendarPeriod, string calendarScope, out object rostersForColoring)
        {
            var query = new QueryParams();
            var listFields = this.m_view.ListMetadata.ListMetadataFields;
            var startDateFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.START_DATE);
            var endDateFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.END_DATE);

            // set filter according period displayed by Calendar
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(startDateFld, CompareType.LessOrEqual, ConcateOperator.And, calendarPeriod.Item2,null));
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(endDateFld,   CompareType.MoreOrEqual, ConcateOperator.And, calendarPeriod.Item1, null));

            // add filter
            if (this.m_GridFilterExpression != null && this.m_GridFilterExpression.WhereCriteria.Any()) {
                query.WhereCriteria.AddRange(this.m_GridFilterExpression.WhereCriteria);
            }

            // extract Roster data
            RosterDataService _dataSrv = new RosterDataService();
            //List<RosterEvent> rosters = (this.m_view.ListMetadataId == Roster.Common.TableIDs.TIMESHEET_ROSTERS && this.displayPrepopulatedRosters) ?
            //    _dataSrv.ViewTimesheetEvents(this.m_view.Id, query, calendarPeriod) : _dataSrv.ViewRosterEvents(this.m_view.Id, query);
            List<RosterEvent> rosters = _dataSrv.ViewRosterEvents(this.m_view.Id, query);

            // expant recurrent events
            var tZone = SPContext.Current.Web.RegionalSettings.TimeZone;
            List<ExpandedRosterEvent> expandedEvents = RecurrenceItemExpander.Expand(rosters, null, calendarPeriod.Item1, calendarPeriod.Item2,
                        FieldNames.START_DATE, FieldNames.END_DATE, tZone).ToList();

            // send Object to output for coloring
            if (!this.m_view.GetDerializedDynamicColourSettings().IsEmpty) {
                if (this.dynColoring_OriginalRosterId != Guid.Empty && !rosters.Any(r => r.Id == this.dynColoring_OriginalRosterId))
                {
                    List<RosterEvent> rosters_dynColor = rosters;
                    RosterEvent origRoster = _dataSrv.GetRosterEvent(this.dynColoring_OriginalRosterId); // !! don't return all properties

                    // get events around Orig.Roster
                    var queryDyn = new QueryParams();
                    queryDyn.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(startDateFld, CompareType.LessOrEqual, ConcateOperator.And, origRoster.GetEndDate(), null));
                    queryDyn.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(endDateFld, CompareType.MoreOrEqual, ConcateOperator.And, origRoster.GetStartDate(),null));
                    rosters_dynColor.AddRange(_dataSrv.ViewRosterEvents(this.m_view.Id, queryDyn));

                    rosters_dynColor = rosters_dynColor.GroupBy(r => r.Id).Select(g => g.First()).ToList(); // remove duplicates
                    rostersForColoring = RecurrenceItemExpander.Expand(rosters_dynColor, null, calendarPeriod.Item1, calendarPeriod.Item2,
                        FieldNames.START_DATE, FieldNames.END_DATE, tZone); // for dynamic coloring

                    // add original Roster (only for Dynamic colour-coding)
                    expandedEvents.Add(new ExpandedRosterEvent(rosters_dynColor.First(r => r.Id == this.dynColoring_OriginalRosterId), true, tZone));
                }
                else
                {
                    rostersForColoring = expandedEvents; // for dynamic coloring
                }
            } else {
                rostersForColoring = rosters;        // for static coloring
            }

            // get default DispForm url and default EventTypeId for rosters
            string defaultDispFormUrl = string.Format("{0}/{1}&ListId={2}", SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/'),
                this.m_view.ListMetadata.DispItemUrl.TrimStart('/'), SPEncode.UrlEncode(this.m_view.ListMetadataId.ToString("B").ToUpper()));
            var matchEventType = Regex.Match(defaultDispFormUrl, @"EventType=(?<eType>\d+)");
            int defaultEventType = matchEventType.Success ? matchEventType.Groups["eType"].Value.ToInt() : -1;
            string eventTitleFieldName = this.m_view.GetEventTitleFieldName(calendarScope);
            string eventLocationFieldName = this.m_view.GetEventLocationFieldName(calendarScope);
            
            // init some variables for Working Rosters which are displayed on TIMESHEET view
            string workingRoster_DispFormUrl = string.Empty;
            string workingRosterEventTitleFieldName = string.Empty;
            string workingRosterEventLocationFieldName = string.Empty;
            if (this.m_view.ListMetadataId == Roster.Common.TableIDs.TIMESHEET_ROSTERS)
            {
                var workingRosterList = new RosterConfigService().GetList(Roster.Common.TableIDs.WORKING_ROSTERS);
                workingRoster_DispFormUrl = string.Format("{0}/{1}&ListId={2}", SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/'),
                    workingRosterList.DispItemUrl.TrimStart('/'), SPEncode.UrlEncode(workingRosterList.Id.ToString("B").ToUpper()));

                var workingRosterView = workingRosterList.ViewMetadatas.FirstOrDefault(x => x.Name == ViewNames.WORKING_ROSTERS_FOR_TIMESHEETS);
                workingRosterEventTitleFieldName = (workingRosterView != null) ? workingRosterView.GetEventTitleFieldName(calendarScope) : FieldNames.START_DATE;
                workingRosterEventLocationFieldName = (workingRosterView != null) ? workingRosterView.GetEventLocationFieldName(calendarScope) : FieldNames.END_DATE;
            }

            // init Calendar Items
            var items = new SPCalendarItemCollection();
            items.AddRange(expandedEvents.Select(x => new SPCalendarItem()
            {
                DisplayFormUrl = (defaultEventType == x.EventTypeId) ? defaultDispFormUrl : workingRoster_DispFormUrl,
                CalendarType = calendarType,
                ItemID = x.InstanceID,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                hasEndDate = true,
                Title = ((IDictionary<string, object>)x.OriginalItem.RosterEventProperties)[(defaultEventType == x.EventTypeId) ? eventTitleFieldName : workingRosterEventTitleFieldName].ToString(),
                Location = ((IDictionary<string, object>)x.OriginalItem.RosterEventProperties)[(defaultEventType == x.EventTypeId) ? eventLocationFieldName : workingRosterEventLocationFieldName].ToString(),
                IsAllDayEvent = x.OriginalItem.GetIsAllDayEvent(),
                IsRecurrence = x.OriginalItem.GetIsRecurrence()
            }).ToList());

            return items;
        }

        public int SelectRowsCount()
        {
            // Init Query
            var query = new QueryParams();
            if (this.m_GridFilterExpression != null && this.m_GridFilterExpression.WhereCriteria.Any()) {
                query.WhereCriteria.AddRange(this.m_GridFilterExpression.WhereCriteria);
            }

            if (this.m_view.ListMetadataId == Roster.Common.TableIDs.STATUS_HISTORY)
                return new RosterDataService().CountEventStatuses(this.m_view.Id, query);
            else
                return new RosterDataService().CountRosterEvents(this.m_view.Id, query);
        }

        public DataTable SelectEmptyData(string SortExpression, int iBeginRowIndex, int iMaximumRows)
        {
            var viewFields = this.m_view.ViewMetadataFields.OrderBy(x => x.Position).Select(x => x.ListMetadataField.GetDbField());
            return GetDataTableStructure(this.m_view, viewFields);
        }
        public int SelectEmptyDataCount()
        {
            return 0;
        }

        #endregion

        #region Static methods

        public static DataTable GetRostersAsDataTable(ViewMetadata view, List<RosterEvent> rosters)
        {
            var viewFields = view.ViewMetadataFields.OrderBy(x => x.Position).Select(x => x.ListMetadataField.GetDbField());
            DataTable tbl = GetDataTableStructure(view, viewFields);
            #region Init Rows
            foreach (var roster in rosters)
            {
                DataRow row = tbl.NewRow();
                row["ID"] = roster.Id;
                var props = (IDictionary<string, object>)roster.RosterEventProperties;
                foreach (var fld in viewFields)
                {
                    try
                    {
                        row[fld.InternalName] = (fld.Type == SPFieldType.Boolean) ?
                            ((bool)fld.GetFieldValue(props[fld.InternalName]) ? "Yes" : "No") : fld.GetFieldValue(props[fld.InternalName]);

                        if (fld.Type == SPFieldType.Lookup) {
                            // add lookup label
                            var fldLookup = (DbFieldLookup)fld;
                            fldLookup.LookupField.Split('$').ToList().ForEach(itemField => {
                                var internalName = string.Format("{0}_{1}", fldLookup.ListId, itemField);
                                row[internalName] = fld.GetFieldValue(props[internalName]);
                            });
                        } else if (fld.Type == SPFieldType.User) {
                            // add user-lookup label
                            var fldUser = (DbFieldUser)fld;
                            fldUser.LookupField.Split('$').ToList().ForEach(itemField => {
                                var internalName = string.Format("{0}_{1}", fldUser.ListId, itemField);
                                row[internalName] = fld.GetFieldValue(props[internalName]);
                            });
                        }
                    } catch (Exception ex) {
                        string aa = ex.Message;
                    }
                }
                tbl.Rows.Add(row);
            }
            #endregion

            return tbl;
        }
        public static DataTable GetRostersAsDataTable(ViewMetadata view, IEnumerable<ExpandedRosterEvent> expandedRosters)
        {
            var viewFields = view.ViewMetadataFields.OrderBy(x => x.Position).Select(x => x.ListMetadataField.GetDbField());
            DataTable tbl = GetDataTableStructure(view, viewFields);

            #region Init Rows

            foreach (var roster in expandedRosters)
            {
                DataRow row = tbl.NewRow();
                row["ID"] = roster.InstanceID;
                row[FieldNames.START_DATE] = roster.StartDate;
                row[FieldNames.END_DATE] = roster.EndDate;

                var props = (IDictionary<string, object>)roster.RosterEventProperties;
                foreach (var fld in viewFields)
                {
                    if (fld.InternalName == FieldNames.START_DATE || fld.InternalName == FieldNames.END_DATE)
                        continue;

                    row[fld.InternalName] = (fld.Type == SPFieldType.Boolean) ?
                        ((bool)fld.GetFieldValue(props[fld.InternalName]) ? "Yes" : "No") :
                        fld.GetFieldValue(props[fld.InternalName]);

                    if (fld.Type == SPFieldType.Lookup) {
                        // add lookup labels
                        var fldLookup = (DbFieldLookup)fld;
                        fldLookup.LookupField.Split('$').ToList().ForEach(itemField => {
                            var lookupValFldName = string.Format("{0}_{1}", fldLookup.ListId, itemField);
                            row[lookupValFldName] = fld.GetFieldValue(props[lookupValFldName]);
                        });
                    } else if (fld.Type == SPFieldType.User) {
                        // add user-lookup labels
                        var fldUser = (DbFieldUser)fld;
                        fldUser.LookupField.Split('$').ToList().ForEach(itemField => {
                            var lookupValFldName = string.Format("{0}_{1}", fldUser.ListId, itemField);
                            row[lookupValFldName] = fld.GetFieldValue(props[lookupValFldName]);
                        });
                    }
                }

                tbl.Rows.Add(row);
            }

            #endregion

            return tbl;
        }
        private static DataTable GetDataTableStructure(ViewMetadata view, IEnumerable<DbField> viewFields)
        {
            DataTable tbl = new DataTable();

            #region Init Columns

            // ID column
            DataColumn column_ID = new DataColumn("ID");
            column_ID.Caption = "ID";
            column_ID.AllowDBNull = false;
            column_ID.DataType = typeof(String);
            tbl.Columns.Add(column_ID);

            foreach (var fld in viewFields)
            {
                DataColumn column = new DataColumn(fld.InternalName);
                column.Caption = fld.DisplayName;
                column.AllowDBNull = true;

                switch (fld.Type)
                {
                    case SPFieldType.DateTime:
                        column.DataType = typeof(DateTime);
                        break;
                    //case SPFieldType.Boolean:
                    //    column.DataType = typeof(Boolean);
                    //    break;
                    case SPFieldType.Number:
                        column.DataType = typeof(Double);
                        break;
                    case SPFieldType.Lookup:
                        column.DataType = typeof(String);
                        // add extra column with label
                        var fldLookup = (DbFieldLookup)fld;
                        fldLookup.LookupField.Split('$').ToList().ForEach(itemField => {
                            var internalName = string.Format("{0}_{1}", fldLookup.ListId, itemField);
                            var column_LookupLabel = new DataColumn(internalName);
                            column_LookupLabel.Caption = fld.DisplayName + itemField + "_Label";
                            column_LookupLabel.AllowDBNull = true;
                            column_LookupLabel.DataType = typeof(String);
                            tbl.Columns.Add(column_LookupLabel);
                        });
                        break;
                    case SPFieldType.User:
                        column.DataType = typeof(String);
                        // add extra column with label
                        var fldUser = (DbFieldUser)fld;
                        fldUser.LookupField.Split('$').ToList().ForEach(itemField => {
                            var internalName = string.Format("{0}_{1}", fldUser.ListId, itemField);
                            var column_UserLabel = new DataColumn(internalName);
                            column_UserLabel.Caption = fld.DisplayName + itemField + "_Label";
                            column_UserLabel.AllowDBNull = true;
                            column_UserLabel.DataType = typeof(String);
                            tbl.Columns.Add(column_UserLabel);
                        });
                        break;
                    default:
                        column.DataType = typeof(String); // default column type
                        break;
                }
                tbl.Columns.Add(column);
            }

            #endregion

            return tbl;
        }
        public static DataTable GetStatusesAsDataTable(ViewMetadata view, List<vwEventStatu> statusItems)
        {
            var viewFields = view.ViewMetadataFields.OrderBy(x => x.Position).Select(x => x.ListMetadataField.GetDbField());
            DataTable tbl = GetDataTableStructure(view, viewFields);

            #region Init Rows

            foreach (var statusItm in statusItems)
            {
                DataRow row = tbl.NewRow();
                row["ID"] = statusItm.Id;
                var props = statusItm.EventStatusDictionary;
                foreach (var fld in viewFields)
                {
                    try
                    {
                        row[fld.InternalName] = (fld.Type == SPFieldType.Boolean) ?
                            ((bool)fld.GetFieldValue(props[fld.InternalName]) ? "Yes" : "No") : fld.GetFieldValue(props[fld.InternalName]);

                        if (fld.Type == SPFieldType.Lookup) {
                            // add lookup label
                            var fldLookup = (DbFieldLookup)fld;
                            fldLookup.LookupField.Split('$').ToList().ForEach(itemField => {
                                var internalName = string.Format("{0}_{1}", fldLookup.ListId, itemField);
                                row[internalName] = fld.GetFieldValue(props[internalName]);
                            });
                        } else if (fld.Type == SPFieldType.User)
                        {
                            // add user-lookup label
                            var fldUser = (DbFieldUser)fld;
                            fldUser.LookupField.Split('$').ToList().ForEach(itemField => {
                                var internalName = string.Format("{0}_{1}", fldUser.ListId, itemField);
                                row[internalName] = fld.GetFieldValue(props[internalName]);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        string aa = ex.Message;
                    }
                }
                tbl.Rows.Add(row);
            }

            #endregion

            return tbl;
        }

        #endregion
    }
}
