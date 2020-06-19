function ULS_SP() {
    if (ULS_SP.caller) {
        ULS_SP.caller.ULSTeamName = "Windows SharePoint Services 4";
        ULS_SP.caller.ULSFileName = "/_layouts/15/Roster.Presentation/js/PageComponent.js";
    }
}

Type.registerNamespace('Roster');

// RibbonApp Page Component
Roster.PageComponent = function () {
    ULS_SP();
    Roster.PageComponent.initializeBase(this);
}

Roster.PageComponent.initialize = function () {
    ULS_SP();
    //ExecuteOrDelayUntilScriptLoaded(Function.createDelegate(null,
    //  Roster.PageComponent.initializePageComponent), 'SP.Ribbon.js');
    SP.SOD.executeOrDelayUntilEventNotified(Roster.PageComponent.initializePageComponent, "sp.bodyloaded");
}

Roster.PageComponent.initializePageComponent = function () {
    ULS_SP();
    var ribbonPageManager = SP.Ribbon.PageManager.get_instance();
    if (null !== ribbonPageManager) {
        ribbonPageManager.addPageComponent(Roster.PageComponent.instance);
        ribbonPageManager.get_focusManager().requestFocusForComponent(
        Roster.PageComponent.instance);
    }
}

Roster.PageComponent.refreshRibbonStatus = function () {
    SP.Ribbon.PageManager.get_instance().get_commandDispatcher().executeCommand(Commands.CommandIds.ApplicationStateChanged, null);
}

Roster.PageComponent.prototype = {
    getFocusedCommands: function () {
        ULS_SP();
        return [];
    },
    getGlobalCommands: function () {
        ULS_SP();
        return getGlobalCommands();
    },
    isFocusable: function () {
        ULS_SP();
        return true;
    },
    receiveFocus: function () {
        ULS_SP();
        return true;
    },
    yieldFocus: function () {
        ULS_SP();
        return true;
    },
    canHandleCommand: function (commandId) {
        ULS_SP();
        return commandEnabled(commandId);
    },
    handleCommand: function (commandId, properties, sequence) {
        ULS_SP();
        return handleCommand(commandId, properties, sequence);
    }
}

// Register classes
ExecuteOrDelayUntilScriptLoaded(function () {
    Roster.PageComponent.registerClass('Roster.PageComponent', CUI.Page.PageComponent);
    Roster.PageComponent.instance = new Roster.PageComponent();
}, "cui.js");

// Notify waiting jobs
NotifyScriptLoadedAndExecuteWaitingJobs("/_layouts/15/Roster.Presentation/js/PageComponent.js")