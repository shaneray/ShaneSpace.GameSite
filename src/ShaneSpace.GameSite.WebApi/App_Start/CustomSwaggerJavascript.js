// If API key is empty show password form
if ($("#input_apiKey").val() === "" && $("#api_selector").find("div").length == 3) {
    $("#header").height("70px");

    $("#input_apiKey").width("215px");

    // build login form
    $("<br><br><div>" +
            "<span id=\"loading\" style=\"display: none\">Authenticating...</span>" +
            "<span id=\"loginForm\"><input placeholder=\"user\" id=\"username\" style=\"width: 100px;\">" +
        " <input placeholder=\"password\" type=\"password\" id=\"pass\"style=\"width: 100px;\"><div>")
        .insertAfter("#input_apiKey");
    $("<br><div style=\"margin-top: 4px;\">" +
            "<a id=\"loginButton\" href=\"#\" style=\"font-weight: bold; text-decoration: none; margin-left: 5px; padding: 6px 8px; font-size: 0.9em; color: white; background-color: #547f00; border-radius: 4px;\">Login</a></span></div>")
        .insertAfter("#explore");

    // events
    $("#loginButton").click(LogIn);

    // submit on enter
    $('#pass').live("keypress", function (e) {
        var code = (e.keyCode ? e.keyCode : e.which);
        if (code == 13) {
            $("#loginButton").click();
        }
    });

    // API Key updated
    $("#input_apiKey").keyup(function () {
        key = $("#input_apiKey").val();

        if (key && key.trim() != "") {
            key = "Bearer " + key;
            swaggerUi.api.clientAuthorizations.add("key", new SwaggerClient.ApiKeyAuthorization("Authorization", key, "header"));
            ShowLoginForm(false);
        }
        else {
            ShowLoginForm(true);

            swaggerUi.api.clientAuthorizations.remove("key");
        }
    });

    // show not auth message
    $(document).ready(function () {
        $("<p id=\"noAuthMessage\" style=\"font-size: 30px; margin-left: auto; margin-right: auto; text-align: center; color: red; font-weight: bold;\">" +
        "Not Authorized. Please log in above.</p><br><br>").insertBefore("#swagger-ui-container");
    });
}


function LogIn() {
    ShowLoginForm(false);

    $("#loading").show();
    var url = "http://ssweb:2022/token?siteKey=631cf4d4-e43d-4cba-be87-20aea46ed147";

    var posting = $.post(url, {
        username: $("#username").val(),
        password: $("#pass").val(),
        grant_type: "password"
    })
        .done(function (data) {
            key = data.access_token;
            $("#input_apiKey").val(key);
            $("#loading").hide();
            $("#input_apiKey").show();
            if (key && key.trim() != "") {
                key = "Bearer " + key;
                swaggerUi.api.clientAuthorizations.add("key", new SwaggerClient.ApiKeyAuthorization("Authorization", key, "header"));
            }
        })
        .fail(function () {
            $("#loading").hide();

            ShowLoginForm(true);

            alert("Could not authenticate.  Please try again.");
        });
}

function ShowLoginForm(booleanValue) {
    if (booleanValue) {
        $("#header").height("70px");

        $("#loginForm").show();
        $("#loginButton").show();
        $("#noAuthMessage").show();
    } else {
        $("#header").height("");

        $("#loginForm").hide();
        $("#loginButton").hide();
        $("#noAuthMessage").hide();
    }
}