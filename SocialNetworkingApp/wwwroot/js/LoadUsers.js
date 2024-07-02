$(document).ready(function () {
    var page = 1;
    var isLoading = false;

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading) {
            isLoading = true;
            page++;
            $.ajax({
                url: '/User/GetUsersWithFriendStatus',
                data: { page: page},
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        $('#usersContainer').append(data);
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