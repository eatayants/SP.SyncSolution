using Roster.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation
{
    public static class Constants
    {
        public static class ColourCodingSettings
        {
            public const string EMPTY_CONDITIONS = "[]";
            public const string STYLE_ELEMENT_TEMPLATE = "<style type=\"text/css\" rel=\"stylesheet\">{0}</style>";
            public const string STYLE_BACKGROUND_CLASS_TEMPLATE = ".ms-acal-color{0} {{ background-color: {1}; }}";
            public const string STYLE_COLOR_CLASS_TEMPLATE = ".ms-acal-color{0} a, .ms-acal-color{0} div {{ color: {1} !important; }}";
        }

        public static class ReceiversNames
        {
            public const string SYNC_DATA_ADDED = "SyncListTableAdded";
            public const string SYNC_DATA_UPDATED = "SyncListTableUpdated";
            public const string SYNC_DATA_DELETING = "SyncListTableDeleting";
        }

        public static class Ribbon
        {
            public const string CONTEXTUAL_TAB_TEMPLATE = @"
                <GroupTemplate Id=""Ribbon.Templates.RosterGroup"">
                    <Layout Title=""OneLargeTwoMedium"" LayoutTitle=""OneLargeTwoMedium"">
                        <OverflowSection Type=""OneRow"" DisplayMode=""Large"" TemplateAlias=""ONERW""/>
                    </Layout>
                </GroupTemplate>";

            public const string VIEW_TAB_TEMPLATE = @"
                <Tab Id=""Ribbon.RosterTab"" Title=""View"" Description=""View"" Sequence=""1105"">
                    <Scaling Id=""Ribbon.RosterTab.Scaling"">
                        <MaxSize Id=""Ribbon.RosterTab.MaxSize"" GroupId=""Ribbon.RosterTab.RosterGroup"" Size=""OneLargeTwoMedium""/>
                        <Scale Id=""Ribbon.RosterTab.Scaling.CustomTabScaling"" GroupId=""Ribbon.RosterTab.RosterGroup"" Size=""OneLargeTwoMedium"" />
                    </Scaling>
                    <Groups Id=""Ribbon.RosterTab.Groups"">
                        <Group
                            Id=""Ribbon.RosterTab.RosterGroup""
                            Description=""Actions on Rosters!""
                            Title=""Actions""
                            Sequence=""52""
                            Template=""Ribbon.Templates.RosterGroup"">
                            <Controls Id=""Ribbon.RosterTab.RosterGroup.Controls"">
                                {0}
                            </Controls>
                        </Group>
                    </Groups>
                </Tab>";
        }

        public static class Filters
        {
            public const string VIEW_ALL_FILTERS_KEY = "viewAllFilters";
            public const string FILTER_BY_SINGLE_DATE_KEY = "filterBySingleDate";
        }

        public static class Pages
        {
            public const string HOLIDAY_FORM_PAGE_URL = "/_layouts/15/Roster.Presentation/HolidayFormPage.aspx";
            public const string PERMISSIONS_PAGE_URL = "/_layouts/15/Roster.Presentation/DbListPermissions.aspx";
        }

        public enum EditableType
        {
            Text,
            Numeric,
            Number,
            Checkbox,
            Date,
            Datetime,
            Select,
            Checklist,
            Dropdown,
            Autocomplete,
            Select2,
            Password,
            UserOrGroup
        }

        public enum Role
        {
            [Description("Roster Admins")]
            RosterAdmins
        }
    }
}
