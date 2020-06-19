var ToolTips = ToolTips || {};

ToolTips._ToolTipManager = function () {

    var _divId = "my_tooltip";
    var _innerDivId = "my_tooltip_inner";
    var cachedData = [];
    var headerTemplate = '<table class="ms-formtable" cellpadding="0" cellspacing="0" style="margin: 10px 8px;">';
    var itemTemplate = '<tr><td class="ms-formlabel" valign="top" style="border-top:1px solid #D8D8D8;width:123px;"><h3 class="ms-standardheader">{0}</h3></td><td style="background-color: #efefef; padding: 3px 6px 4px; border-top:1px solid #D8D8D8;" valign="top">{1}</td></tr>';
    var footerTemplate = '</table>';

    this.attachToolTipRawAsync = function (element) {
        $addHandler(element, 'click', function (e) {
            showTooltipAsync(element);
        });
    }

    function showTooltipAsync(element) {

            // get event ID
            var href = m$(element).find('a').attr('href');
            //var _eventId = href.substring(href.indexOf('ID=') + 3);
            var _eventId = GetUrlKeyValue('ID', false, href);
            var _eventListId = GetUrlKeyValue('ListId', false, href);
            var doesEventFromCurrentView = (window.RosterContext.listId.toLowerCase() == _eventListId.replace('{', '').replace('}', '').toLowerCase());

            // for reccurrence events
            var _eventGuid = _eventId;
            if (_eventId.indexOf('.') >= 0) {
                _eventGuid = _eventId.substr(0, _eventId.indexOf('.'));
            }

            var contentWrapper = "<div class=\"ms-cui-tooltip-description\">{0}</div><div class=\"ms-cui-tooltip-clear\"> " +
                                        "</div><hr /><div class=\"ms-cui-tooltip-footer\"><div style=\"width:97%;text-align:right;\">{1}</div></div>";

            if (cachedData[_eventId]) {

                // load tooltip layout from cache
                showTooltip(element, cachedData[_eventId]);

            } else {
                // get selected event properties
                var _viewId = doesEventFromCurrentView ? window.RosterContext.viewId : '00000000-0000-0000-0000-000000000000';
                var _listId = doesEventFromCurrentView ? window.RosterContext.listId : _eventListId;
                Roster.Common.GetPopupContent(_listId, _viewId, _eventGuid, function (data) {
                    var _items = data.d.Items;

                    var contentTypeInternalId = -1;
                    var tooltipHtml = [];
                    // add repeater header
                    tooltipHtml.push(headerTemplate);
                    // add rows
                    for (var i = 0; i < _items.length; i++) {
                        if (_items[i].Name == 'ContentTypeInternalId') {
                            contentTypeInternalId = _items[i].Text;
                        } else {
                            tooltipHtml.push(String.format(itemTemplate, _items[i].Name, _items[i].Text));
                        }
                    }
                    // add repeater footer
                    tooltipHtml.push(footerTemplate);

                    // get urls from ContentTypes
                    var contentTypeUrlParam = (contentTypeInternalId == -1) ? '' : ('&ContentTypeId=' + contentTypeInternalId);
                    var ctInfo = window.RosterContext.ContentTypes;
                    var contentTypeDispFormUrl = '', contentTypeEditFormUrl = '';
                    for (var t = 0; t < ctInfo.length; t++) {
                        if (ctInfo[t].Id.toString() == contentTypeInternalId) {
                            contentTypeDispFormUrl = (GetUrlKeyValue('ContentTypeId', false, ctInfo[t].DispFormUrl)) ? ctInfo[t].DispFormUrl : (ctInfo[t].DispFormUrl + contentTypeUrlParam);
                            contentTypeEditFormUrl = (GetUrlKeyValue('ContentTypeId', false, ctInfo[t].EditFormUrl)) ? ctInfo[t].EditFormUrl : (ctInfo[t].EditFormUrl + contentTypeUrlParam);
                            break;
                        }
                    }

                    var viewFormUrl = '', editFormUrl = '';
                    if (doesEventFromCurrentView) {
                        // current item  belongs to displayed VIEW
                        viewFormUrl = String.format('{0}&ListId={1}&ID={2}', contentTypeDispFormUrl, window.RosterContext.listId, _eventGuid);
                        editFormUrl = String.format('{0}&ListId={1}&ID={2}', contentTypeEditFormUrl, window.RosterContext.listId, _eventGuid);
                    } else {
                        // item is not from a current list (it is Working roster on a Timesheet view)
                        viewFormUrl = href;
                    }

                    var tooltipButtons = [];
                    tooltipButtons.push(String.format('<button onclick=\"OpenPopUpPage(\'{0}\', null, 600, 700)\">View Item</button>', viewFormUrl));
                    if (_eventId == _eventGuid && !SP.ScriptHelpers.isNullOrUndefinedOrEmpty(editFormUrl))
                        tooltipButtons.push(String.format('<button onclick=\"OpenPopUpPage(\'{0}\', null, 600, 700)\">Edit Item</button>', editFormUrl));

                    var completeHtml = String.format(contentWrapper, tooltipHtml.join(''), tooltipButtons.join(''));
                    cachedData[_eventId] = completeHtml; // cache data

                    // show tooltip
                    showTooltip(element, completeHtml);
                });
            }
    }

    function showTooltip(element, rawHtml) {

        var tooltipDiv = $get(_divId);
        if (tooltipDiv == null)
            tooltipDiv = createTooltip();

        $get(_innerDivId).innerHTML = rawHtml;

        displayTooltipNextToElement(tooltipDiv, element);
    }

    function displayTooltipNextToElement(tooltipDiv, element) {

        tooltipDiv.style.display = '';
        var loc = Sys.UI.DomElement.getLocation(element);
        var vpWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
        var vpHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;

        // set 'left' style
        var horizantalOffscreen = (loc.x + tooltipDiv.offsetWidth) - vpWidth;
        if (horizantalOffscreen > 0) {
            tooltipDiv.style.left = loc.x - horizantalOffscreen + 'px';
        } else {
            tooltipDiv.style.left = loc.x + 'px';
        }

        // set 'top' style
        var verticalOffscreen = loc.y + element.offsetHeight + tooltipDiv.offsetHeight - vpHeight;
        if (verticalOffscreen > 0) {
            tooltipDiv.style.top = loc.y + element.offsetHeight - verticalOffscreen + 'px';
        } else {
            tooltipDiv.style.top = loc.y + element.offsetHeight - 1 + 'px';
        }

        if (tooltipDiv.curTimeout != null)
            clearTimeout(tooltipDiv.curTimeout);
    }

    function createTooltip() {
        var mainDiv = document.createElement('span');
        mainDiv.id = _divId;
        mainDiv.className = 'ms-cui-tooltip';
        mainDiv.style.width = 'auto';
        mainDiv.style.position = 'absolute';

        var bodyDiv = document.createElement('div');
        bodyDiv.className = 'ms-cui-tooltip-body';
        bodyDiv.style.width = 'auto';
        bodyDiv.style.backgroundColor = '#F6F6F6';

        var innerDiv = document.createElement('div');
        innerDiv.id = _innerDivId;
        innerDiv.className = 'ms-cui-tooltip-glow';
        innerDiv.style.width = 'auto';

        var closeSpan = document.createElement('span');
        closeSpan.style.position = 'absolute';
        closeSpan.style.top = '0';
        closeSpan.style.right = '5px';
        closeSpan.style.cursor = 'pointer';
        closeSpan.innerHTML = ' X ';

        bodyDiv.appendChild(innerDiv);
        bodyDiv.appendChild(closeSpan);

        mainDiv.appendChild(bodyDiv);

        document.body.appendChild(mainDiv);

        // add handler
        $addHandler(closeSpan, 'click', hideDiv);

        return mainDiv;
    }
    function hideDiv() {
        var dd = $get(_divId);
        if (dd)
            dd.style.display = 'none';
    }
    function showDiv() {
        var dd = $get(_divId);
        if (dd)
            dd.style.display = '';
    }
}

ToolTips.ToolTipManager = new ToolTips._ToolTipManager();