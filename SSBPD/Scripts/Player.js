$(document).ready(function () {
    drawChart();
    $('a#expandFlag').on('click', function (event) {
        event.preventDefault();
        $('div#flag').toggleClass('hidden');
    });

    $('a#submit').on('click', function (event) {
        event.preventDefault();
        var selectValue = parseInt($('select#toPlayerId').val());
        var newTagValue = $('input#newTagFlag').val();
        if (!(selectValue > 0 || newTagValue.length > 0)) {
            $('p#flagError').html("Please specify either a player or a new tag for this player.");
            return false;
        }
        var data;
        if (selectValue > 0) {
            data = { toPlayerId: selectValue };
        } else {
            data = { newTag: newTagValue };
        }
        data.playerId = $('input#playerId').val();
        $.post('/flagPlayer', data, function (data) {
            $('div#flagWrapper').html(data.response);
        });
    });

    $('a#suggestLink').on('click', function (event) {
        event.preventDefault();
        var regionValue = parseInt($('select#regionValue').val());
        var characterValue = parseInt($('select#characterValue').val());
        var newPlayer = parseInt($('select#toPlayerId').val());
        var newTag = $('input#newTagFlag').val();

        if (!(regionValue || characterValue || newPlayer || newTag)) {
            $(this).text("Please suggest something.");
            return false;
        }
        var playerId = $('input#playerId').val();
        if (regionValue) {
            suggestRegion(playerId, regionValue);
        }
        if (characterValue) {
            suggestCharacter(playerId, characterValue);
        }
        if (newTag || newPlayer) {
            suggestTag(playerId, newTag, newPlayer);
        }
        $(this).replaceWith("Thank you for your suggestions!");
    });
    $('a.set-detail').fancybox({
        'onStart': showLoadingFancybox,
        'onComplete': fancyboxLoaded,
        'onCancel': hideLoadingFancybox
    });
    $('a#hideSets').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        if (self.text() == "Only sets with videos") {
            self.text("Show all sets");
        } else {
            self.text("Only sets with videos");
        }
        $('ul#recentSets li.novideo').toggleClass('hidden');
    });
});

function suggestTag(playerId, newTag, newPlayer) {
    var data;
    if (newPlayer) {
        data = { toPlayerId: newPlayer };
    } else {
        data = { newTag: newTag };
    }
    data.playerId = playerId;
    $.post('/flagPlayer', data, function (data) {
        $('div#flagWrapper').html(data.response);
    });
}

function suggestRegion(playerId, regionValue) {
    var data = { regionValue: regionValue };
    data.playerId = playerId;
    $.post('/flagRegion/' + data.playerId, data, function (data) {
    });
}

function suggestCharacter(playerId, characterValue) {
    var data = { characterValue: characterValue };
    data.playerId = playerId;
    $.post('/flagCharacter/player/' + data.playerId, data, function (resp) {
    });
}