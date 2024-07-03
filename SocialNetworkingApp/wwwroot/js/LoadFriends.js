$(document).ready(function () {
    var page = 1;
    var lastFriendId = 0;
    var isLoading = false;

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading) {
            isLoading = true;
            lastFriendId = $('#friendsContainer .card').last().data('id')
            page++;
            $.ajax({
                url: '/Friend/GetFriends',
                data: { page: page, lastFriendId: lastFriendId },
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        $('#friendsContainer').append(data);
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