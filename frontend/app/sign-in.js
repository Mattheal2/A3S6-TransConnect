// when pressed
$("#show-password-btn").click(function() {
    var password = $("#password-input");
    var passwordType = password.attr("type");
    if (passwordType === "password") {
        password.attr("type", "text");
    } else {
        password.attr("type", "password");
    }
});

$("#login-form").submit(function(event) {
    event.preventDefault();

    var email = $("#email-input").val();
    var password = $("#password-input").val();
    
    // postJson
    $.ajax({
        url: "/api/Auth/Login",
        method: "POST",
        data: JSON.stringify({ email: email, password: password }),
        contentType: "application/json",
        cache: false,
        success: function(response) {
            window.location.href = "/home";
        },
        error: function(msg) {
            showError(msg.message);
        }
    });
});

function showError(error) {
    $("#error-message").text(error);
    $("#error-alert").show();
}