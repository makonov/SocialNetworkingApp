$(document).ready(function () {
    var dialogCard = $('#dialog');
    var flagElement = $("#flag");
    dialogCard.scrollTop(dialogCard[0].scrollHeight);

    var interlocutorName = dialogCard.data("interlocutor-name");
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("ReceiveMessage", function (fromUserId, message, sentAt) {
        var userName = fromUserId === $("#fromUserId").val() ? "Вы" : interlocutorName;

        var messageDiv = $("<div>").addClass("message mb-2");
        var strongElement = $("<strong>").text(userName + ":");
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


    connection.start().then(function () {
        console.log("SignalR connection started");
    }).catch(function (err) {
        console.error("Error starting SignalR connection:", err.toString());
    });

    $("#messageForm").submit(function (event) {
        event.preventDefault();
        var fromUserId = $("#fromUserId").val();
        var toUserId = $("#toUserId").val();
        var message = $("#messageInput").val();
        connection.invoke("SendMessage", fromUserId, toUserId, message)
            .catch(function (err) {
                console.error("Error sending message:", err.toString());
            });
        $("#messageInput").val("").focus();
    });
});
