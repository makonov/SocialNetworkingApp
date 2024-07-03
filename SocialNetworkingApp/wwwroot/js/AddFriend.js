$(document).ready(function () {
    $(document).on('click', '.add-friend-btn', function () {
        event.preventDefault();
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
        event.preventDefault();
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
        event.preventDefault();
        var $btn = $(this);
        var userId = $(this).data("user-id");
        $.ajax({
            type: 'POST',
            url: '/Friend/AcceptRequest',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    var newButton = '<a href="#" data-user-id="' + userId + '" class="btn btn-primary delete-friend-btn">Удалить из друзей</a>';
                    $btn.replaceWith(newButton);
                    /*location.reload();*/
                }
            },
            error: function () {
                console.error('Ошибка при принятии запроса в друзья')
            }
        });

    });

    $(document).on('click', '.delete-friend-btn', function () {
        event.preventDefault();
        var $btn = $(this);
        var userId = $(this).data("user-id");
        $.ajax({
            type: 'POST',
            url: '/Friend/DeleteFriend',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    $btn.closest('.card').remove();
                    
                }
            },
            error: function () {
                console.error('Ошибка при удалении друга')
            }
        });

    });

    $(document).on('click', '.accept-friendreq-from-reqlist-btn', function (event) {
        event.preventDefault();
        var $btn = $(this);
        var userId = $btn.data("user-id");
        $.ajax({
            type: 'POST',
            url: '/Friend/AcceptRequest',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    $btn.closest('.card').remove();

                    var newCard = `
                <div class="card mb-3" data-id="${response.friendId}">
                    <div class="card-body">
                        <h5 class="card-title">
                            <a class="profile-link" href="/Profile/Index?userId=${response.friendId}">
                                ${response.firstName} ${response.lastName}
                            </a>
                        </h5>
                        <img src="${response.profilePicture != null ? response.profilePicture : '/default-data/default-profile-pic.png'}" class="fixed-profile-img rounded-circle" alt="Фото профиля друга">
                        <div>
                            <a href="/Message/Index?postId=${response.friendId}">Написать сообщение</a>
                            <a class="btn btn-light btn-sm delete-friend-btn" style="float: right; margin-top: 5px;" data-user-id="${response.friendId}">
                                <i class="bi bi-trash-fill"></i>
                            </a>
                        </div>
                    </div>
                </div>`;

                    $('#friendsContainer').append(newCard);
                    location.reload();
                }
            },
            error: function () {
                console.error('Ошибка при принятии запроса в друзья');
            }
        });
    });


});