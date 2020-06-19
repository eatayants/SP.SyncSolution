// Re-route SharePoint's inplview.aspx via method hijacking.
if (true) { //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    // run update on PageLoad and before 'ProcessDefaultOnLoad' function
    _spBodyOnLoadFunctionNames.push("updateOnLoad");

    function updateOnLoad() {
        ExecuteOrDelayUntilScriptLoaded(reRouteInplViewUrl, "inplview.js");
    }

    var oldCLVPInplViewUrl = null;
    var augCLVPInplViewUrl = function () {
        var spResolvedUrl = oldCLVPInplViewUrl();
        return spResolvedUrl.replace("inplview.aspx", "Roster.Presentation/InplViewAdv.aspx");
    };
    var reRouteInplViewUrl = function () {
        if (oldCLVPInplViewUrl == null) {
            oldCLVPInplViewUrl = CLVPInplViewUrl;
            CLVPInplViewUrl = augCLVPInplViewUrl;
            CLVP.prototype.InplViewUrl = augCLVPInplViewUrl;
        }
    };

    var oldOnIframeLoad = null;
    var newOnIframeLoad = function () {
        oldOnIframeLoad.apply(this, arguments);
        
        var _oper = '';
        var fieldName = filterTable.getAttribute('Name')
        var opers = m$('input[id$="hidFilterOper"]')[0].value.split(';#');
        for (var j = 0; j < opers.length; j += 2) {
            if (opers[j] == fieldName) {
                _oper = ((j + 1) < opers.length) ? opers[j + 1].toLowerCase() : 'or';
                break;
            }
        }

        var _itmText = '';
        switch (_oper) {
            case 'and':
                _itmText = 'Matches all';
                break;
            case 'or':
                _itmText = 'Matches one';
                break;
            case 'not':
                _itmText = 'Does not match';
                break;
            default:
                _itmText = 'Matches one';
                break;
        }

        var _myMenu = m$(filterTable).parent().find('ul.ms-core-menu-list');
        var menuItems = _myMenu.find('li');

        for (var i = 0; i < menuItems.length; i++) {
            var curElem = menuItems[i];
            var curClassName = curElem.className;
            if (curClassName === 'ms-core-menu-separator') {
                // init New menu item
                var menuItm = document.createElement('li');
                menuItm.className = 'ms-core-menu-item ms-core-menu-itemDisabled';
                var menuItm_a = document.createElement('a');
                menuItm_a.href = 'javascript:;';
                menuItm_a.className = 'ms-core-menu-link';
                var menuItm_div_ico = document.createElement('div');
                menuItm_div_ico.className = 'ms-core-menu-icon';
                var menuItm_div_lbl = document.createElement('div');
                menuItm_div_lbl.className = 'ms-core-menu-labelCompact';
                var menuItm_span_dis = document.createElement('span');
                menuItm_span_dis.className = 'ms-accessible ms-core-menu-disabledText';
                menuItm_span_dis.innerHTML = 'Disabled';
                var menuItm_span_tit = document.createElement('span');
                menuItm_span_tit.className = 'ms-core-menu-title';
                menuItm_span_tit.innerHTML = _itmText
                var menuItm_span_acc = document.createElement('span');
                menuItm_span_acc.className = 'ms-accessible';

                // append
                menuItm_div_lbl.appendChild(menuItm_span_dis);
                menuItm_div_lbl.appendChild(menuItm_span_tit);
                menuItm_a.appendChild(menuItm_div_ico);
                menuItm_a.appendChild(menuItm_div_lbl);
                menuItm_a.appendChild(menuItm_span_acc);
                menuItm.appendChild(menuItm_a);

                // insert to DOM
                _myMenu[0].insertBefore(menuItm, curElem);

                break;
            } else {
                curElem.style.display = 'none';
            }
        }
    };
    var reRouteOnIframeLoad = function () {
        if (oldOnIframeLoad == null) {
            oldOnIframeLoad = OnIframeLoad;
            OnIframeLoad = newOnIframeLoad;
        }
    };

    var oldAddFilterMenuItems = null;
    var newAddFilterMenuItems = function (menu, menuLoading) {
        if (IsFieldNotFilterable(filterTable)) {
            addFilteringDisabledMenuItem(menu);
            return;
        }
        var iframe = document.getElementById("FilterIframe" + filterTable.getAttribute('CtxNum'));

        if (iframe == null)
            return;
        var strDocUrl = ctxFilter.queryString;

        if (null == strDocUrl || strDocUrl == "") {
            strDocUrl = iframe.getAttribute('FilterLink');
        }
        if (strDocUrl == null || strDocUrl == "") {
            window.alert("Unexpected");
        }
        if (strDocUrl == '?') {
            var strHash = ajaxNavigate.getParam("InplviewHash" + ctxFilter.view);

            if (Boolean(strHash))
                strDocUrl += DecodeHashAsQueryString(strHash);
        }
        var strFilterField = escapeProperly(filterTable.getAttribute('Name'));

        strFilteredValue = null;
        var strFilterQuery = "";
        var filterNo = 0;
        var arrayField;
        var arrayValue;

        do {
            filterNo++;
            var multi = false;

            arrayField = strDocUrl.match(new RegExp("FilterField" + String(filterNo) + "=[^&#]*"));
            if (!Boolean(arrayField))
                arrayField = strDocUrl.match(new RegExp("FilterFields" + String(filterNo) + "=[^&#]*"));
            arrayValue = strDocUrl.match(new RegExp("&FilterValue" + String(filterNo) + "=[^&#]*"));
            if (!Boolean(arrayValue)) {
                arrayValue = strDocUrl.match(new RegExp("&FilterValues" + String(filterNo) + "=[^&#]*"));
                multi = true;
            }
            if (arrayField != null && arrayValue != null) {
                if (strFilteredValue == null) {
                    strFilteredValue = getFilterValueFromUrl(arrayField.toString() + arrayValue.toString(), strFilterField);
                    bIsMultiFilter = multi;
                }
                strFilterQuery = strFilterQuery + "&" + arrayField.toString() + arrayValue.toString();
                var arrayOp = strDocUrl.match(new RegExp("&FilterOp" + String(filterNo) + "=[^&#]*"));

                if (arrayOp != null)
                    strFilterQuery = strFilterQuery + arrayOp.toString();
                var arrayLookupId = strDocUrl.match(new RegExp("&FilterLookupId" + String(filterNo) + "=[^&#]*"));

                if (arrayLookupId != null)
                    strFilterQuery = strFilterQuery + arrayLookupId.toString();
                var arrayData = strDocUrl.match(new RegExp("&FilterData" + String(filterNo) + "=[^&#]*"));

                if (arrayData != null)
                    strFilterQuery = strFilterQuery + arrayData.toString();
                if (arrayLookupId != null && arrayData == null && strFilteredValue != null) {
                    addFilteringDisabledMenuItem(menu);
                    return;
                }
            }
        } while (null != arrayField);
        var bFiltered = strFilteredValue != null;
        var strDisplayText = StBuildParam(Strings.STS.L_DontFilterBy_Text, filterTable.getAttribute('DisplayName'));
        var strFilterUrl = "javascript:HandleFilter(event, '" + STSScriptEncode(FilterFieldV3(ctxFilter.view, strFilterField, "", 0, ctxFilter.queryString, true)) + "')";
        var strImageUrl;

        if (bFiltered)
            strImageUrl = GetThemedImageUrl("DeleteFilterGlyph.png");
        else
            strImageUrl = GetThemedImageUrl("DisabledDeleteFilterGlyph.png");
        CAMOptFilter(menu, menuLoading, strDisplayText, strFilterUrl, strImageUrl, bFiltered, "fmi_clr");
        var mi = CAMOpt(menuLoading, Strings.STS.L_Loading_Text, "");

        mi.setAttribute("enabled", "false");
        setTimeout("ShowFilterLoadingMenu()", 500);
        menuLoading._onDestroy = OnMouseOutFilter;
        arrayField = strDocUrl.match(new RegExp("MembershipGroupId=[^&]*"));
        if (arrayField != null) {
            strFilterQuery = strFilterQuery + "&" + arrayField.toString();
        }
        arrayField = strDocUrl.match(new RegExp("InstanceID=[^&]*"));
        if (arrayField != null) {
            strFilterQuery = strFilterQuery + "&" + arrayField.toString();
        }
        if (strFilterQuery != null && strFilterQuery.length > 0) {
            if (ctxFilter.overrideFilterQstring != null && ctxFilter.overrideFilterQstring.length > 0) {
                strFilterQuery = "&" + ReconcileQstringFilters(strFilterQuery.substring(1), ctxFilter.overrideFilterQstring);
            }
        }
        else {
            if (ctxFilter.overrideFilterQstring != null && ctxFilter.overrideFilterQstring.length > 0) {
                strFilterQuery = "&" + ctxFilter.overrideFilterQstring;
            }
        }
        var strRootFolder = "";
        var clvp;

        if (ctxFilter != null && (clvp = ctxFilter.clvp) != null && clvp.rootFolder != null && clvp.rootFolder.length > 0) {
            strRootFolder = "&RootFolder=" + escapeProperlyCore(clvp.rootFolder, true);
        }
        else {
            arrayField = strDocUrl.match(new RegExp("RootFolder=[^&]*"));
            if (arrayField != null)
                strRootFolder = "&" + arrayField.toString();
        }
        var strOverrideScope = "";

        arrayField = strFilterQuery.match(new RegExp("OverrideScope=[^&]*"));
        if (ctxFilter != null && typeof ctxFilter.overrideScope != "undefined" && arrayField == null) {
            strOverrideScope = "&OverrideScope=" + escapeProperlyCore(ctxFilter.overrideScope, false);
        }
        if (browseris.safari) {
            iframe.src = "/_layouts/15/blank.htm";
            iframe.style.offsetLeft = "-550px";
            iframe.style.offsetTop = "-550px";
            iframe.style.border = "0px";
            iframe.style.display = "block";
        }

        // CUSTOM CODE :: remove filters from URL
        iframe.src = ctxFilter.HttpRoot + "/_layouts/15/filter.aspx?ListId=" + ctxFilter.listName + strRootFolder + strOverrideScope + "&FieldInternalName=" + strFilterField + "&ViewId=" + ctxFilter.view + "&FilterOnly=1&Filter=1"; //  + strFilterQuery
        bMenuLoadInProgress = true;
    }
    var reRouteAddFilterMenuItems = function () {
        if (oldAddFilterMenuItems == null) {
            oldAddFilterMenuItems = addFilterMenuItems;
            addFilterMenuItems = newAddFilterMenuItems;
        }
    };

    var oldNotifyOnLoad = NotifyOnLoad;
    NotifyOnLoad = function (sod) {
        if (sod.url.indexOf("inplview.debug.js") > 0 || sod.url.indexOf("inplview.js") > 0) {
            //console.log('notify_impl');
            reRouteInplViewUrl();
        }
        if (sod.url.indexOf("core.debug.js") > 0 || sod.url.indexOf("core.js") > 0) {
            //console.log('notify_core');
            _RefreshPageTo = function (evt, url, bForceSubmit) {
                Sys.Debug.assert(FV4UI());
                EnsureScript("inplview", typeof inplview, null, true);
                reRouteInplViewUrl();
                inplview.RefreshPageTo(evt, url, bForceSubmit);
            };

            // redefine 'OnIframeLoad' function
            reRouteOnIframeLoad();

            // redefine 'addFilterMenuItems' function
            reRouteAddFilterMenuItems();
        }
        oldNotifyOnLoad(sod);
    };
}