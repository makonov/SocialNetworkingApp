$(document).ready(function () {
    function initInfiniteScroll(containerId, loadUrl, extraData = {}) {
        var page = 1;
        var lastItemId = 0;
        var isLoading = false;

        function loadMore() {
            if (!isLoading) {
                isLoading = true;
                lastItemId = $(`#${containerId} .card`).last().data('id');
                /*page++;*/
                $.ajax({
                    url: loadUrl,
                    data: { page: page, lastItemId: lastItemId, ...extraData },
                    type: 'GET',
                    success: function (data) {
                        if (data.length > 0) {
                            $(`#${containerId}`).append(data);
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

        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
                loadMore();
            }
        });

        $('.scrollToTop').click(function () {
            $('html, body').animate({ scrollTop: 0 }, 'slow');
            return false;
        });
    }

    // Инициализация для всех контейнеров
    initInfiniteScroll('commentsContainer', '/Comment/GetComments');
    initInfiniteScroll('friendsContainer', '/Friend/GetFriends');
    initInfiniteScroll('postsContainer', '/Post/GetPosts');
    initInfiniteScroll('usersContainer', '/User/GetUsersWithFriendStatus');
});
