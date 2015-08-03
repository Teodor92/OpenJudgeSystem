// TODO: Segregate into modules

$(document).ready(function () {
    kendo.culture("en-GB");

    if (getCookie("cookies-notification") != "ok") {
        $("#cookies-notification").show();
    }

    $("#cookies-notification-button").click(function () {
        $("#cookies-notification").hide();
        setCookie("cookies-notification", "ok", 3650);
        return false;
    });
});

function CreateExportToExcelButton(elementId) {
    elementId = typeof elementId !== 'undefined' ? elementId : "DataGrid";
    CreateKendoSubmitParamsButton("export", elementId);
}

function CreateKendoSubmitParamsButton(buttonId, elementId) {
    elementId = typeof elementId !== 'undefined' ? elementId : "DataGrid";
    var grid = $('#' + elementId).data('kendoGrid');
    // ask the parameterMap to create the request object for you
    var requestObject = (new kendo.data.transports["aspnetmvc-server"]({ prefix: "" }))
    .options.parameterMap({
        page: grid.dataSource.page(),
        sort: grid.dataSource.sort(),
        filter: grid.dataSource.filter()
    });

    // Get the export link as jQuery object
    var $exportLink = $('#' + buttonId);

    // Get its 'href' attribute - the URL where it would navigate to
    var href = $exportLink.attr('href');

    // Update the 'page' parameter with the grid's current page
    href = href.replace(/page=([^&]*)/, 'page=' + requestObject.page || '~');

    // Update the 'sort' parameter with the grid's current sort descriptor
    href = href.replace(/sort=([^&]*)/, 'sort=' + requestObject.sort || '~');

    // Update the 'pageSize' parameter with the grid's current pageSize
    href = href.replace(/pageSize=([^&]*)/, 'pageSize=' + grid.dataSource._pageSize);

    //update filter descriptor with the filters applied

    href = href.replace(/filter=([^&]*)/, 'filter=' + (requestObject.filter || '~'));

    // Update the 'href' attribute
    $exportLink.attr('href', href);
}

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toGMTString();
    document.cookie = cname + "=" + cvalue + "; " + expires;
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
}

function calculateRemainingTimeOnClient(condownTimerContainerId, remainingTimeFormat, remainingTimeOnServerInMilliseconds) {
    $('#' + condownTimerContainerId).prepend(remainingTimeFormat);
    var remainingTimeOnServer;
    if (typeof stringValue) {
        remainingTimeOnServer = parseInt(remainingTimeOnServerInMilliseconds);
    }
    else {
        remainingTimeOnServer = remainingTimeOnServerInMillisecond;
    }
    var currentTimeOnClient = new Date();
    var remainingTimeOnClient = currentTimeOnClient;
    remainingTimeOnClient.setTime(currentTimeOnClient.getTime() + remainingTimeOnServer);
    var timer = new countdownTimer({
        year: remainingTimeOnClient.getFullYear(),
        month: remainingTimeOnClient.getMonth(),
        day: remainingTimeOnClient.getDate(),
        hour: remainingTimeOnClient.getHours(),
        minute: remainingTimeOnClient.getMinutes(),
        second: remainingTimeOnClient.getSeconds()
    });

    timer.start();
}

var OjsControls = OjsControls || {};

OjsControls.ClientDisplayHelpers = (function () {

    var api = {};

    api.getYesNoStateTemplate = function (stateId) {
        if (stateId === true) {
            return '<span class="label label-success">Да</span>';
        } else if (stateId === false) {
            return '<span class="label label-info">Не</span>';
        } else {
            return 'Неизвестен';
        }
    };

    api.getLinkTemplate = function (value) {
        return '<a href="' + value + '" target="_blank" rel="nofollow">' + value + '</a>';
    };

    api.getEntryProcesedStateTemplate = function (stateId) {
        if (stateId === 0) {
            return '<span class="label label-danger">Одобрена</span>';
        } else if (stateId === 1) {
            return '<span class="label label-warning">В изчакване</span>';
        } else if (stateId === 2) {
            return '<span class="label label-success">Отхвърлена</span>';
        } else {
            return "Неизвестен";
        }
    };

    api.getTeamRoleTemplate = function (roleId) {
        switch (roleId) {
            case 0:
                return '<span class="label label-success">Лидер</span>';
            case 1:
                return '<span class="label label-primary">Офицер</span>';
            case 2:
                return '<span class="label label-info">Участник</span>';
            default:
                return "Неизвестенa";
        }
    };

    return api;
})();