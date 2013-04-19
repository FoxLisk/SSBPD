$(document).ready(function () {
    var playerSearchBar = $("#playerSearch");
    playerSearchBar.focus(function (srcc) {
        if (playerSearchBar.val() == "Search players") {
            playerSearchBar.removeClass("defaultText");
            playerSearchBar.val("");
        }
    });

    playerSearchBar.blur(function () {
        if ($(this).val() == "") {
            $(this).addClass("defaultText");
            $(this).val("Search players");
        }
    });

    playerSearchBar.blur();

    var tournamentSearchBar = $("#tournamentSearch");
    tournamentSearchBar.focus(function (srcc) {
        if (tournamentSearchBar.val() == "Search tournaments") {
            tournamentSearchBar.removeClass("defaultText");
            tournamentSearchBar.val("");
        }
    });

    tournamentSearchBar.blur(function () {
        if ($(this).val() == "") {
            $(this).addClass("defaultText");
            $(this).val("Search tournaments");
        }
    });

    tournamentSearchBar.blur();

    $('td#accountArea').on('click', 'a#loginLink', function (event) {
        event.preventDefault();
        logIn();
    });

    $('td#accountArea').on('click', 'a#logoutLink', function (event) {
        event.preventDefault();
        $.post('/logout', function (data) {
            $('td#accountArea').html(data);
        });
    });

    $('td#accountArea').on('keydown', '#login input', function (event) {
        if (event.which != 13) {
            return;
        }
        event.preventDefault();
        logIn();
    });

    $('td#accountArea').on('click', 'a#createAccountViewLink', function (event) {
        event.preventDefault();
        $.get('/createaccount', function (data) {
            $('td#accountArea').html(data);
        });
    });

    $('td#accountArea').on('click', 'a#createAccountLink', function (event) {
        event.preventDefault();
        createAccount();
    });

    $('td#accountArea').on('keydown', '#createAccount input', function (event) {
        if (event.which != 13) {
            return;
        }
        event.preventDefault();
        createAccount();
    });

});

function createAccount() {
    var username = $('#usernameHeader').val();
    var password = $('#passwordHeader').val();
    var emailAddress = $('#emailHeader').val();
    var data = {
        username: username,
        password: password,
        emailAddress: emailAddress
    };
    if ($('#stayLoggedIn').is(":checked")) {
        data.stayLoggedIn = true;
    }
    $.ajax({
        url: "/createAccount",
        data: data,
        type: "POST",
        success: function (data) {
            $('td#accountArea').html(data);
        }
    });
}

function logIn() {
    var data = {
        username: $('#usernameHeader').val(),
        password: $('#passwordHeader').val()
    };
    if ($('#stayLoggedIn').is(":checked")) {
        data.stayLoggedIn = true;
    }
    $.ajax({
        url: "/login",
        data: data,
        type: "POST",
        success: function (data) {
            $('td#accountArea').html(data);
        }
    });
}

function showLoadingWidget(element) {
    element.siblings('.loading').removeClass('hidden');
    element.addClass('hidden');
}