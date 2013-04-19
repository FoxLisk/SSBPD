$(document).ready(function () {
    $('a#updatePlayerLink').on('click', function (event) {
        event.preventDefault();
        var postData = $('form#update').serialize();
        var playerId = parseInt($('input#playerId').val());
        var self = $(this);
        $.post('/admin/updateplayer/' + playerId, postData, function (data) {
            self.parent().html(data.response);
        });
    });

    $('div#deletePlayer').on('click', 'a', function (event) {
        event.preventDefault();
        var errorEl = $('p#deleteError');
        var playerId = parseInt($(this).attr('id'));
        showLoadingWidget($(this));
        $.post('/admin/deleteplayer/' + playerId, function (data) {
            if (data.response) {
                errorEl.html(data.response);
            }
            else if (data.url) {
                window.location.href = data.url;
            }
        });
    });
});