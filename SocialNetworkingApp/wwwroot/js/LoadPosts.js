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
                data: { page: page, lastPostId: lastPostId},
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

    
    //$('.comment').click(function (e) {
    //    e.preventDefault(); // Отменяем стандартное действие ссылки
    //    var scrollPosition = $(window).scrollTop();

    //    $.ajax({
    //        url: '/Feed/ShowComments',
    //        type: 'GET',
    //        data: { page: page, scrollPosition: scrollPosition },
    //        success: function (response) {
    //            // Обработка успешного ответа от сервера
    //            console.log(response);
    //            // Дополнительные действия по необходимости
    //        },
    //        error: function () {
    //            // Обработка ошибки
    //            console.error('Ошибка при отправке AJAX-запроса');
    //        }
    //    });
    //});
    
});