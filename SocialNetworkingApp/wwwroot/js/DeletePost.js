$(document).ready(function () {
    $(document).on('click', '.delete-btn', function () {
        var postId = $(this).data('id');
        var post = $(this).closest('.card');
        $.ajax({
            type: 'POST',
            url: '/Post/DeletePost',
            data: { postId: postId },
            success: function (response) {
                if (response.success) {
                    post.remove();
                }
            },
            error: function () {
                console.error('Ошибка при удалении поста')
            }
        });
    });


});