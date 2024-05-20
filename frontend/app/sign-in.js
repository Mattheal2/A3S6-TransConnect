// when pressed
$("#show-password-btn").click(function () {
    var password = $("#password-input");
    var passwordType = password.attr("type");
    if (passwordType === "password") {
        password.attr("type", "text");
    } else {
        password.attr("type", "password");
    }
});

$("#login-form").submit(function (event) {
    event.preventDefault();

    var email = $("#email-input").val();
    var password = $("#password-input").val();

    // postJson
    postJson(
        "/api/Auth/Login",
        { email: email, password: password },
        function (response) {
            window.location.href = "/home";
        },
        showError
    );
});

function showError(error) {
    $("#error-message").text(error);
    $("#error-alert").show();
}

function postJson(url, data, success, error) {
    $.ajax({
        url: url,
        method: "POST",
        data: JSON.stringify(data),
        contentType: "application/json",
        cache: false,
        success: function (response) {
            if (response.error) {
                error(response.error.message);
                return;
            }
            success(response);
        },
        error: error
    });
}

$("#email-input, #password-input").keyup(function () {
    $("#error-alert").hide();
});

$("#email-input").focus();