"use strict"
$(document).ready(function() {
    var emailPattern = /\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}\b/;

    $("#contact_form").submit( function (evt) {
        var isValid = false;
        var email = $("#email").val().trim();

        if($("#email").val() == "")
        {
            $("#email").next().text("This field needs to be filled");
        }
        else if(!emailPattern.test(email))
        {
            $("#email").next().text("Please put in a valid email");
        }
        else
        {
            isValid = true;
            $("#email").val($("#email").val().trim());
            $("#email").next().text("");
        }

        if(isValid == false)
        {
            evt.preventDefault();
        }
    });
}); // end ready