$(document).ready(function () {
    $('a#updateTournamentLink').on('click', function (event) {
        event.preventDefault();
        var data = $('form#updateTournament').serialize();
        var tournamentId = $('input#tournamentId').val();
        var self = $(this);
        $.post('/tournament/update/' + tournamentId, data, function (resp) {
            self.replaceWith(resp.response);
            if (resp.newDate) {
                $('#date').text("Played on " + resp.newDate);
            }
            if (resp.newName) {
                $('#tournamentName').text(resp.newName);
            }
        });

    });
    $('a.view-bracket').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        self.siblings().each(function (id, el) {
            $(el).toggleClass('hidden');
        });
        if (self.text() == "Show bracket") {
            self.text("Hide bracket");
        } else {
            self.text("Show bracket");
        }
    });
    $('div.viewBracket').each(function (id, el) {
        var self = $(el);
        var tournamentId = $('input#tournamentId').val();
        var bracketName = self.attr('bracketName');
        $.get('/tournament/bracket/' + tournamentId + '/' + bracketName, function (resp) {
            self.html(resp);
            self.siblings('.loading').remove();
        });
    });
    $('a.edit-fancybox').fancybox({
        'onStart': showLoadingFancybox,
        'onComplete': fancyboxLoaded,
        'onCancel': hideLoadingFancybox

    });

});
