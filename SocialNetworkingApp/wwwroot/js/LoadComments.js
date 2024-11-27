$(document).ready(function () {
    var page = 1;
    var lastCommentId = 0;
    var isLoading = false;

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading && $('#commentsContainer .card').length > 0) {
            isLoading = true;
            lastCommentId = $('#commentsContainer .card').last().data('id')

            $.ajax({
                url: '/Comment/GetComments',
                data: { page: page, lastCommentId: lastCommentId },
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        $('#commentsContainer').append(data);
                        page++;
                    }
                    isLoading = false;
                },
                error: function () {
                    isLoading = false;
                }
            });
        }
    }

    $('.scrollToTop').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 'slow');
        return false;
    });
});



