$(document).ready(function () {
    $(document).on('click', '.like-btn', function (event) {
        event.preventDefault();
        var postId = $(this).data('id');
        var $likeIcon = $(this).find('i');
        var isLiked = $(this).data('is-liked');
        var $likesCount = $(this).closest('.card-body').find('.likes-count');
        var $this = $(this);
        $.ajax({
            type: 'POST',
            url: '/Post/LikePost',
            data: { postId: postId },
            success: function (response) {
                if (response.success) {
                    isLiked = !isLiked;
                    $this.data('is-liked', isLiked);
                    $likeIcon.removeClass('bi-hand-thumbs-up bi-hand-thumbs-up-fill');
                    if (isLiked) {
                        $likeIcon.addClass('bi-hand-thumbs-up-fill');
                    } else {
                        $likeIcon.addClass('bi-hand-thumbs-up');
                    }
                    $likesCount.text(response.likes);

                } else {
                    console.error('Ошибка при постановке/снятии лайка');
                }
            },
            error: function () {
                console.error('Ошибка при выполнении запроса');
            }
        });
    });
});