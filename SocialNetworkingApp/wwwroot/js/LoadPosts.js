$(document).ready(function () {
    var page = 1;
    var lastPostId = 0;
    var isLoading = false;

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading) {
            isLoading = true;
            lastPostId = $('#postsContainer .card').last().data('id')
            $.ajax({
                url: '/Feed/GetPosts',
                data: { page: page, lastPostId: lastPostId },
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        $('#postsContainer').append(data);
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