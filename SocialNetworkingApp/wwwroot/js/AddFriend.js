﻿$(document).ready(function () {
    $(document).on('click', '.add-friend-btn', function () {
        var $btn = $(this);
        var userId = $(this).data('user-id');
        $.ajax({
            type: 'POST',
            url: '/Friend/SendRequest',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    var newButton = '<a href="#" data-user-id="' + userId + '" class="btn btn-primary deny-friend-request-btn">Отменить заявку в друзья</a>';
                    $btn.replaceWith(newButton);
                }
            },
            error: function () {
                console.error('Ошибка при добавлении нового друга')
            }
        });
    });

    $(document).on('click', '.deny-friend-request-btn', function () {
        var $btn = $(this);
        var userId = $(this).data("user-id");
        $.ajax({
            type: 'POST',
            url: '/Friend/DenyRequest',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    var newButton = '<a href="#" data-user-id="' + userId + '" class="btn btn-primary add-friend-btn">Добавить в друзья</a>';
                    $btn.replaceWith(newButton);
                }
            },
            error: function () {
                console.error('Ошибка при отклонении запроса в друзья')
            }
        });
    });

    $(document).on('click', '.accept-friend-request-btn', function () {
        var $btn = $(this);
        var userId = $(this).data("user-id");
        $.ajax({
            type: 'POST',
            url: '/Friend/AcceptRequest',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
            },
            error: function () {
                console.error('Ошибка при принятии запроса в друзья')
            }
        });
    });

});