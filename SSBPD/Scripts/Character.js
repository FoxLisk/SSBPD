$(document).ready(function () {
    $('a.showStats').on('click', function (event) {
        event.preventDefault();
        var el = $(this);
        var charId = el.attr('characterid');
        $('ol[characterid=' + charId + ']').toggleClass('hidden');
        if (el.text() == "show stats") {
            el.text("hide stats");
        } else {
            el.text("show stats");
        }
    });
});