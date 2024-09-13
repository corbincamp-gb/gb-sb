var regexString = "^[a-zA-Z0-9\b\n .!,()$@?:_]+$";
var regex = new RegExp(regexString);

var isCommonSetup = false;
var maxTableStringLength = 60;

var changeDetectionEnabled = false;
var disableEditingOnPage = false;

var selectsLoaded = false;

document.onreadystatechange = function () {
    if (document.readyState !== "complete") {
        $('main').css("visibility", "hidden");
    } else {
        HideLoader();
    }
};

function HideLoader() {
    //console.log("HideLoader called");
    $('#loader').css("display", "none");
    $('#loader-bg').css("display", "none");
    $('#loader-text').css("display", "none");
    $('main').css("visibility", "visible");
    $('#generating-file').css("display", "none");
}

$(document).ready(function()
{
    // Handle side nav menu hamburger button clicks
    $('.leftmenutrigger').on('click', function (e) {
        $('.side-nav').toggleClass("open");
        $('#wrapper').toggleClass("open");
        e.preventDefault();
    });

    // Handle section clicks and arrow swapping on side nav
    $('.side-nav .nav-link').on('click', function (e) {
        // Is the menu section already in transition?
        var isCollapsing = $(this).parent('li').children('ul').hasClass('collapsing');

        // If the section isn't in transition already, transition it
        if (isCollapsing == false) {
            if ($(this).find('.menu-arrow').hasClass('fa-chevron-down')) {
                $(this).find('.menu-arrow').removeClass('fa-chevron-down');
                $(this).find('.menu-arrow').addClass('fa-chevron-up');
            }
            else if ($(this).find('.menu-arrow').hasClass('fa-chevron-up')) {
                $(this).find('.menu-arrow').removeClass('fa-chevron-up');
                $(this).find('.menu-arrow').addClass('fa-chevron-down');
            }
        }
    });

    $("a[data-toggle=collapse]").on('click', function (e) {
        var link = $(this).attr('href');

        if ($(link).hasClass("collapsing") == true || $(this).hasClass("nav-link") == true) {
            return;
        }

        if ($(this).attr("aria-expanded") == "true") {
            $(this).find("svg").removeClass("fa-minus");
            $(this).find("i").removeClass("fa-minus");
            $(this).find("svg").addClass("fa-plus");
            $(this).find("i").addClass("fa-plus");
        }
        else {
            $(this).find("svg").removeClass("fa-plus");
            $(this).find("i").removeClass("fa-plus");
            $(this).find("svg").addClass("fa-minus");
            $(this).find("i").addClass("fa-minus");
        }
    });

    //PreventSpecialCharactersInText();

    //$(':text,textarea').bind('paste input', RemoveSpecialCharsOnPaste);
    //$(':text,textarea').bind('drag input', RemoveSpecialCharsOnPaste);
    //$(':text,textarea').bind('drop input', RemoveSpecialCharsOnPaste);

    // Autofocus modal via JS when opened
    /*$('#myModal').on('shown.bs.modal', function () {
        $('#myInput').trigger('focus')
    })*/

    if (isCommonSetup === false) {
        SetupFirstVisit();

        isCommonSetup = true;
    }

    // Change Detection
    if (changeDetectionEnabled) {
        SetupChangeDetection();
    }

    // Setup dark mode based on cookie
    var darkModeCookie = getCookie("darkMode");

    if (darkModeCookie == "true") {
        $('body').removeClass("darkMode").addClass("darkMode");
        $('#light-dark-toggle').bootstrapToggle('on');
    }

    //$('.is-active-toggle').bootstrapToggle('on');

    // Init bootstrap tooltips
    $('#toggle-container').tooltip({ animation: false });
    $('[data-toggle="tooltip"]').tooltip({ animation:false });

    // Dark mode toggle event handler
    $('#light-dark-toggle').change(function () {
        if ($(this).prop('checked')) {
            $('body').removeClass("darkMode").addClass("darkMode");
            setCookie("darkMode", "true", 30);
        }
        else {
            $('body').removeClass("darkMode");
            setCookie("darkMode", "false", 30);
        }
    });

    setTimeout(function () { CheckForSelectLoaded(); }, 250);

    PreventXSSVulnerabilities();    

    SetupCommonAnalyticsEvents();

    // Handle phone number changes
    $('input[type="tel"]').change(function () {
        var tel = $(this);
        var cleaned = ('' + $(tel).val()).replace(/\D/g, '');
        var match = cleaned.match(/^(\d{3})(\d{3})(\d{4})(\d*)$/);
        if (match) {
            $(tel).val('(' + match[1] + ') ' + match[2] + '-' + match[3] + (match[4] != '' ? ' x' + match[4] : ''));
        }
    });

    // Training plan logic
    $('[name="BreakdownCount"]').change(function () {
        var html = '';
        var numOfWeeks = $(this).val();

        html = html + '        <label>Please provide the Title of Training Module, Learning Objective, and Total Hours for each week covered in your SkillBridge program.</label>\n';
        html = html + '        <div class="font-italic">Note: Learning objectives should clearly express how the organizations SkillBridge training objectives align with job competencies for each block of training(usually derived from a job task analysis for the job opportunity).</div>\n';
        html = html + '        <table class="table mt-2">\n';
        html = html + '            <thead>\n';
        html = html + '                <tr>\n';
        html = html + '                    <th>Week</th>\n';
        html = html + '                    <th style="width: 25%;">Title of Training Module <span class="req">&nbsp;*</span></th>\n';
        html = html + '                    <th style="width: 60%;">Learning Objective <span class="req">&nbsp;*</span></th>\n';
        html = html + '                    <th>Total Hours <span class="req">&nbsp;*</span></th>\n';
        html = html + '                </tr>\n';
        html = html + '            </thead>\n';
        html = html + '            <tbody>\n';
        html = html + '                <tr>\n';
        html = html + '                    <td class="font-italic">Ex.</td>\n';
        html = html + '                    <td class="font-italic">Intro to Industry Operations</td>\n';
        html = html + '                    <td class="font-italic">Participant will learn all of the organization\'s roles and their functions</td>\n';
        html = html + '                    <td class="font-italic">8</td>\n';
        html = html + '                </tr>\n';

        for (var i = 0; i < numOfWeeks; i++) {
            html = html + '            <tr>\n';
            html = html + '                <td>' + (i + 1) + ' <input type="hidden" id="TrainingPlanBreakdowns[' + (i) + ']__RowId" name="TrainingPlanBreakdowns[' + (i + 1) + '].RowId" value="' + (i + 1) + '" /></td>\n';
            html = html + '                <td>\n';
            html = html + '                    <input type="text" class="form-control"\n';
            html = html + '                        id="TrainingPlanBreakdowns[' + (i+1) + ']__TrainingModuleTitle"\n';
            html = html + '                        name="TrainingPlanBreakdowns[' + (i+1) + '].TrainingModuleTitle"\n';
            html = html + '                        data-val="true"\n';
            html = html + '                        data-val-required="The training module title for week ' + (i+1) + ' is required."\n';
            html = html + '                        data-toggle="tooltip"\n';
            html = html + '                        title="Enter the training module title for week ' + (i+1) + '"\n';
            html = html + '                    />\n';
            html = html + '                    <span class="text-danger field-validation-valid" data-valmsg-for="TrainingPlanBreakdowns[' + (i+1) + '].TrainingModuleTitle" data-valmsg-replace="true"></span>\n';
            html = html + '                </td>\n';
            html = html + '                <td>\n';
            html = html + '                    <input type="text" class="form-control"\n';
            html = html + '                        id="TrainingPlanBreakdowns[' + (i+1) + ']__LearningObjective"\n';
            html = html + '                        name="TrainingPlanBreakdowns[' + (i+1) + '].LearningObjective"\n';
            html = html + '                        data-val="true"\n';
            html = html + '                        data-val-required="The learning objective for week ' + (i+1) + ' is required."\n';
            html = html + '                        data-toggle="tooltip"\n';
            html = html + '                        title="Enter the learning objective for week ' + (i+1) + '"\n';
            html = html + '                    />\n';
            html = html + '                    <span class="text-danger field-validation-valid" data-valmsg-for="TrainingPlanBreakdowns[' + (i+1) + '].LearningObjective" data-valmsg-replace="true"></span>\n';
            html = html + '                </td>\n';
            html = html + '                <td>\n';
            html = html + '                    <input type="number" class="form-control"\n';
            html = html + '                        id="TrainingPlanBreakdowns[' + (i+1) + ']__TotalHours"\n';
            html = html + '                        name="TrainingPlanBreakdowns[' + (i+1) + '].TotalHours"\n';
            html = html + '                        data-val="true"\n';
            html = html + '                        data-val-required="The total hours for week ' + (i+1) + ' is required."\n';
            html = html + '                        data-toggle="tooltip"\n';
            html = html + '                        data-val-range="Total hours for week ' + (i + 1) + ' must be between 1 and 999."\n';
            html = html + '                        data-val-range-min="1" data-val-range-max="999"\n';
            html = html + '                        title="Enter the total hours for week ' + (i+1) + '"\n';
            html = html + '                    />\n';
            html = html + '                    <span class="text-danger field-validation-valid" data-valmsg-for="TrainingPlanBreakdowns[' + (i+1) + '].TotalHours" data-valmsg-replace="true"></span>\n';
            html = html + '                </td>\n';
            html = html + '            </tr>\n';
        }                                   

        html = html + '        </tbody>\n';
        html = html + '    </table>\n';

        $('#BreakdownUi').html(html);

        // Remove validation.
        $("form").removeData("validator").removeData("unobtrusiveValidation");

        // Add validation again.
        $.validator.unobtrusive.parse("form");

        $(this).focus();
    });


    // Handle sorting on tables
    $('a[data-sortby]').click(function () {
        const link = $(this);

        // Handle the case where the user is toggling the same column
        if ($(link).data('sortby') == $('#SortBy').val()) {
            if ($('#SortOrder').val() == 'asc') {
                $('#SortOrder').val('desc')
            }
            else {
                $('#SortOrder').val('asc')
            }
        }
        else {
            $('#SortOrder').val('asc');
        }

        $('#SortBy').val($(link).data('sortby'));
        $('#' + $(link).closest('tr').data('formid')).submit();
    });
});

// On Ajax Requests Finished
$(document).ajaxStop(function () {
    
});

$(window).resize(function ()
{
    //alert("$(document).resize");
    clearTimeout(window.refresh_size);
    window.refresh_size = setTimeout(function () { update_size(); }, 250);
});


var update_size = function () {
    //alert("update_size");
    if ($("#example").length > 0) {
        //$("#example").css({ width: $("#example").parent().width() });
        $('#example').DataTable()
            .columns.adjust()
            .responsive.recalc();
        //alert("should be resizing");
    }
}

// Wait until select2 dropdowns are loaded, add XSS vulnerability prote
function CheckForSelectLoaded() {
    if (selectsLoaded == false) {
        // If Select2 has been initialized
        if ($('select').hasClass("select2-hidden-accessible")) {
            
            selectsLoaded = true;

            // Prevent XSS issues
            $('select').on('select2:open', function (e) {
                $(".select2-search__field").attr("onCopy", "return false");
                $(".select2-search__field").attr("onDrag", "return false");
                $(".select2-search__field").attr("onDrop", "return false");
                $(".select2-search__field").attr("onPaste", "return false");

                $(".select2-search__field").keypress(function (e) {
                    var regex = new RegExp("^[a-zA-Z0-9\b &(),.:/@!-]+$");
                    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
                    var allowedSpecialKeys = 'ArrowLeftArrowRightArrowUpArrowDownDelete';
                    var key = e.key;

                    /*IE doesn't fire events for arrow keys, Firefox does*/
                    if (allowedSpecialKeys.indexOf(key) > -1) {
                        return true;
                    }

                    if (regex.test(str)) {
                        return true;
                    }

                    return false;
                });
            });
        }
    }
}

function HandleEditViews()
{
    //window.history.pushState({}, document.title, window.location.pathname);

    // If we are just viewing this record, disable all fields and the button
    if (getParameterByName("edit") != "true" || disableEditingOnPage == true) {
        $('main input').attr("disabled", true);
        $('main textarea').attr("disabled", true);
        $('#update-btn').attr("disabled", true).css("display", "none");
        $("main .custom-select").attr("disabled", true);
        $("#find-coords-btn").attr("disabled", true);
        $('.is-active-toggle').bootstrapToggle('disable');
    }
    else {
        $('a.btn-success').remove();
        $("#find-coords-btn").attr("disabled", false);
        $('#rules-modal').modal('show');  // Show the rules modal

        SetupChangeDetection();
        $('.is-active-toggle').bootstrapToggle('enable');
    }

}

function SetupChangeDetection() {
    // Disable the submit button to start
    DisableSubmit();

    if (disableEditingOnPage == false)
    {
        $('input[type="text"]').each(function () {
            $(this).attr("data-start-val", $(this).val());
        });

        $('textarea').each(function () {
            $(this).attr("data-start-val", $(this).val());
        });

        $('input[type="checkbox"]').each(function () {
            // Set an attribute on each field for its starting value
            $(this).attr("data-start-val", $(this).prop("checked"));
        });

        $('input[type="date"]').each(function () {
            // Set an attribute on each field for its starting value
            $(this).attr("data-start-val", $(this).val());
        });

        $('input[type="tel"]').each(function () {
            // Set an attribute on each field for its starting value
            $(this).attr("data-start-val", $(this).val());
        });

        $('input[type="url"]').each(function () {
            // Set an attribute on each field for its starting value
            $(this).attr("data-start-val", $(this).val());
        });

        $("select").each(function () {
            // Set an attribute on each field for its starting value
            $(this).attr("data-start-val", $(this).val());
        });

        $('input').keyup(function (e) {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('textarea').keyup(function (e) {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('input[type="checkbox"]').click(function (e) {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('input[type="radio"]').click(function (e) {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('input[data-toggle="toggle"]').change(function (e) {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('input[type="date"]').change(function () {
            var changed = CheckForDifferences();

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('select').on('select2:select', function (e) {
            var changed = false;

            $("select").each(function () {
                var previousVal = $(this).attr("data-start-val");
                var currentVal = $(this).val();

                //console.log("currentVal sorted: " + currentVal);

                if (previousVal !== currentVal.toString()) {
                    changed = true;
                }
            });

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });

        $('select').on('select2:unselect', function (e) {
            var changed = false;

            $("select").each(function () {
                var previousVal = $(this).attr("data-start-val");
                var currentVal = $(this).val();

                //console.log("currentVal sorted: " + currentVal);

                if (previousVal !== currentVal.toString()) {
                    changed = true;
                }
            });

            if (changed) {
                EnableSubmit();
            }
            else {
                DisableSubmit();
            }
        });
    }
}

function SetupDisabledAlert(isDisabled)
{
    // Show alert if a parent record is disabled
    if (isDisabled == 1) {
        $("#disabled-alert").removeAttr("style").css("display", "block");
        $("#edit-record-btn").attr("disabled", "").css("pointer-events", "none").css("background", "#ccc").css("borderColor", "#6c757d").css("color", "#6c757d");
        disableEditingOnPage = true;
    }
    else {
        disableEditingOnPage = false;
    }
}

function RemoveSpecialCharsOnPaste(e) {
    var self = $(this); 
    setTimeout(function () {
        var initVal = self.val(),
            outputVal = initVal.replace(/[^0-9A-Za-z\b\n .!,()$@?:_]/, "");
        if (initVal != outputVal) self.val(outputVal);
    });
}

function PreventSpecialCharactersInText() {
    "use strict";

    //$("#guided-search-filter-box").attr("onCopy", "return false");
    //$('input[type="text"]').attr("onDrag", "SanitizePaste(this)");
    //$('input[type="text"]').attr("onDrop", "SanitizePaste(this)");
    //$('input[type="text"]').attr("onPaste", "SanitizePaste(this)");

    /*$('input[type="text"]').keypress(function (e) {
        "use strict";
        //alert("text being entered!")
        var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
        var allowedSpecialKeys = 'ArrowLeftArrowRightArrowUpArrowDownDelete';
        var key = e.key;

        //IE doesn't fire events for arrow keys, Firefox does
        if (allowedSpecialKeys.indexOf(key) > -1) {
            return true;
        }

        if (regex.test(str)) {
            return true;
        }

        //alert("NO");
        return false;
    });*/
}

function ShowAllSubPanels(container) {
    //console.log("ShowAllSubPanels called for " + container);
    "use strict";
    if ($("#" + container).length > 0) {
        $("#" + container + " .card").each(function () {
            var headArea = $(this).find(".collapsible-card");
            var expandArea = $(this).find(".collapse");
            $(expandArea).removeClass("show");
            $(expandArea).addClass("show");

            /*if($(headArea).attr("aria-expanded") == "true")
            {
                $(headArea).find("svg").removeClass("fa-minus");
                $(headArea).find("svg").addClass("fa-plus");
            }
            else
            {*/
            $(headArea).find("svg").removeClass("fa-plus");
            $(headArea).find("svg").addClass("fa-minus");
            $(headArea).find("i").removeClass("fa-plus");
            $(headArea).find("i").addClass("fa-minus");
            //}

            $(headArea).attr("aria-expanded", "true");
        });
    }
    else {
        $("." + container + " .card").each(function () {
            var headArea = $(this).find(".collapsible-card");
            var expandArea = $(this).find(".collapse");
            $(expandArea).removeClass("show");
            $(expandArea).addClass("show");

            /*if($(headArea).attr("aria-expanded") == "true")
            {
                $(headArea).find("svg").removeClass("fa-minus");
                $(headArea).find("svg").addClass("fa-plus");
            }
            else
            {*/
            $(headArea).find("svg").removeClass("fa-plus");
            $(headArea).find("svg").addClass("fa-minus");
            $(headArea).find("i").removeClass("fa-plus");
            $(headArea).find("i").addClass("fa-minus");
            //}

            $(headArea).attr("aria-expanded", "true");
        });
    }
}

function HideAllSubPanels(container) {
    "use strict";
    if ($("#" + container).length > 0) {
        $("#" + container + " .card").each(function () {
            var headArea = $(this).find(".collapsible-card");
            var expandArea = $(this).find(".collapse");

            $(expandArea).removeClass("show");

            $(headArea).find("svg").removeClass("fa-minus");
            $(headArea).find("svg").addClass("fa-plus");
            $(headArea).find("i").removeClass("fa-minus");
            $(headArea).find("i").addClass("fa-plus");

            $(headArea).attr("aria-expanded", "false");
        });
    }
    else {
        $("." + container + " .card").each(function () {
            var headArea = $(this).find(".collapsible-card");
            var expandArea = $(this).find(".collapse");

            $(expandArea).removeClass("show");

            $(headArea).find("svg").removeClass("fa-minus");
            $(headArea).find("svg").addClass("fa-plus");
            $(headArea).find("i").removeClass("fa-minus");
            $(headArea).find("i").addClass("fa-plus");

            $(headArea).attr("aria-expanded", "false");
        });
    }
}

// Compare each start value with its current value
function CheckForDifferences() {
    //console.log("CheckForDifferences");
    var changesDetected = false;

    $("textarea").each(function () {
        var previousVal = $(this).attr("data-start-val");
        var currentVal = $(this).val();

        if (previousVal !== currentVal) {
            //console.log("Change Detected!======================");
            //console.log("Field: " + $(this).text());
            //console.log("previousVal: " + previousVal);
            //console.log("currentVal: " + currentVal);
            changesDetected = true;
        }
    });

    // If one has changed, changes are detected
    $("input").each(function () {
        var type = $(this).attr("type");
        //console.log("type: " + type);
        var previousVal = $(this).attr("data-start-val");
        var currentVal = $(this).val();

        if (type == "text") {
            if (previousVal !== currentVal) {
                //console.log("Change Detected!======================");
                //console.log("Field: " + $(this).text());
                //console.log("previousVal: " + previousVal);
                //console.log("currentVal: " + currentVal);
                changesDetected = true;
            }
        }
        else if (type == "checkbox" || type == "radio") {
            var currentVal = $(this).prop("checked").toString();
            if (previousVal != currentVal) {
                //console.log("Change Detected!======================");
                //console.log("Field: " + $(this).parent().text());
                //console.log("previousVal: " + previousVal);
                //console.log("currentVal: " + currentVal);
                changesDetected = true;
            }
        }
        else if (type == "url") {
            if (previousVal != currentVal) {
                //console.log("Change Detected!======================");
                //console.log("Field: " + $(this).parent().text());
                //console.log("previousVal: " + previousVal);
                //console.log("currentVal: " + currentVal);
                changesDetected = true;
            }
        }
        else if (type == "tel") {
            if (previousVal != currentVal) {
                //console.log("Change Detected!======================");
                //console.log("Field: " + $(this).parent().text());
                //console.log("previousVal: " + previousVal);
                //console.log("currentVal: " + currentVal);
                changesDetected = true;
            }
        }
        else if (type == "date") {
            currentVal = $(this).val();
            
            if (previousVal != currentVal) {
                //console.log("Change Detected!======================");
                //console.log("Field: " + $(this).parent().text());
                //console.log("previousVal: " + previousVal);
                //console.log("currentVal: " + currentVal);
                changesDetected = true;
            }
        }
    });

    return changesDetected;
}

function EnableSubmit() {
    $('button[type="submit"].btn-primary').removeAttr("disabled");
}

function DisableSubmit() {
    $('button[type="submit"].btn-primary').attr("disabled", true);
}

// First visit functionality
function SetupFirstVisit() {
    var sessionCookie = getCookie("first_visit");

    if(sessionCookie == "")
    {
        //$('#rules-modal').modal('show');  // Show the rules modal
    }

    //Set cookie to false so first time stuff doesn't happen on next page load
    setCookie("first_visit", "false", 1);		// SET TO 1 instead of 0 WHEN DONE TESTING
}

// Set a session cookie
/*function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + "; " + expires;
}

// Get a session cookie if it exists
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1);
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
}*/

var setCookie = function (name, value, expiracy) {
    var exdate = new Date();
    exdate.setTime(exdate.getTime() + expiracy * 1000);
    var c_value = escape(value) + ((expiracy == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = name + "=" + c_value + '; path=/';
};

var getCookie = function (name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == name) {
            return y ? decodeURI(unescape(y.replace(/\+/g, ' '))) : y; //;//unescape(decodeURI(y));
        }
    }
};

// Datatable ellipsis render functionality
$.fn.dataTable.render.ellipsis = function () {
    return function (data, type, row) {
        return type === 'display' && data.length > maxTableStringLength ?
            data.substr(0, maxTableStringLength) + '…' :
            data;
    }
};

// Get query string parameters
function getParameterByName(name) {
    "use strict";
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function PreventXSSVulnerabilities() {
    "use strict";
    // Table Search
    //$("input").attr("onCopy", "return false");
    //$("input").attr("onDrag", "return false");
    //$("input").attr("onDrop", "return false");
    //$("input").attr("onPaste", "return false");

    $("input[type!='password']").keypress(function (e) {
        var regex = new RegExp("^[a-zA-Z0-9\b &$(),.:/@!-_]+$");
        var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
        var allowedSpecialKeys = 'ArrowLeftArrowRightArrowUpArrowDownDelete';
        var key = e.key;

        /*IE doesn't fire events for arrow keys, Firefox does*/
        if (allowedSpecialKeys.indexOf(key) > -1) {
            return true;
        }

        if (regex.test(str)) {
            return true;
        }

        return false;
    });
}

// Google Analytics Event Tracking
function SetupCommonAnalyticsEvents() {
    /* Other Content Events */
    
    // First Visit Tutorial Invitation Modal Overlay Click
    $('#download-btn').click(function (e) {
        "use strict";
        var text = $(this).text();
        _gaq.push(['_trackEvent', 'Downloads', 'click', text + ' Clicked', , true]);
    });
}

