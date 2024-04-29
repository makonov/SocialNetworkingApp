$(document).ready(function () {
    var page = 1;
    var lastPostId = 0;
    var isLoading = false;
    var distance = localStorage.getItem('scrollDistance');

    // Если значение расстояния существует, прокручиваем страницу на это расстояние
    if (distance) {
        $(window).scrollTop(distance);
        console.log(distance);
    }

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading) {
            isLoading = true;
            lastPostId = $('#postsContainer .card').last().data('id')
            /*var distance = measureDistance();*/
            /*console.log(distance);*/
            $.ajax({
                url: '/Feed/GetPosts',
                data: { page: page, lastPostId: lastPostId/*, distance: distance*/ },
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

    function measureDistance() {
        // Получаем верхний пост и прогруженный пост
        var topPost = $('.card:first');
        var loadedPost = $('.card:last');

        // Получаем координаты верхнего поста
        var topPostOffset = topPost.offset();
        var topPostY = topPostOffset.top;

        // Получаем координаты прогруженного поста
        var loadedPostOffset = loadedPost.offset();
        var loadedPostY = loadedPostOffset.top;

        // Вычисляем расстояние между верхним и прогруженным постами по оси Y
        var distance = Math.abs(loadedPostY - topPostY);

        return distance;
    }

    // При покидании страницы сохраняем значение расстояния в localStorage
    $(window).on('beforeunload', function () {
        var distance = measureDistance();
        localStorage.setItem('scrollDistance', distance);
    });
});