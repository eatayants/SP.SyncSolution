<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RosterTableLookupFieldEditorControl.ascx.cs" Inherits="Roster.Presentation.CONTROLTEMPLATES.RosterTableLookupFieldEditorControl" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/15/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/15/InputFormSection.ascx" %>

 <wssuc:InputFormSection runat="server" id="RosterLookupControl" Title="Roster lookup settings">
    <template_inputformcontrols>
        <wssuc:InputFormControl runat="server" LabelText="Select source">
            <Template_Control>
                <asp:DropDownList ID="ddlSource" runat="server" CssClass="ms-ButtonheightWidth" AutoPostBack="true" OnSelectedIndexChanged = "ddlSource_SelectedIndexChanged" Width="300px" />
            </Template_Control>
        </wssuc:InputFormControl>
        <wssuc:InputFormControl id ="fcTable" runat="server" LabelText="Select database table">
            <Template_Control>
                <asp:DropDownList ID="ddlTable" runat="server" CssClass="ms-ButtonheightWidth" AutoPostBack="true" OnSelectedIndexChanged = "ddlTable_SelectedIndexChanged" Width="300px" />
            </Template_Control>
        </wssuc:InputFormControl>
        <wssuc:InputFormControl id ="fcQuery" runat="server" LabelText="Write custom SQL query">
            <Template_Control>
                <asp:HiddenField ID ="hdnCustomQueryName" runat="server"/>
                <asp:TextBox runat="server" ID="txtCustomQuery" CssClass="ms-ButtonheightWidth" TextMode="MultiLine" Rows="10" Width="300px">
                </asp:TextBox><br/>
                <asp:Button runat="server" ID="btnValidateQuery" OnClick="btnValidateQuery_Click" Text=" Validate Query"/><br/>
                <asp:Panel  ID ="pnlValidateResult" runat="server" CssClass="ms-ButtonheightWidth"></asp:Panel>
            </Template_Control>
        </wssuc:InputFormControl>
        <wssuc:InputFormControl runat="server" LabelText="Select key column">
            <Template_Control>
                <asp:DropDownList ID="ddlLookupKeyField" runat="server" CssClass="ms-ButtonheightWidth" Width="300px" />
            </Template_Control>
        </wssuc:InputFormControl>
        <wssuc:InputFormControl runat="server" LabelText="Select display column">
            <Template_Control>
                <asp:DropDownList ID="ddlLookupDispField" runat="server" CssClass="ms-ButtonheightWidth" Width="300px" />
                <asp:Panel runat="server" ID ="pnlExeption"></asp:Panel>
            </Template_Control>
        </wssuc:InputFormControl>
    </template_inputformcontrols>
 </wssuc:InputFormSection>