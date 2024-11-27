$(document).ready(function () {
    $(document).on('click', '.delete-btn', function () {
        var commentId = $(this).data('id');
        var comment = $(this).closest('.card');
        $.ajax({
            type: 'POST',
            url: '/Comment/DeleteComment',
            data: { commentId: commentId },
            success: function (response) {
                if (response.success) {
                    comment.remove();
                }
            },
            error: function () {
                console.error('Ошибка при удалении комментария')
            }
        });
    });
});