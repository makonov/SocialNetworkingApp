$(document).ready(function () {
    $(document).on('click', '.delete-friend-btn', function () {
        const $card = $(this).closest('.card');
        var $btn = $(this);
        var userId = $(this).data('user-id');
        $.ajax({
            type: 'POST',
            url: '/Friend/DeleteFriend',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    $card.remove();
                }
            },
            error: function () {
                console.error('Ошибка при удалении друга')
            }
        });
    });
});