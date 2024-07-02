$(document).ready(function () {
    var page = 1;
    var isLoading = false;

    var filterData = $('#filterData');
    var filters = {
        firstName: filterData.data('first-name'),
        lastName: filterData.data('last-name'),
        city: filterData.data('city'),
        gender: filterData.data('gender'),
        fromAge: filterData.data('from-age'),
        toAge: filterData.data('to-age')
    };

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
                url: '/User/GetFilteredUsersWithFriendStatus',
                data: {
                    page: page,
                    firstName: filters.firstName,
                    lastName: filters.lastName,
                    city: filters.city,
                    gender: filters.gender,
                    fromAge: filters.fromAge,
                    toAge: filters.toAge
                },
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
