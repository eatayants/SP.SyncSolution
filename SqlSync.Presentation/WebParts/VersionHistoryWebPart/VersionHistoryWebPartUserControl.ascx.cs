using System;
using System.Linq;
using System.Web.UI;
using Roster.Common;
using Roster.BL;
using Roster.Presentation.Extensions;
using Roster.Model.DataContext;
using System.Collections.Generic;
using Roster.Presentation.Controls.Fields;
using Microsoft.SharePoint;
using Roster.Presentation.Controls;

namespace Roster.Presentation.WebParts.VersionHistoryWebPart
{
    public partial class VersionHistoryWebPartUserControl : UserControl
    {
        #region Properties

        public Guid? ItemId
        {
            get { return Request.QueryString["RosterId"].ToNullableGuid(); }
        }

        public Guid? ListId
        {
            get { return Request.QueryString["ListId"].ToNullableGuid(); }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (ItemId.HasValue)
                {
                    VersionsRep.DataSource = _getDataSource();
                    VersionsRep.DataBind();
                }
                //
            }
        }

        private List<VersionDetails> _getDataSource()
        {
            var versions = new List<VersionDetails>();
            var dataService = new RosterDataService();
            var rosterEvent = dataService.GetRosterEvent(this.ItemId.Value);
            var listId = ListId.HasValue ? ListId.Value :
                (rosterEvent.EventTypeId == 0) ? TableIDs.PLANNED_ROSTERS : TableIDs.WORKING_ROSTERS;
            var allListFields = new RosterConfigService().GetList(listId).ListMetadataFields.Select(fld => fld.GetDbField());
            // get history items
            var historyInfo = dataService.ListTrackData(this.ItemId.Value);
            string[] excludeFields = new string[] { FieldNames.ID, FieldNames.ROSTER_EVENT_ID,
                FieldNames.CONTENT_TYPE_ID, FieldNames.MODIFIED, FieldNames.MODIFIED_BY};
            for (int k = 0; k < historyInfo.Count; k++)
            {
                var currentVersionTrackData = historyInfo[k].TrackDataDictionary;
                var fldModifiedByText = string.Empty;
                var fldModifiedBy = allListFields.FirstOrDefault(f => f.InternalName.Equals(FieldNames.MODIFIED_BY));
                if (fldModifiedBy != null)
                {
                    if (currentVersionTrackData.ContainsKey(FieldNames.MODIFIED_BY))
                    {
                       fldModifiedByText = fldModifiedBy.GetFieldValueAsText(currentVersionTrackData[FieldNames.MODIFIED_BY]);
                    }
                }
                var vd = new VersionDetails(historyInfo[k], fldModifiedByText);
                if (k == historyInfo.Count - 1)
                {
                    foreach (string fldName in currentVersionTrackData.Keys.Where(ke => !excludeFields.Contains(ke)))
                    {
                        var fld = allListFields.FirstOrDefault(f => f.InternalName.Equals(fldName));
                        if (fld == null)
                        {
                            continue;
                        }
                        vd.Changes.Add(new ChangeInfo
                        {
                            FieldName = fld.DisplayName,
                            FieldValue = fld.GetFieldValueAsText(currentVersionTrackData[fldName])
                        });
                    }
                }
                else
                {
                    var prevVersionTrackData = historyInfo[k + 1].TrackDataDictionary;
                    foreach (string fldName in currentVersionTrackData.Keys.Where(ke => !excludeFields.Contains(ke)))
                    {
                        if (!prevVersionTrackData.ContainsKey(fldName) || (prevVersionTrackData.ContainsKey(fldName) &&
                            prevVersionTrackData.GetValue(fldName).ToSafeString() != currentVersionTrackData[fldName].ToSafeString()))
                        {
                            var fld = allListFields.FirstOrDefault(f => f.InternalName.Equals(fldName));
                            if (fld == null)
                            {
                                continue;
                            }
                            vd.Changes.Add(new ChangeInfo
                            {
                                FieldName = fld.DisplayName,
                                FieldValue = fld.GetFieldValueAsText(currentVersionTrackData[fldName])
                            });
                        }
                    }
                }
                if (vd.Changes.Count() == 0)
                {
                    vd.Changes.Add(new ChangeInfo { FieldName = "No changes", FieldValue = string.Empty});
                }
                versions.Add(vd);
            }
            return versions;
        }
    }
    public class VersionDetails
    {
        public string Version { get; private set; }
        public string Modified { get; private set; }
        public string ModifiedBy { get; private set; }
        public List<ChangeInfo> Changes { get; set; }

        public VersionDetails(vwRosterEventTrackData trackData, string modifiedBy)
        {
            this.Version = trackData.Version.ToSafeString() + ".0";
            this.Modified = trackData.CreatedOn.ToString("dd/MM/yyyy HH:mm");
            this.ModifiedBy = modifiedBy;
            this.Changes = new List<ChangeInfo>();
        }
    }

    public class ChangeInfo
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
