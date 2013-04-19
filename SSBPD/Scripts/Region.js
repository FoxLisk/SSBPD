$(document).ready(function () {
    $('a#toggleInactive').on('click', function (event) {
        event.preventDefault();
        if ($(this).text() == "Hide inactive players") {
            $(this).text("Show inactive players");
        } else {
            $(this).text("Hide inactive players");
        }
        $.each($('.elo'), function (i, el) {
            $(el).toggleClass('hidden');
        });
        $.each($('li.inactive'), function (i, el) {
            $(el).toggleClass('hidden');
        });
    });
});