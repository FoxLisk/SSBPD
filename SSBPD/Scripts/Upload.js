function validateAccount() {
    var hasErrors = false;
    if ($('#username-create').val().trim().length == 0) {
        $('#usernameError').html("Please enter a username");
        hasErrors = true;
    }
    if ($('#password-create').val().trim().length < 8) {
        $('#passwordError').html("This password is too weak. Please select a password of at least 8 characters.");
        hasErrors = true;
    }
    email = ($('#email-create').val().trim());
    if (!/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(email)) {
        $('#emailError').html("This is not a valid email.");
        hasErrors = true;
    }
    return !hasErrors;
}