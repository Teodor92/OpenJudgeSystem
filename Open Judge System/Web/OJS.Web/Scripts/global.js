﻿$(document).ready(function () {
    'use strict';

    kendo.culture('en-GB');

    if (getCookie('cookies-notification') !== 'ok') {
        $('#cookies-notification').show();
    }

    $('#cookies-notification-button').click(function () {
        $('#cookies-notification').hide();
        setCookie('cookies-notification', 'ok', 3650);
        return false;
    });
});

/* exported CreateExportToExcelButton */
function CreateExportToExcelButton(elementId) {
    'use strict';

    elementId = typeof elementId === 'undefined' ? 'DataGrid' : elementId;
    CreateKendoSubmitParamsButton('export', elementId);
}

/* eslint new-cap: 0 */
// TODO: Convert to lower-case
function CreateKendoSubmitParamsButton(buttonId, elementId) {
    'use strict';

    elementId = typeof elementId === 'undefined' ? 'DataGrid' : elementId;
    var grid = $('#' + elementId).data('kendoGrid');

    // ask the parameterMap to create the request object for you
    var requestObject = (new kendo.data.transports['aspnetmvc-server']({ prefix: '' }))
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
    /* eslint no-underscore-dangle: 0 */
    href = href.replace(/pageSize=([^&]*)/, 'pageSize=' + grid.dataSource._pageSize);

    //update filter descriptor with the filters applied

    href = href.replace(/filter=([^&]*)/, 'filter=' + (requestObject.filter || '~'));

    // Update the 'href' attribute
    $exportLink.attr('href', href);
}

function setCookie(cname, cvalue, exdays) {
    'use strict';

    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = 'expires=' + d.toGMTString();
    document.cookie = cname + '=' + cvalue + '; ' + expires;
}

function getCookie(cname) {
    'use strict';

    var name = cname + '=';
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return '';
}

/* exported calculateRemainingTimeOnClient */
function calculateRemainingTimeOnClient(condownTimerContainerId, remainingTimeFormat, remainingTimeOnServerInMilliseconds) {
    'use strict';

    $('#' + condownTimerContainerId).prepend(remainingTimeFormat);
    var remainingTimeOnServer = parseInt(remainingTimeOnServerInMilliseconds);
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

var Ojs = Ojs || {};
Ojs.KendoControls = Ojs.KendoControls || {};

Ojs.KendoControls.DropDownList = (function () {
    'use strict';

    function alignDropDownToInput(ev) {
        setTimeout(function () {
            var position = $(ev.sender.element).parent().offset();
            var height = $(ev.sender.element).parent().height();

            $('div.k-animation-container').css('top', position.top + height);
            $('div.k-animation-container').css('left', position.left);
        }, 100);
    }

    return {
        alignDropDownToInput: alignDropDownToInput
    };
})();
