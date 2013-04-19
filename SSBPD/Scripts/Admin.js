$(document).ready(function () {
    //admin home
    $('a#toggleProcessedFiles').on('click', function (event) {
        event.preventDefault();
        $('tr.processed').each(function (i, el) {
            $(el).toggleClass('hidden');
        });
        var el = $(this);
        var text = el.text()
        if (text == "Show processed files") {
            el.text("Hide processed files");
        } else {
            el.text("Show processed files");
        }
    });
    $('a#toggleProcessedTournaments').on('click', function (event) {
        event.preventDefault();
        $('tr.elo-processed').each(function (i, el) {
            $(el).toggleClass('hidden');
        });
        var el = $(this);
        var text = el.text()
        if (text == "Show processed tournaments") {
            el.text("Hide processed tournaments");
        } else {
            el.text("Show processed tournaments");
        }
    });
    $('a#resetCharacterCache').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        $.post('/admin/resetcache/character', function (data) {
            self.parent().html(data.response);
        });
    });

    $('table#uploaded-files').on('click', '.process-file', function (event) {
        event.preventDefault();
        var self = $(this);
        var tournamentFileId = this["id"]; //tournamentFile_XX
        showLoadingWidget(self);
        $.post('/admin/processFile/' + tournamentFileId,
            function (data) {
                self.parent().html(data.response);
            });
    });
    $('table#uploaded-files').on('click', '.erase-file', function (event) {
        event.preventDefault();
        var self = $(this);
        var tournamentFileId = this["id"]; //tournamentFile_XX
        showLoadingWidget(self);
        $.post('/admin/eraseTournamentFile/' + tournamentFileId,
            function (data) {
                self.parent().html(data.response);
            });
    });

    $('table#tournaments').on('click', '.process-tournament', function (event) {
        event.preventDefault();
        var self = $(this);
        var tournamentId = this["id"]; //tournament_XX
        showLoadingWidget(self);
        $.post('/admin/processElo/' + tournamentId,
             function (data) {
                 self.parent().html(data.response);
             });
    });
    $('table#tournaments').on('click', '.erase-tournament', function (event) {
        event.preventDefault();
        var self = $(this);
        var tournamentId = this["id"]; //tournament_XX
        showLoadingWidget(self);
        $.post('/admin/eraseTournament/' + tournamentId,
             function (data) {
                 self.parent().html(data.response);
             });
    });
    $('a.unlock-tournament').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var tournamentId = self.attr('tournamentId');
        $.post('/admin/releaselock/' + tournamentId, function (resp) {
            self.parent().html(resp.response);
        });

    });

    $('table#users tr').on('click', 'a', function (event) {
        event.preventDefault();
        var self = this;
        $.post(self.href, function (data) {
            $(self).parent().html(data.response);
        });
    });

    $('a#resetAllEloScores').on('click', function (event) {
        event.preventDefault();
        var areYouSerious = confirm("Seriously? I'm not kidding you, this is going to reset everyone's scores.");
        var self = $(this);
        if (areYouSerious) {
            $.post('/admin/resetEloScores',
                    function (data) {
                        self.parent().html(data.response);
                    });
        }
    });

    $('a#processAllTournaments').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        $.post('/admin/processAllTournaments',
                function (data) {
                    self.parent().html(data.response);
                });
    });


    //flagged players
    $('li.playerTagFlags').on('click', 'a.delete-tag-flags', function (event) {
        event.preventDefault();
        var self = $(this);
        var id = self.attr('id');
        $.post('/admin/erasePlayerFlags/' + id, function (data) {
            self.parent().html(data.response);
        });
    });
    $('li.playerRegionFlags').on('click', 'a.delete-region-flags', function (event) {

        event.preventDefault();
        var self = $(this);
        var id = self.attr('playerId');
        $.post('/admin/erasePlayerRegionFlags/' + id, function (data) {
            self.parent().html(data.response);
        });
    });

    $('li.playerRegionFlags').on('click', 'a.assign-region-link', function (event) {
        event.preventDefault();
        var self = $(this);
        var playerId = parseInt(self.attr('playerId'));
        var regionValue = parseInt(self.attr('regionValue'));
        var postData = { newRegion: regionValue };
        $.post('/admin/updateplayer/' + playerId, postData, function (data) {
            self.parent().html(data.response);
        });
    });
    $('li.playerCharacterFlags').on('click', 'a.assign-character-link', function (event) {
        event.preventDefault();
        var self = $(this);
        var playerId = parseInt(self.attr('playerId'));
        var characterValue = parseInt(self.attr('characterValue'));
        var postData = { newChar: characterValue };
        var overwrite = self.siblings('#overwrite').attr('checked') == "checked";
        postData.overwrite = overwrite;
        $.post('/admin/updateplayer/' + playerId, postData, function (resp) {
            self.parent().html(resp.response);
        });
    });

    $('a.delete-character-flags').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var playerId = parseInt(self.attr('playerId'));
        var postData = { playerId: playerId };
        $.post('/admin/erasePlayerCharacterFlags/' + playerId, function (resp) {
            self.parent().html(resp.response);
        });
    });

    //assign sets
    $('select[name="fromPlayer"]').on('change', function () {
        var playerId = parseInt($(this).val());
        $.get('/admin/tournamentselect/' + playerId, function (html) {
            $('select[name="fromTournament"]').html(html);
        });
    });

    $('a.assign-character-set').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var setId = self.attr('setId');
        var character = self.attr('characterid');
        var isWinners = self.attr('winner');
        if (isWinners) {
            data = { winnerChar: character };
        } else {
            data = { loserChar: character };
        }
        $.post('/admin/updateSet/' + setId, data, function (resp) {
            self.parent().html(resp.response);
        });
    });
    $('a.delete-set-link').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var setLinkId = self.attr('setLinkId');
        $.post('/deleteLink/' + setLinkId, '', function (resp) {
            self.parent().html(resp.response);
        });
    });

});