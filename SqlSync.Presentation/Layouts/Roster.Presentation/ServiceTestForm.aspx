<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ServiceTestForm.aspx.cs" Inherits="Roster.Presentation.Layouts.ServiceTestForm" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <link href="/_layouts/15/Roster.Presentation/js/select2.min.css" type="text/css" rel="stylesheet"/>
    <script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
    <script src="/_layouts/15/Roster.Presentation/js/select2.min.js"></script>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <button type="button" id="test" onclick="callWCF(); return false;"> Call</button>
    <span id="testResponse">

    </span>
    <script type="text/javascript">
        callWCF = function() {
            Roster.Common.ExecuteAction("RorterEvents_PlannedCreate",
            [{ Name: "@eventIds", Value: "DF83A571-FB9F-E8DF-D60D-33C584872A7A" }]);
        };
    </script>
</asp:Content>
