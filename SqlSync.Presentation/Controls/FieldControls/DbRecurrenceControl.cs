using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using Roster.Presentation.Controls.Fields;
using System.Reflection;
using Roster.Common;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbRecurrenceControl : DbBaseFieldControl
    {
        public const string DivWrapperId = "recurrence_div_wrapper";
        private CheckBox _ctrlCheckbox;
        private Label _ctrlCheckboxComment;
        private CustomValidator _ctrlRecurStartValidator;

        private Label _ctrlLabel;

        private RecurrenceDataControl _crtlRecurrence;

        public DbRecurrenceControl()
        {
           
        }
        public DbRecurrenceControl(CustomValidator ctrlRecurStartValidator)
        {
            _ctrlRecurStartValidator = ctrlRecurStartValidator;
        }

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Field != null)
            {
                if (base.ControlMode == SPControlMode.Display)
                {
                    _ctrlLabel = new Label {ID = Field.InternalName};
                    this.Controls.Add(_ctrlLabel);
                }
                else
                {
                    _ctrlCheckbox = new CheckBox {ID = Field.InternalName};
                    //_ctrlCheckbox.Attributes.Add("onClick", 
                    //    string.Format("document.getElementById('{0}').style.display = this.checked ? 'block' : 'none'", DivWrapperId));
                    _ctrlCheckbox.Attributes.Add("onClick",
                        string.Format("Roster.Common.RecurrenceCheckBoxClickEvent(this.checked, '{0}', ['{1}','{2}'])", DivWrapperId, FieldNames.START_DATE, FieldNames.END_DATE));
                    _ctrlCheckboxComment = new Label {Text = @"Make this a repeating event."};
                    _crtlRecurrence = new RecurrenceDataControl {ItemContext = SPContext.Current};
                    Controls.Add(_ctrlCheckbox);
                    Controls.Add(_ctrlCheckboxComment);
                    Controls.Add(_crtlRecurrence);

                    if (_ctrlRecurStartValidator != null)
                    {
                       Controls.Add(_ctrlRecurStartValidator);
                    }
                    
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            if (ControlMode == SPControlMode.Display)
            {
                _ctrlLabel.RenderControl(writer); // <label/>
            }
            else
            {
                _ctrlCheckbox.RenderControl(writer); // <label/>
                _ctrlCheckboxComment.RenderControl(writer); // <label/>

                writer.Write("<div id=\"{0}\" style=\"display:none\">", DivWrapperId);

                // HACK
                // disable RequiredValidator
                var wsDTCinfo = typeof(RecurrenceDataControl).GetField("m_windowStart_DateTimeControl", BindingFlags.NonPublic | BindingFlags.Instance);
                if (wsDTCinfo != null)
                {
                    var windowStart_DTC = wsDTCinfo.GetValue(_crtlRecurrence) as DateTimeControl;
                    if (windowStart_DTC != null) {
                        windowStart_DTC.IsRequiredField = false;
                        wsDTCinfo.SetValue(_crtlRecurrence, windowStart_DTC);
                    }
                }

                _crtlRecurrence.RenderControl(writer);
                writer.Write("</div>");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (_ctrlCheckbox != null)
            {
                // show/hide Recurrence block on PageLoad + postbacks
                var cstype = this.GetType();
                var csm = Page.ClientScript;
                var csname = "RecurenceLoadHelper" + Field.InternalName;
                //if (!csm.IsClientScriptBlockRegistered(csname) && _ctrlCheckbox.Checked) 
                //{
                    //csm.RegisterClientScriptBlock(cstype, csname, string.Format(
                    //    "SP.SOD.executeOrDelayUntilEventNotified(function () " +
                    //    "{{ document.getElementById('{0}').style.display = 'block'; }}, " +
                    //    "'sp.bodyloaded');", DivWrapperId), true);

                    csm.RegisterClientScriptBlock(cstype, csname, string.Format(
                        "SP.SOD.executeOrDelayUntilEventNotified(function () " +
                        "{{ Roster.Common.RecurrenceCheckBoxClickEvent({3}, '{0}', ['{1}','{2}']); }}, " +
                        "'sp.bodyloaded');", DivWrapperId, FieldNames.START_DATE, FieldNames.END_DATE, _ctrlCheckbox.Checked.ToString().ToLower()), true);
                //}
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return _ctrlCheckbox.Checked ? _crtlRecurrence.Value : string.Empty;
            }
            set
            {
                EnsureChildControls();
                if (value == null) return;
                if (base.ControlMode == SPControlMode.Display)
                {
                    var dbFieldRecurrence = Field as DbFieldRecurrence;
                    if (dbFieldRecurrence != null)
                    {
                        _ctrlLabel.Text = (string)(dbFieldRecurrence.GetFieldValue(value)) ?? string.Empty;
                    }
                }
                else
                {
                    _ctrlCheckbox.Checked = !string.IsNullOrWhiteSpace(value.ToSafeString());
                    _crtlRecurrence.Value = value.ToString();
                }
            }
        }
    }
}
