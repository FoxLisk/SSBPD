$(document).ready(function () {
    $('.checkAllRegions').on('change', function () {
        var checkedValue = $(this).is(':checked');
        $(this).siblings('.regionList').find('input').each(function (idx, el) {
            $(el).attr('checked', checkedValue);
        });
    });
    $('.checkAllCharacters').on('change', function () {
        var checkedValue = $(this).is(':checked');
        $(this).siblings('.characterList').find('input').each(function (idx, el) {
            $(el).attr('checked', checkedValue);
        });
    });
    $('a.set-detail').fancybox({
        'onStart': showLoadingFancybox,
        'onComplete': fancyboxLoaded,
        'onCancel': hideLoadingFancybox
    });


});