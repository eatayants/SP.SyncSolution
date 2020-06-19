var tblHeaders = [];

jQuery(document).ready(function ($) {
    if (RosterContext && RosterContext.QuickEditMode) {
        // get original SPGridView table
        var origTbl = $('table[id$="myRostersGridView"]');

        // collect DATA from all cells
        var gridData = origTbl.find('> tbody > tr:not(:first)').map(function (tr) {
            return [$(this).children().map(function (td) { return $(this).html().replace(/&nbsp;/g, '') }).get()]
        }).get();

        // clear last empty row
        if (gridData.length > 1 && (gridData[0].length != gridData[gridData.length - 1].length)) {
            gridData.pop();
        }

        // get HEADERS
        var colIdx = 0;
        var visibleColumnIndexes = Enumerable.From(RosterContext.QuickEditSettings).Select(function (x) { return x.data }).ToArray();
        origTbl.find('tr.ms-viewheadertr > th').each(function (index) {
            if ($.inArray(index, visibleColumnIndexes) != -1) {
                tblHeaders[colIdx++] = $(this).html();
            }
        });

        var container = document.getElementById('QuickEditTable');
        var hot = new Handsontable(container, {
            data: gridData,
            maxRows: gridData.length,
            rowHeaders: false,
            contextMenu: false,
            allowInsertRowBoolean: false,
            manualColumnResizeBoolean: false,
            fillHandle: 'vertical',
            disableVisualSelection: 'area',
            minSpareRows: 0,
            colHeaders: function(index) {
                return tblHeaders[index];
            },
            columns: RosterContext.QuickEditSettings,
            afterChange: function (change, source) {
                if (source === 'loadData') {
                    return; //don't save this change
                }

                for (var i = 0; i < change.length; i++) {
                    if (change[i][2] != change[i][3]) {
                        var fieldInfo = Enumerable.From(RosterContext.QuickEditSettings).Where(function (x) { return x.data == change[i][1] }).FirstOrDefault();

                        var resultValue = change[i][3];
                        if (fieldInfo.editor == 'select2' || fieldInfo.editor == 'userorgroup') {
                            // LOOKUP or User
                            resultValue = resultValue.split(';#')[0];
                        } else if (fieldInfo.editor == 'select' || fieldInfo.editor == 'checklist') {
                            // CHOICE
                            resultValue = String.format('<Items>{0}</Items>', Enumerable.From(resultValue.split(',')).Select(function (x) { return '<Value>' + x.trim() + '</Value>' }).ToArray().join(''));
                        } else if (fieldInfo.editor == 'checkbox') {
                            // BOOLEAN
                            resultValue = (fieldInfo.checkedTemplate == resultValue) ? 'true' : 'false';
                        }
                        Roster.Common.SaveRosterItemData(RosterContext.listId, 
                            hot.getDataAtCell(change[i][0], gridData[0].length - 1), fieldInfo.name, resultValue);

                        var childFields = Enumerable.From(RosterContext.QuickEditSettings).Where(function (x) {
                            return x.select2LookupOptions != null && x.select2LookupOptions.ParentData === fieldInfo.data
                        }).Select(function (x) { return x.data }).ToArray();

                        $.each(childFields, function (key, prop) {
                            hot.setDataAtRowProp(change[i][0], prop,"");
                        });
                    }
                }
            }
        });

        // remove header from Original table
        origTbl.find('tr.ms-viewheadertr').remove();

        //remove inline WIDTH
        jQuery(container).find('.wtHolder').css("width", "");
    }

});

function lookupRenderer(instance, tdCell, row, col, prop, value, cellProperties) {
    var escapedVal = Handsontable.helper.stringify(value);
    var parts = escapedVal.split(';#');
    tdCell.innerHTML = parts.length > 1 ? parts[1] : parts[0];
    return tdCell;
}
Handsontable.renderers.registerRenderer('lookupRenderer', lookupRenderer);