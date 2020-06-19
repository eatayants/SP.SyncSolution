using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Roster.Model.DataContext;
using Roster.Presentation.ColourCoding;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Xml.Linq;
using Roster.BL;
using Roster.BL.Extentions;
using Roster.Common;
using Roster.Presentation.ColourCoding.Recurrence;
using Roster.Presentation.Controls.NAVproxy;

namespace Roster.Presentation.Extensions
{
    public static class BLExtensions
    {
        public static void SyncList(Guid listId)
        {
            var configProvider = new RosterConfigService();
            var mappingSetting = configProvider.GetMappingByList(listId.ToString());
            if (mappingSetting == null) return;
            var dataProvider = new RosterDataService();
            {
                var list = SPContext.Current.SPList(listId);
                var listitems = list.ListItems(mappingSetting.Key, 
                    String.Join("$", mappingSetting.ListMappingFields.Select(item => item.ItemName).ToList()),true);

                dataProvider.SaveRows(mappingSetting, listitems);
            }
        }

        public static void PublishRosterEvent(Guid itemId, int daysAhead)
        {
            var dataService = new RosterDataService();
            var item = dataService.GetRosterEvent(itemId);
            if (item == null)
            {
                throw new Exception(string.Format("Roster with ID '{0}' does not exists!", itemId));
            }
            if (!item.GetIsRecurrence())
            {
                throw new Exception(string.Format("Unable to publish non recurrent Roster Event with ID '{0}'!", itemId));
            }
            var wrListFields = new RosterConfigService().GetList(TableIDs.WORKING_ROSTERS).ListMetadataFields;

            var plannedRosterProps = item.RosterEventDictionary;
            var publishPeriodStart = DateTime.Today;
            var publishPeriodEnd = DateTime.Today.AddDays(daysAhead);
            var expandedEvents = RecurrenceItemExpander.Expand(new List<RosterEvent> { item },
                            null, publishPeriodStart, publishPeriodEnd,
                            FieldNames.START_DATE, FieldNames.END_DATE,
                            SPContext.Current.Web.RegionalSettings.TimeZone);
            var skipProps = new[] 
            { 
                FieldNames.START_DATE, FieldNames.END_DATE,
                FieldNames.RECURRENCE, FieldNames.ROSTER_EVENT_ID, 
                FieldNames.ID, FieldNames.PARENT_ROSTER_ID 
            };
            var expandedRosterEvents = expandedEvents as IList<ExpandedRosterEvent> ?? expandedEvents.ToList();
            publishPeriodStart = expandedRosterEvents.Min(expandedEvent => expandedEvent.StartDate);
            publishPeriodEnd = expandedRosterEvents.Max(expandedEvent => expandedEvent.StartDate);

            #region Get Working Rosters from period to avoid duplicates

            // get required fields
            var startDateFld = wrListFields.FirstOrDefault(itm => itm.InternalName == FieldNames.START_DATE);
            var endDateFld = wrListFields.FirstOrDefault(itm => itm.InternalName == FieldNames.END_DATE);
            var parentIdFld = wrListFields.FirstOrDefault(itm => itm.InternalName == FieldNames.PARENT_ROSTER_ID);
            var rEventIdFld = wrListFields.FirstOrDefault(itm => itm.InternalName == FieldNames.ROSTER_EVENT_ID);

            var qp = new QueryParams { SkipRows = 0, TakeRows = 500 };
            qp.SelectCriteria.Add(startDateFld);
            qp.SelectCriteria.Add(rEventIdFld);
            qp.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                (parentIdFld, CompareType.Equal, ConcateOperator.And, itemId, null));
            qp.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                (startDateFld, CompareType.LessOrEqual, ConcateOperator.And, publishPeriodEnd, null));
            qp.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                (endDateFld, CompareType.MoreOrEqual, ConcateOperator.And, publishPeriodStart, null));

            var existingWorkRosters = dataService.ListRosterEventProperties(TableIDs.WORKING_ROSTERS, qp);
            var existingKeys = existingWorkRosters.Select(wrProps => wrProps.FirstOrDefault(ff =>
                ff.Key == FieldNames.START_DATE)).Select(stKey => (DateTime)stKey.Value).ToList();

            #endregion
          
            foreach (var expEvent in expandedRosterEvents)
            {
                var startDate = expEvent.StartDate;
                var endDate = expEvent.EndDate;
                if (existingKeys.Contains(startDate)) { continue; } // this WorkingRoster instance already published
                var newRoEv = dataService.CreateRosterEvent(TableIDs.WORKING_ROSTERS, (int)RosterEventType.WorkingRosterEvent);
                newRoEv.RosterEventDictionary[FieldNames.START_DATE] = startDate;
                newRoEv.RosterEventDictionary[FieldNames.END_DATE] = endDate;
                newRoEv.RosterEventDictionary[FieldNames.END_DATE] = endDate;
                newRoEv.RosterEventDictionary[FieldNames.RECURRENCE] = null;
                newRoEv.RosterEventDictionary[FieldNames.PARENT_ROSTER_ID] = itemId;
                foreach (var propName in plannedRosterProps.Keys.Where(propName => !skipProps.Contains(propName)))
                {
                    newRoEv.RosterEventDictionary[propName] = plannedRosterProps[propName];
                }
                dataService.SaveRosterEvent(newRoEv, TableIDs.WORKING_ROSTERS);
            }
        }

        public static Tuple<int,ExpandoObject> ToSyncObject(this SPListItem listItem, ListMapping mapping)
        {
            var result = (IDictionary<string,object>)new ExpandoObject();
            mapping.ListMappingFields.ToList().ForEach(item =>
            {
                var spField = item.ItemName.IsGuid() ? listItem.FieldById(item.ItemName.ToGuid()) : listItem.FieldByName(item.ItemName);
                object spItemValue;
                try {
                    spItemValue = (spField != null) ? listItem.GetObjectValue(spField) : string.Empty;
                } catch {
                    spItemValue = string.Empty;
                }
                result.Add(item.ItemName, spItemValue);
            });
            return new Tuple<int, ExpandoObject>(listItem.ID, (ExpandoObject)result);
        }

        public static List<Tuple<int, ExpandoObject>> SourceContent(string source, string key, string fields, int type, List<Tuple<string, string>> whereCriteria = null)
        {
            List<Tuple<int, ExpandoObject>> itemCollection = null;
            switch (type)
            {
                case (int)LookupSourceType.SpList:
                    {
                        itemCollection = SPContext.Current.SPList(source.ToGuid()).ListItemsByCriteria(key, fields, false, whereCriteria);
                        break;
                    }
                case (int)LookupSourceType.Table:
                case (int)LookupSourceType.Query:
                    {
                        itemCollection = new RosterDataService().TableContent(source, key, fields, whereCriteria);
                        break;
                    }
            }
            return itemCollection;
        }

        public static SPFieldType FieldType(this ListMetadataField metaField)
        {
            var fldType = SPFieldType.Invalid;
            try
            {
                var document = XDocument.Parse(metaField.ControlProperties);
                if (document.Root == null) return fldType;
                var fieldTypeAttr = document.Root.Attribute("Type");
                fldType = EnumHelper.GetEnumByDescription<SPFieldType>(fieldTypeAttr.Value);
                return fldType;
            }
            catch
            {
                return fldType;
            }
        }

        public static List<string> FieldValues(this ListMetadataField metaField)
        {
            var fieldValues = new List<string>();
            try
            {
                var document = XDocument.Parse(metaField.ControlProperties);
                if (document.Root == null) return fieldValues;
                var fieldValuesAttr = document.Root.Element("CHOICES");
                if (fieldValuesAttr != null)
                {
                    fieldValues.AddRange(fieldValuesAttr.Elements("CHOICE").Select(x => x.Value).ToArray());
                }
                return fieldValues;
            }
            catch
            {
                return fieldValues;
            }
        }

        public static DbField GetDbField(this ListMetadataField metaField)
        {
            DbField dbField = null;

            XDocument xDoc = XDocument.Parse(metaField.ControlProperties);
            XElement root = xDoc.Root;

            XAttribute fieldTypeAttr = root.Attribute("Type");
            if (fieldTypeAttr != null)
            {
                SPFieldType pFldType = EnumHelper.GetEnumByDescription<SPFieldType>(fieldTypeAttr.Value);
                switch (pFldType)
                {
                    case SPFieldType.Guid:
                        dbField = new DbFieldGuid(metaField.Id);
                        break;
                    case SPFieldType.Text:
                        DbFieldText txtFld = new DbFieldText(metaField.Id);

                        int mLen = 0;
                        XAttribute maxLengthAttr = root.Attribute("MaxLength");
                        if (maxLengthAttr != null && Int32.TryParse(maxLengthAttr.Value, out mLen))
                            txtFld.MaxLength = mLen;

                        dbField = txtFld;
                        break;
                    case SPFieldType.Note:
                        DbFieldNote noteFld = new DbFieldNote(metaField.Id);

                        int nol = 0;
                        XAttribute nolAttr = root.Attribute("NumLines");
                        if (nolAttr != null && Int32.TryParse(nolAttr.Value, out nol))
                            noteFld.NumberOfLines = nol;

                        dbField = noteFld;
                        break;
                    case SPFieldType.Boolean:
                        dbField = new DbFieldBoolean(metaField.Id);
                        break;
                    case SPFieldType.DateTime:
                        DbFieldDateTime dtFld = new DbFieldDateTime(metaField.Id);

                        XAttribute formatAttr = root.Attribute("Format");
                        if (formatAttr != null && !string.IsNullOrEmpty(formatAttr.Value))
                            dtFld.Format = formatAttr.Value; // "DateOnly" or "DateTime"

                        dbField = dtFld;
                        break;
                    case SPFieldType.Number:
                        DbFieldNumber numFld = new DbFieldNumber(metaField.Id);

                        int dcp = 0;
                        XAttribute decAttr = root.Attribute("Decimals");
                        if (decAttr != null && Int32.TryParse(decAttr.Value, out dcp))
                            numFld.DecimalPlaces = dcp; // -1 = 'Automatic'

                        double minV = 0;
                        XAttribute minAttr = root.Attribute("Min");
                        if (minAttr != null && Double.TryParse(minAttr.Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out minV))
                            numFld.MinValue = minV;

                        double maxV = 0;
                        XAttribute maxAttr = root.Attribute("Max");
                        if (maxAttr != null && Double.TryParse(maxAttr.Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out maxV))
                            numFld.MaxValue = maxV;

                        dbField = numFld;
                        break;
                    case SPFieldType.Recurrence:
                        dbField = new DbFieldRecurrence(metaField.Id);
                        break;
                    case SPFieldType.Choice:
                        DbFieldChoice chFld = new DbFieldChoice(metaField.Id);

                        XAttribute chFormatAttr = root.Attribute("Format");
                        if (chFormatAttr != null && !string.IsNullOrEmpty(chFormatAttr.Value))
                            chFld.ControlType = chFormatAttr.Value; // "Dropdown" or "RadioButtons" or "Checkboxes"

                        XElement choicesEl = root.Element("CHOICES");
                        if (choicesEl != null)
                            chFld.Choices = choicesEl.Elements("CHOICE").Select(x => x.Value).ToArray();

                        dbField = chFld;
                        break;
                    case SPFieldType.Lookup:
                        var lookFld = new DbFieldLookup(metaField.Id);

                        var listSourceAttr = root.Attribute("Source");
						if (listSourceAttr != null && !string.IsNullOrEmpty(listSourceAttr.Value))
							lookFld.ListSource = listSourceAttr.Value.ToInt();

                        var listAttr = root.Attribute("List");
                        if (listAttr != null && !string.IsNullOrEmpty(listAttr.Value))
                            lookFld.ListId = listAttr.Value;

                        var listKeyAttr = root.Attribute("LookupKey");
                        if (listKeyAttr != null && !string.IsNullOrEmpty(listKeyAttr.Value))
                            lookFld.LookupKey = listKeyAttr.Value;

                        var lfAttr = root.Attribute("LookupField");
                        if (lfAttr != null && !string.IsNullOrEmpty(lfAttr.Value))
                            lookFld.LookupField = lfAttr.Value;

                        var dependentParentAttr = root.Attribute("DependentParent");
                        if (dependentParentAttr != null && !string.IsNullOrEmpty(dependentParentAttr.Value))
                            lookFld.DependentParent = dependentParentAttr.Value;

                        var dependentParentFieldAttr = root.Attribute("DependentParentField");
                        if (dependentParentFieldAttr != null && !string.IsNullOrEmpty(dependentParentFieldAttr.Value))
                            lookFld.DependentParentField = dependentParentFieldAttr.Value;

                            var filterByFieldAttr = root.Attribute("FilterByField");
                        if (filterByFieldAttr != null && !string.IsNullOrEmpty(filterByFieldAttr.Value))
                            lookFld.FilterByField = filterByFieldAttr.Value;

                        dbField = lookFld;
                        break;
                    case SPFieldType.User:
                        var userFld = new DbFieldUser(metaField.Id);

                        var listSourceAttr2 = root.Attribute("Source");
                        if (listSourceAttr2 != null && !string.IsNullOrEmpty(listSourceAttr2.Value))
                            userFld.ListSource = listSourceAttr2.Value.ToInt();

                        var listAttr2 = root.Attribute("List");
                        if (listAttr2 != null && !string.IsNullOrEmpty(listAttr2.Value))
                            userFld.ListId = listAttr2.Value;

                        var listKeyAttr2 = root.Attribute("LookupKey");
                        if (listKeyAttr2 != null && !string.IsNullOrEmpty(listKeyAttr2.Value))
                            userFld.LookupKey = listKeyAttr2.Value;

                        var lfAttr2 = root.Attribute("LookupField");
                        if (lfAttr2 != null && !string.IsNullOrEmpty(lfAttr2.Value))
                            userFld.LookupField = lfAttr2.Value;

                        var asAttr2 = root.Attribute("AllowSelection");
                        if (asAttr2 != null && !string.IsNullOrEmpty(asAttr2.Value))
                            userFld.AllowSelection = asAttr2.Value;

                        var cfAttr2 = root.Attribute("ChooseFrom");
                        if (cfAttr2 != null && !string.IsNullOrEmpty(cfAttr2.Value))
                            userFld.ChooseFrom = cfAttr2.Value;

                        var cfgAttr2 = root.Attribute("ChooseFromGroup");
                        if (cfgAttr2 != null && !string.IsNullOrEmpty(cfgAttr2.Value))
                            userFld.ChooseFromGroup = cfgAttr2.Value;

                        dbField = userFld;
                        break;
                    default:
                        break;
                }

                if (dbField != null)
                {
                    // common attributes
                    dbField.ReadOnly = metaField.ReadOnly;
                    dbField.DisplayName = metaField.FieldName;
                    dbField.InternalName = metaField.InternalName;
                    dbField.DefaultValue = metaField.DefaultValue;
                    dbField.Required = metaField.Required;
                    dbField.Hidden = metaField.Hidden;
                }
            }

            return dbField;
        }

        public static void SetControlProperties(this ListMetadataField metaField, DbField dbField)
        {
            XDocument xDoc = new XDocument(new XElement("Field",
                                                new XAttribute("Type", dbField.TypeAsString)));
            XElement root = xDoc.Root;

            switch (dbField.Type)
            {
                case SPFieldType.Text:
                    root.Add(new XAttribute("MaxLength", (dbField as DbFieldText).MaxLength));
                    break;
                case SPFieldType.Note:
                    root.Add(new XAttribute("NumLines", (dbField as DbFieldNote).NumberOfLines));
                    break;
                case SPFieldType.Boolean:
                    break;
                case SPFieldType.DateTime:
                    root.Add(new XAttribute("Format", (dbField as DbFieldDateTime).Format));
                    break;
                case SPFieldType.Number:
                    DbFieldNumber numFld = dbField as DbFieldNumber;
                    root.Add(new XAttribute("Decimals", numFld.DecimalPlaces));
                    root.Add(new XAttribute("Min", numFld.MinValue));
                    root.Add(new XAttribute("Max", numFld.MaxValue));
                    break;
                case SPFieldType.Recurrence:
                    dbField = new DbFieldRecurrence(metaField.Id);
                    break;
                case SPFieldType.Choice:
                    DbFieldChoice chFld = dbField as DbFieldChoice;
                    root.Add(new XAttribute("Format", chFld.ControlType)); // "Dropdown" or "RadioButtons" or "Checkboxes"
                    root.Add(new XElement("CHOICES", chFld.Choices.Select(x => new XElement("CHOICE", x))));
                    break;
                case SPFieldType.Lookup:
                    DbFieldLookup loFld = dbField as DbFieldLookup;
                    root.Add(new XAttribute("Source", loFld.ListSource));
                    root.Add(new XAttribute("List", loFld.ListId));
                    root.Add(new XAttribute("LookupKey", loFld.LookupKey));
                    root.Add(new XAttribute("LookupField", loFld.LookupField));
                    if (!String.IsNullOrWhiteSpace(loFld.DependentParent)) {
                        root.Add(new XAttribute("DependentParent", loFld.DependentParent));
                        root.Add(new XAttribute("DependentParentField", loFld.DependentParentField));
                        root.Add(new XAttribute("FilterByField", loFld.FilterByField));
                    }
                    break;
                case SPFieldType.User:
                    DbFieldUser usFld = dbField as DbFieldUser;
                    root.Add(new XAttribute("Source", usFld.ListSource));
                    root.Add(new XAttribute("List", usFld.ListId));
                    root.Add(new XAttribute("LookupKey", usFld.LookupKey));
                    root.Add(new XAttribute("LookupField", usFld.LookupField));
                    root.Add(new XAttribute("AllowSelection", usFld.AllowSelection));
                    root.Add(new XAttribute("ChooseFrom", usFld.ChooseFrom));
                    root.Add(new XAttribute("ChooseFromGroup", usFld.ChooseFromGroup));
                    break;
                default:
                    break;
            }

            metaField.ControlProperties = xDoc.ToString(SaveOptions.DisableFormatting);
        }

        public static bool IsCalendarView(this ViewMetadata view)
        {
            return view.DisplayType == Microsoft.SharePoint.SPViewCollection.SPViewType.Calendar.ToString();
        }

        public static StaticColourSettings GetDerializedStaticColourSettings(this ViewMetadata view)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();

            StaticColourSettings staticSettings = new StaticColourSettings();
            staticSettings.Conditions = jsSer.Deserialize<List<StaticCondition>>(view.StaticColourCodingSettings);

            return staticSettings;
        }

        public static DynamicColourSettings GetDerializedDynamicColourSettings(this ViewMetadata view)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            DynamicColourSettings dynSettings = jsSer.Deserialize<DynamicColourSettings>(view.DynamicColourCodingSettings);

            return dynSettings;
        }

        public static string GetEventTitleFieldName(this ViewMetadata view, string calendarScope)
        {
            if (string.IsNullOrEmpty(view.ViewData))
                throw new ArgumentException("Cannot get Calendar view Title field - ViewData is empty");

            string tKey = string.Empty;
            if (calendarScope.ToLower() == "week")
                tKey = "CalendarWeekTitle";
            else if (calendarScope.ToLower() == "month")
                tKey = "CalendarMonthTitle";
            else
                tKey = "CalendarDayTitle";

            var xDoc = XDocument.Parse(view.ViewData);
            var fRefEl = xDoc.Root.Descendants("FieldRef").FirstOrDefault(el => el.Attribute("Type").Value == tKey);
            if (fRefEl == null)
                throw new ArgumentException("Cannot get Calendar view Title field");

            string fldName = fRefEl.Attribute("Name").Value;
            var eventTitleField = view.ListMetadata.ListMetadataFields.FirstOrDefault(f => f.InternalName == fldName);
            if (eventTitleField == null)
                throw new Exception("Field '" + fldName + "' is used in ViewData but no longer exists. Please update View!");

            if (eventTitleField.FieldType() == SPFieldType.Lookup) {
                var eventTitleDbFieldLookup = eventTitleField.GetDbField() as DbFieldLookup;
                return string.Format("{0}_{1}", eventTitleDbFieldLookup.ListId, eventTitleDbFieldLookup.LookupField.Split('$')[0]);
            } else if (eventTitleField.FieldType() == SPFieldType.User) {
                var eventTitleDbFieldUser = eventTitleField.GetDbField() as DbFieldUser;
                return string.Format("{0}_{1}", eventTitleDbFieldUser.ListId, eventTitleDbFieldUser.LookupField.Split('$')[0]);
            } else {
                return fldName;
            }
        }

        public static string GetEventLocationFieldName(this ViewMetadata view, string calendarScope)
        {
            if (string.IsNullOrEmpty(view.ViewData))
                throw new ArgumentException("Cannot get Calendar view Location field - ViewData is empty");

            string tKey = string.Empty;
            if (calendarScope.ToLower() == "day")
                tKey = "CalendarDayLocation";
            else
                tKey = "CalendarWeekLocation"; // common for week and month modes

            var xDoc = XDocument.Parse(view.ViewData);
            var fRefEl = xDoc.Root.Descendants("FieldRef").FirstOrDefault(el => el.Attribute("Type").Value == tKey);
            if (fRefEl == null)
                throw new ArgumentException("Cannot get Calendar view Location field");

            string fldName = fRefEl.Attribute("Name").Value;
            var eventLocationField = view.ListMetadata.ListMetadataFields.FirstOrDefault(f => f.InternalName == fldName);
            if (eventLocationField == null)
                throw new Exception("Field '" + fldName + "' is used in ViewData but no longer exists. Please update View!");

            if (eventLocationField.FieldType() == SPFieldType.Lookup) {
                var eventLocationDbFieldLookup = eventLocationField.GetDbField() as DbFieldLookup;
                return string.Format("{0}_{1}", eventLocationDbFieldLookup.ListId, eventLocationDbFieldLookup.LookupField.Split('$')[0]);
            } else if (eventLocationField.FieldType() == SPFieldType.User) {
                var eventLocationDbFieldUser = eventLocationField.GetDbField() as DbFieldUser;
                return string.Format("{0}_{1}", eventLocationDbFieldUser.ListId, eventLocationDbFieldUser.LookupField.Split('$')[0]);
            } else {
                return fldName;
            }
        }

        public static List<SPCalendarItem> GetInstanceEventsOfRecurrenceEvent(this SPCalendarItem calendarItem, string recurrenceData, Pair calendarPeriod, DateOptions dateOptions)
        {
            List<SPCalendarItem> instanceEvents = new List<SPCalendarItem>();

            if (string.IsNullOrEmpty(recurrenceData)) {
                instanceEvents.Add(calendarItem);
                return instanceEvents;
            }

            // init Event period inside Calendar view scope
            DateTime fromDate = (DateTime)calendarPeriod.First > calendarItem.StartDate ? ((DateTime)calendarPeriod.First).Date : calendarItem.StartDate.Date; // get MAX date
            DateTime toDate = (DateTime)calendarPeriod.Second < calendarItem.EndDate ? ((DateTime)calendarPeriod.Second).Date : calendarItem.EndDate.Date; // get MIN date

            if (fromDate > toDate)
                return instanceEvents;

            XDocument recDoc = XDocument.Parse(recurrenceData);
            XElement repeatEl = recDoc.Descendants("repeat").FirstOrDefault();

            IEnumerable<DateTime> instanceDates = null;
            if (repeatEl.Element("daily") != null)
            {
                XAttribute attrDailyFrequency = repeatEl.Element("daily").Attribute("dayFrequency");
                if (attrDailyFrequency != null)
                {
                    // Daily-Frequency
                    // update from date
                    int _dayFrequency = Int32.Parse(attrDailyFrequency.Value);
                    int daysBetweenEventStartAndCalendarPeriod = fromDate.Subtract(calendarItem.StartDate.Date).Days;
                    if (daysBetweenEventStartAndCalendarPeriod > 0)
                        fromDate = calendarItem.StartDate.Date.AddDays((daysBetweenEventStartAndCalendarPeriod / _dayFrequency) * _dayFrequency); // move fromDate to the beginning of Calendar interval
                    // get dates
                    instanceDates = Enumerable.Range(0, 1 + toDate.Subtract(fromDate).Days).Select(c => fromDate.AddDays(c * _dayFrequency)).Where(x => (x >= fromDate && x <= toDate));
                }
                else
                {
                    // Daily-weekly
                    instanceDates = Enumerable.Range(0, 1 + toDate.Subtract(fromDate).Days).Select(c => fromDate.AddDays(c)).Where(x => (dateOptions.IsWorkDay((int)x.DayOfWeek) && x >= fromDate && x <= toDate));
                }
            }
            else if (repeatEl.Element("weekly") != null)
            {
                XElement weeklyEl = repeatEl.Element("weekly");
                int weekFreq = Convert.ToInt32(weeklyEl.Attribute("weekFrequency").Value);
                var days = weeklyEl.Attributes().Where(x => x.Name.ToString().Length == 2).Select(x => Utils.GetDayOfWeekByDayShortName(x.Name.ToString()));
                // get real date of first event
                DateTime stRealDt = calendarItem.StartDate.Date;
                while (!days.Any(x => x == stRealDt.DayOfWeek)) {
                    stRealDt = stRealDt.AddDays(1);
                }
                int weeksBetweenEventStartAndCalendarPeriod = fromDate.Subtract(stRealDt).Days / 7;
                if (weeksBetweenEventStartAndCalendarPeriod > 0)
                    fromDate = stRealDt.AddDays((weeksBetweenEventStartAndCalendarPeriod / weekFreq) * weekFreq * 7); // move fromDate to the beginning of Calendar interval
                //instanceDates = Enumerable.Range(0, 1 + (toDate.Subtract(fromDate).Days / 7)).Select(c => fromDate.AddDays(c)).Where(x => (dateOptions.IsWorkDay((int)x.DayOfWeek) && x >= fromDate && x <= toDate));
            }
            else if (repeatEl.Element("monthlyByDay") != null)
            {

            }
            else if (repeatEl.Element("monthly") != null)
            {
                XElement monthlyByDayEl = repeatEl.Element("monthly");
                int monthFreq = Convert.ToInt32(monthlyByDayEl.Attribute("monthFrequency").Value);
                int day = Convert.ToInt32(monthlyByDayEl.Attribute("day").Value);
                DateTime nextMonth = calendarItem.StartDate.Date.AddMonths(1);
                fromDate = calendarItem.StartDate.Day <= day ? new DateTime(calendarItem.StartDate.Year, calendarItem.StartDate.Month, day) : new DateTime(nextMonth.Year, nextMonth.Month, day);
                if (toDate >= fromDate)
                    instanceDates = Enumerable.Range(0, 1 + toDate.Subtract(fromDate).Days / 28).Select(c => fromDate.AddMonths(c * monthFreq)).Where(x => (x >= fromDate && x <= toDate));
            }
            else if (repeatEl.Element("yearly") != null)
            {
                XElement yearlyEl = repeatEl.Element("yearly");
                int month = Convert.ToInt32(yearlyEl.Attribute("month").Value);
                int day = Convert.ToInt32(yearlyEl.Attribute("day").Value);
                DateTime yDt = new DateTime(month == 1 ? toDate.Year : fromDate.Year, month, day);
                if (fromDate <= yDt && yDt <= toDate)
                    instanceDates = new[] { yDt };
            }
            else if (repeatEl.Element("yearlyByDay") != null)
            {
                XElement yearlyDbEl = repeatEl.Element("yearlyByDay");
                int month = Convert.ToInt32(yearlyDbEl.Attribute("month").Value);
                string weekdayOfMonth = yearlyDbEl.Attribute("weekdayOfMonth").Value;
                string dayShortName = yearlyDbEl.Attributes().Where(x => x.Name.ToString().Length == 2).Select(x => x.Name.ToString()).FirstOrDefault();
                DateTime yDt = new DateTime(month == 1 ? toDate.Year : fromDate.Year, month, 1).GetDayOfMonth(dayShortName, weekdayOfMonth);
                if (fromDate <= yDt && yDt <= toDate)
                    instanceDates = new[] { yDt };
            }

            // COPY pattern to instances
            if (instanceDates != null)
            {
                int startHour = calendarItem.StartDate.Hour;
                int startMinute = calendarItem.StartDate.Minute;
                int endHour = calendarItem.EndDate.Hour;
                int endMinute = calendarItem.EndDate.Minute;
                instanceEvents.AddRange(instanceDates.Select(x => new SPCalendarItem() {
                    CalendarType = calendarItem.CalendarType,
                    DisplayFormUrl = calendarItem.DisplayFormUrl,
                    ItemID = calendarItem.ItemID,
                    StartDate = new DateTime(x.Year, x.Month, x.Day, startHour, startMinute, 0),
                    EndDate = new DateTime(x.Year, x.Month, x.Day, endHour, endMinute, 0),
                    hasEndDate = true,
                    Title = calendarItem.Title,
                    Location = calendarItem.Location,
                    Description = calendarItem.Description,
                    IsAllDayEvent = calendarItem.IsAllDayEvent,
                    IsRecurrence = true
                }));
            }

            return instanceEvents;
        }

        public static DateTime GetDayOfMonth(this DateTime instance, string dayShortName, string weekdayOfMonth)
        {
            // Change to first day of the month
            DateTime dayOfMonth = instance.AddDays(1 - instance.Day);

            #region Init params from strings

            DayOfWeek dayOfWeek = Utils.GetDayOfWeekByDayShortName(dayShortName);

            int occurance = 0;
            switch (weekdayOfMonth)
            {
                case "first":
                    occurance = 1;
                    break;
                case "second":
                    occurance = 2;
                    break;
                case "third":
                    occurance = 3;
                    break;
                case "fourth":
                    occurance = 4;
                    break;
                case "last":
                    occurance = 5; // last index can be four or five
                    break;
                default:
                    break;
            }

            #endregion

            // Find first dayOfWeek of this month;
            while (dayOfMonth.DayOfWeek != dayOfWeek)
                dayOfMonth = dayOfMonth.AddDays(1);

            // add 7 days per occurance
            dayOfMonth = dayOfMonth.AddDays(7 * (occurance - 1));

            // make sure this occurance is within the original month
            // in case of weekdayOfMonth = 'last' and occurence=5 is incorrect
            if (dayOfMonth.Month != instance.Month)
                dayOfMonth = dayOfMonth.AddDays(-7);

            return dayOfMonth;
        }

        public static TimeZoneInfo GetZoneInfoById(this SPTimeZoneCollection instance, int id)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                SPTimeZone zone = instance[i];
                if (zone.ID == id) {
                    //return TimeZoneInfo.FindSystemTimeZoneById(zone.KeyName);
                }
            }
            throw new ArgumentOutOfRangeException("id");
        }

        public static DateTime GetStartDate(this RosterEvent rEvent)
        {
            string fldKey = FieldNames.START_DATE;
            var props = (IDictionary<string, object>)rEvent.RosterEventProperties;
            if (props.ContainsKey(fldKey) && props[fldKey] != null)
                return Convert.ToDateTime(props[fldKey]);

            throw new ArgumentException(string.Format("There is no '{0}' field!", fldKey));
        }
        public static DateTime GetEndDate(this RosterEvent rEvent)
        {
            string fldKey = FieldNames.END_DATE;
            var props = (IDictionary<string, object>)rEvent.RosterEventProperties;
            if (props.ContainsKey(fldKey) && props[fldKey] != null)
                return Convert.ToDateTime(props[fldKey]);

            throw new ArgumentException(string.Format("There is no '{0}' field!", fldKey));
        }
        public static string GetRecurrence(this RosterEvent rEvent)
        {
            string fldKey = FieldNames.RECURRENCE;
            var props = (IDictionary<string, object>)rEvent.RosterEventProperties;
            if (props.ContainsKey(fldKey) && props[fldKey] != null)
                return Convert.ToString(props[fldKey]);
            else
                return string.Empty;
        }
        public static bool GetIsRecurrence(this RosterEvent rEvent)
        {
            return !string.IsNullOrEmpty(rEvent.GetRecurrence());
        }
        public static bool GetIsAllDayEvent(this RosterEvent rEvent)
        {
            string fldKey = FieldNames.ALL_DAY_EVENT;
            var props = (IDictionary<string, object>)rEvent.RosterEventProperties;
            if (props.ContainsKey(fldKey) && props[fldKey] != null)
                return props[fldKey] != DBNull.Value ? Convert.ToBoolean(props[fldKey]) : false;

            //throw new ArgumentException(string.Format("There is no '{0}' field!", fldKey));
            return false;
        }
        public static int GetShPEventType(this RosterEvent rEvent)
        {
            return 1;
        }

        public static string GetButtonAsXml(this ListMetadataAction action)
        {
            XDocument btnDoc = new XDocument(
                new XElement("Button",
                    new XAttribute("Id", action.Id.ToString("N")),
                    new XAttribute("Sequence", action.Sequence.ToString()),
                    new XAttribute("Image32by32", action.ImageUrl),
                    new XAttribute("Description", action.Description ?? string.Empty),
                    new XAttribute("Command", action.GetCommandName()),
                    new XAttribute("LabelText", action.LabelText),
                    new XAttribute("TemplateAlias", "ONERW")));

            return btnDoc.ToString(SaveOptions.DisableFormatting);
        }
        public static string GetCommandName(this ListMetadataAction action)
        {
            return string.Format("{0}.{1}", action.Id.ToString("N"), "Command");
        }

        public static bool IsVisibleForUser(this ListMetadataAction customAction, SPUser user)
        {
            try
            {
                if (string.IsNullOrEmpty(customAction.AccessGroup.Trim()))
                    return true;

                return SPContext.Current.Web.CurrentUser.ID == SPContext.Current.Site.SystemAccount.ID ||
                        customAction.AccessGroup.Split(';').Select(g => g.ToUpper()).Intersect(user.Groups.Cast<SPGroup>().Select(gr => gr.Name.ToUpper())).Any();
            }
            catch
            {
                return false;
            }
        }

        public static List<int> GroupIDs(this SPUser user)
        {
            return user.Groups.Cast<SPGroup>().Select(gr => gr.ID).ToList();
        }

        public static void FillFromRoster(this CreateTimeSheetLines line, RosterEvent roster, IEnumerable<DbField> timesheetFields, List<Roster.Presentation.Layouts.MapElem> mapping)
        {
            var rosterProps = roster.RosterEventDictionary;

            Func<Layouts.MapElem, IEnumerable<DbField>, string> getLookupValueFunc = delegate(Layouts.MapElem map, IEnumerable<DbField> fields) {
                string _val = string.Empty;
                if (map != null) {
                    string[] fldNameParts = map.FieldName.Split('$');
                    var lookupFld = fields.FirstOrDefault(fld => fld.InternalName == map.FieldName || fld.Id == fldNameParts[0].ToGuid()) as DbFieldLookup;
                    _val = (fldNameParts.Length == 2) ?
                        rosterProps[string.Format("{0}_{1}", lookupFld.ListId, fldNameParts[1])].ToSafeString() :
                        rosterProps[map.FieldName].ToSafeString();
                }
                return _val;
            };

            // set Description
            var descrMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Description));
            if (descrMap != null) {
                line.Description = rosterProps[descrMap.FieldName].ToSafeString();
            }

            // set Type
            var typeMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Type));
            if (typeMap != null) {
                string rosterType = rosterProps[typeMap.FieldName].ToSafeString().XmlToList().FirstOrDefault();
                line.Type = EnumHelper.GetEnumByDescription<Roster.Presentation.Controls.NAVproxy.Type>(
                        string.IsNullOrEmpty(rosterType) ? "_blank_" : rosterType
                    );
                line.TypeSpecified = (line.Type != Roster.Presentation.Controls.NAVproxy.Type._blank_);
            }

            // set Resource_No
            var resNoMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Resource_No));
            line.Resource_No = getLookupValueFunc(resNoMap, timesheetFields);

            // set Job_No
            var jobNoMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Job_No));
            line.Job_No = getLookupValueFunc(jobNoMap, timesheetFields);

            // set Job_Task_No
            var jobTaskNoMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Job_Task_No));
            line.Job_Task_No = getLookupValueFunc(jobTaskNoMap, timesheetFields);

            // set Cause_of_Absence_Code
            var coaCoMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Cause_of_Absence_Code));
            line.Cause_of_Absence_Code = getLookupValueFunc(coaCoMap, timesheetFields);

            // set Work_Type_Code
            var workTypeMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Work_Type_Code));
            line.Work_Type_Code = getLookupValueFunc(workTypeMap, timesheetFields);

            // set Chargeable
            var chargMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Chargeable));
            if (chargMap != null) {
                line.Chargeable = rosterProps[chargMap.FieldName].ToBoolean();
                line.ChargeableSpecified = true;
            }

            // set Non_Roster_Day
            var nonRosMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Non_Roster_Day));
            if (nonRosMap != null) {
                line.Non_Roster_Day = rosterProps[nonRosMap.FieldName].ToBoolean();
                line.Non_Roster_DaySpecified = true;
            }

            // set Start_Time
            var startMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Start_Time));
            if (startMap != null) {
                line.Start_Time = (DateTime)rosterProps[startMap.FieldName];
                line.Start_TimeSpecified = true;
            }

            // set End_Time
            var endMap = mapping.FirstOrDefault(m => m.WebParam == EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.End_Time));
            if (endMap != null) {
                line.End_Time = (DateTime)rosterProps[endMap.FieldName];
                line.End_TimeSpecified = true;
            }

            // QTY1-7
            DateTime rosterStartDate = (DateTime)rosterProps[FieldNames.START_DATE];
            DateTime rosterEndDate = (DateTime)rosterProps[FieldNames.END_DATE];
            double qty = Math.Round((rosterEndDate - rosterStartDate).TotalHours, 2, MidpointRounding.AwayFromZero);
            int qtyIndex = (rosterStartDate - line.Time_Sheet_Starting_Date).Days + 1;
            switch (qtyIndex)
            {
                case 1:
                    line.Qty1 = (decimal)qty; line.Qty1Specified = true; break;
                case 2:
                    line.Qty2 = (decimal)qty; line.Qty2Specified = true; break;
                case 3:
                    line.Qty3 = (decimal)qty; line.Qty3Specified = true; break;
                case 4:
                    line.Qty4 = (decimal)qty; line.Qty4Specified = true; break;
                case 5:
                    line.Qty5 = (decimal)qty; line.Qty5Specified = true; break;
                case 6:
                    line.Qty6 = (decimal)qty; line.Qty6Specified = true; break;
                case 7:
                    line.Qty7 = (decimal)qty; line.Qty7Specified = true; break;
                default:
                    break;
            }

            // -------------------------
            // You need to set Specified = true before your initial call to Create.
            // The same goes for all value types – if you don’t set Specified to true, then the value doesn’t get transferred to NAV Web Services,
            // this is a behavior of the proxy classes generated by Visual Studio.
            // When you get the record back from NAV – all value type fields will have their corresponding Specified field set to true and thus you can modify and send it back.
            // -------------------------
        }
    }
}
