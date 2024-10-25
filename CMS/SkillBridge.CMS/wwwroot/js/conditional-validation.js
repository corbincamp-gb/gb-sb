﻿$(document).ready(function() {
    $.validator.addMethod('requiredif',
        function (value, element) {
            var id = '#' + $(element).attr('requiredif-dependentproperty');

            // get the target value (as a string, 
            // as that's what actual value will be)
            var targetvalue = $(element).attr('requiredif-targetvalue');
            targetvalue =
                (targetvalue == null ? '' : targetvalue).toString();

            // get the actual value of the target control
            // note - this probably needs to cater for more 
            // control types, e.g. radios
            var control = $(id);
            var controltype = control.attr('type');
            var actualvalue =
                controltype != 'checkbox' || control.is(':checked') ?
                    control.val() : '';

            // if the condition is true, reuse the existing 
            // required field validator functionality
            if (targetvalue === actualvalue)
                return $.validator.methods.required.call(
                    this, value, element, { dependentproperty: $(element).attr('requiredif-dependentproperty'), targetvalue: $(element).attr('requiredif-targetvalue'), required: $(element).attr('requiredif-required') });

            return true;
        }
    );

    $.validator.unobtrusive.adapters.add(
        'requiredif',
        ['dependentproperty', 'targetvalue'],
        function (options) {
            options.rules['requiredif'] = {
                dependentproperty: options.params['dependentproperty'],
                targetvalue: options.params['targetvalue']
            };
        });

    $.validator.addMethod('requiredgroup',
        function (value, element) {
            var checked = $('[name="' + $(element).attr('name') + '"]:checked');

            if (checked.length == 0)
                return $.validator.methods.required.call(
                    this, value, element);

            return true;
        }
    );

    $.validator.unobtrusive.adapters.add(
        'requiredgroup',
        [],
        function (options) {
        });

    $.validator.unobtrusive.parse();
});
