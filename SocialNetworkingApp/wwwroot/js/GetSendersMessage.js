$(document).ready(function () {
    var dialogCard = $('#dialog');
    var flagElement = $("#flag");
    dialogCard.scrollTop(dialogCard[0].scrollHeight);

    var messageContainer = $("#messageContainer");
    var interlocutorName = messageContainer.data("interlocutor-name");
    var fromUserId = $("#fromUserId").val();
    var toUserId = $("#toUserId").val();
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    // Подключение к SignalR
    connection.start().then(function () {
        console.log("SignalR connection started");
        joinChat();
    }).catch(function (err) {
        console.error("Error starting SignalR connection:", err.toString());
    });

    function markMessagesAsRead() {
        var fromUserId = $("#toUserId").val();
        var toUserId = $("#fromUserId").val();

        connection.invoke("MarkMessagesAsRead", fromUserId, toUserId)
            .then(function (isMarked) {
                if (isMarked) {
                    console.log("Messages were marked as read.");
                    $(".badge-message").remove();
                } else {
                    console.log("No messages were marked as read.");
                }
            })
            .catch(function (err) {
                console.error("Error marking messages as read:", err.toString());
            });
    }


    function joinChat() {
        connection.invoke("JoinChat", fromUserId, toUserId)
            .then(function () {
                markMessagesAsRead(); 
            })
            .catch(function (err) {
                console.error("Error joining chat:", err.toString());
            });
    }


    // Сообщаем серверу, что пользователь закрыл диалог (например, при выходе со страницы)
    function leaveChat() {
        connection.invoke("LeaveChat", fromUserId)
            .catch(function (err) {
                console.error("Error leaving chat:", err.toString());
            });
    }

    //// Обработчик получения сообщений
    //connection.on("ReceiveMessage", function (fromUserId, message, sentAt) {
    //    var userName = fromUserId === $("#fromUserId").val() ? "Вы" : interlocutorName;

    //    var messageDiv = $("<div>").addClass("message mb-2");
    //    var strongElement = $("<strong>").text(userName + ":");
    //    var messageParagraph = $("<p>").text(message);

    //    var sentAtDate = new Date(Date.parse(sentAt));
    //    var formattedDate = sentAtDate.toLocaleDateString('ru-RU');
    //    var formattedTime = sentAtDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    //    var formattedDateTime = formattedDate + " " + formattedTime;

    //    var smallElement = $("<small>").addClass("text-muted").text(formattedDateTime);

    //    messageDiv.append(strongElement).append(messageParagraph).append(smallElement);
    //    $("#dialog .card-body").append(messageDiv);
    //    dialogCard.scrollTop(dialogCard[0].scrollHeight);

    //    if (flagElement.length) {
    //        flagElement.remove();
    //    }
    //}); 
    // Обработчик получения сообщений
    connection.on("ReceiveMessage", function (fromUserId, message, sentAt) {
        var userName = fromUserId === $("#fromUserId").val() ? "Вы" : interlocutorName;

        var messageDiv = $("<div>").addClass("message mb-2");

        // Формируем ссылку на профиль
        var profileLink = $("<a>")
            .addClass("profile-link")
            .attr("href", "/Profile/Index?userId=" + fromUserId)
            .text(userName + ":");

        var strongElement = $("<strong>").append(profileLink);
        var messageParagraph = $("<p>").text(message);

        var sentAtDate = new Date(Date.parse(sentAt));
        var formattedDate = sentAtDate.toLocaleDateString('ru-RU');
        var formattedTime = sentAtDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        var formattedDateTime = formattedDate + " " + formattedTime;

        var smallElement = $("<small>").addClass("text-muted").text(formattedDateTime);

        messageDiv.append(strongElement).append(messageParagraph).append(smallElement);
        $("#dialog .card-body").append(messageDiv);
        dialogCard.scrollTop(dialogCard[0].scrollHeight);

        if (flagElement.length) {
            flagElement.remove();
        }
    });


    // Отправка сообщения
    $("#messageForm").submit(function (event) {
        event.preventDefault();
        var message = $("#messageInput").val();
        connection.invoke("SendMessage", fromUserId, toUserId, message)
            .catch(function (err) {
                console.error("Error sending message:", err.toString());
            });
        $("#messageInput").val("").focus();
    });

    // Вызываем leaveChat при закрытии страницы или переключении диалога
    $(window).on("beforeunload", function () {
        leaveChat();
    });
});
