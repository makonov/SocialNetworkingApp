$(document).ready(function () {
    var page = 1;
    var lastMessageId = 0;
    var isLoading = false;

    $('#dialog').scroll(function () {
        if ($('#dialog').scrollTop() <= 200) {
            loadMore();
        }
    });

    function loadMore() {
        if (!isLoading) {
            isLoading = true;
            lastMessageId = $("#messageContainer .card").last().data("id");
            var messageContainer = $('#messageContainer');
            var interlocutorId = messageContainer.data("interlocutor-id");
            var interlocutorName = messageContainer.data("interlocutor-name");
            page++;
            $.ajax({
                url: '/Messenger/GetMessages',
                data: { interlocutorId: interlocutorId, interlocutorName: interlocutorName, page: page, lastMessageId: lastMessageId },
                type: 'GET',
                success: function (data) {
                    if (data.length > 0) {
                        $('#messageContainer').prepend(data);
                    }
                    isLoading = false;
                },
                error: function () {
                    isLoading = false;
                }
            });
        }
    }
});