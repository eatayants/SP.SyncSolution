<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Page Language="C#" Inherits="Roster.Presentation.Layouts.InplViewAdv" %>
<%@ Register Tagprefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<html>
  <head>
	<SharePoint:CssLink runat="server"/>
	<SharePoint:ScriptLink language="javascript" name="core.js" Localizable="false" runat="server" />
	<SharePoint:ScriptLink Name="sp.js" Localizable="false" LoadAfterUI="true" runat="server" defer="false" />
  </head>
<body>
	<SharePoint:ListSiteMapPath
		runat="server"
		ID="inplContentMap"
		SiteMapProviders="SPSiteMapProvider,SPContentMapProvider"
		RenderCurrentNodeAsLink="false"
		PathSeparator=""
		CssClass="s4-breadcrumb"
		NodeStyle-CssClass="s4-breadcrumbNode"
		CurrentNodeStyle-CssClass="s4-breadcrumbCurrentNode"
		RootNodeStyle-CssClass="s4-breadcrumbRootNode"
		HideInteriorRootNodes="true"
		SkipLinkText="" />

    <div style="margin:5px;" onkeydown="FilterEnterKey(event)">
        <form runat="server">
	        <SharePoint:FormDigest runat="server"/>
        </form>
    </div>
</body>

<SharePoint:ScriptBlock runat="server">
    function ResizeFrameToFit()
    {
	    var frm = window.frameElement;
	    var div = frm.parentNode.parentNode;
	    var divTitle = div.getElementsByTagName("DIV")[0];
	    var button = document.getElementsByName("BtnOk")[0];
	    frm.style.overflow = "visible";
	    div.style.overflow = "visible";
	    var cx = (AbsLeft(button) + button.offsetWidth + 10);
	    var cxpx = "" + cx + "px";
	    frm.style.width = cxpx;
	    div.style.width = cxpx;
    }
    ResizeFrameToFit();
    function FilterEnterKey(evt)
    {
	    var key = evt.keyCode != null ? evt.keyCode : evt.which;
	    if (key == 13)
	    {
		    var but = document.getElementsByName("BtnOk")[0];
		    but.click();
		    return false;
	    }
	    return true;
    }
</SharePoint:ScriptBlock>

</html>
