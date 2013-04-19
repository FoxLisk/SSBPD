
function hideLoadingFancybox(element) {
    element.siblings('.loading').addClass('hidden');
    element.removeClass('hidden');
}

function showLoadingFancybox(elements, idx, opts) {
    var element = $(elements[idx]);
    element.siblings('.loading').removeClass('hidden');
    element.addClass('hidden');
}


function fancyboxLoaded(elements, idx, opts) {
    var el = $(elements[idx]);
    hideLoadingFancybox(el);
    bindLinks();
}

function bindLinks() {
    bindUpdate();
    bindAddVideo();
    bindSubmitCharacterFlags();
    bindReportLinks();
    bindModLinks();
}

function bindModLinks() {
    $('a.delete-setlink').on('click', function (event) {
        event.preventDefault();
        var _this = $(this);
        var setLinkId = _this.attr('setlinkid');
        $.post('/deleteLink/' + setLinkId, {}, function (resp) {
            _this.parent().html(resp.response);
        });
    });

    $('a.rename-setlink').on('click', function (event) {
        event.preventDefault();
        var _this = $(this);
        var setLinkId = _this.attr('setlinkid');
        var setLinkName = _this.siblings('input').val();
        $.post('/renameLink/' + setLinkId, { newTitle: setLinkName }, function (resp) {
            if (resp.error) {
                _this.parent().html(resp.error);
            } else {
                _this.siblings('a.link').text(setLinkName);
            }
        });
    });
}

function bindReportLinks() {
    $('a.report-link').on('click', function (event) {
        event.preventDefault();
        var span = $(this).parent();
        span.addClass('hidden');
        span.siblings('span.report').removeClass('hidden');
    });

    $('a.flag-link').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var setLinkId = self.attr('setLinkId');
        $.post('/flagLink/' + setLinkId, '', function (resp) {
            self.parent().html(resp.response);
        });
    });
}

function bindSubmitCharacterFlags() {
    $('a.submitCharacterFlags').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var form = self.parent();
        var setId = form.attr('setId');
        var winnerChar = form.children('select[name=winnerCharacter]').val();
        var loserChar = form.children('select[name=loserCharacter]').val();
        if (winnerChar) {
            submitCharacterFlag(setId, winnerChar, true);
        }
        if (loserChar) {
            submitCharacterFlag(setId, loserChar, false);
        }
        form.html("Thank you!");
    });
}

function submitCharacterFlag(setId, characterId, isWinner) {
    var data = { characterValue: characterId, winnerFlag: isWinner };
    $.post('/flagCharacter/set/' + setId, data, function () { });
}


function bindUpdate() {
    $('a.setUpdate').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var form = self.parent();
        var setId = self.attr('setId');
        var data = form.serialize();
        $.post('/admin/updateSet/' + setId, data, function (resp) {
            self.parent().html(resp.response);
        });
    });
}
function bindAddVideo() {
    $('a.addVideo').on('click', function (event) {
        event.preventDefault();
        var self = $(this);
        var form = self.parent();
        var setId = self.attr('setId');
        var data = form.serialize();
        $.post('/set/addVideo/' + setId, data, function (resp) {
            self.parent().html(resp.response);
        });
    });
}