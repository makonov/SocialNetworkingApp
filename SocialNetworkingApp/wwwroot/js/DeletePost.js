$(document).ready(function () {
    $(document).on('click', '.delete-btn', function (event) {
        event.preventDefault();
        var postId = $(this).data('id');
        var post = $(this).closest('.card');
        console.log("postId:", postId);
        $.ajax({
            type: 'POST',
            url: '/Feed/DeletePost',
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