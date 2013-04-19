$(document).ready(function () {
    $('a#sendResetEmail').on('click', function (event) {
        event.preventDefault();
        var email = $('input#email').val();
        $.post('/user/sendReset/' + email, function (resp) {
            $('div#resetPassword').html(resp.response);
        });
    });
    $('a#showCreate').on('click', function (event) {
        event.preventDefault();
        $('form#createCustomRegion').toggleClass('hidden');
    });

    $('a.showCustomRegion').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var crId = self.attr('crId');
        $('ol[crId=' + crId + ']').toggleClass('hidden');
        var text = self.text();
        if (text.indexOf('+') >= 0) {
            self.text(text.replace('+', '-'));
        } else {
            self.text(text.replace('-', '+'));
        }
    });
    $('a.delete-region').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var crId = self.attr('crId');
        $.post('/customregions/delete/' + crId, function (data) {
            if (data.success) {
                $('li[crId=' + crId + ']').remove();
            } else {
                self.html(data.response).contents().unwrap();
            }
        });

    });
    $('form#createCustomRegion input[type=submit]').on('click', function (event) {
        event.preventDefault();
        var data = $('form#createCustomRegion').serialize();
        $.post('/customregions/create', data, function (resp) {
            if (resp.success) {
                location.reload();
            } else {
                location.hash = "";
                $('p#createError').html(resp.response);
            }
        });
    });

});