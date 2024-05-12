$(document).ready(function () {
    let isEditMode = false;
    let currentCommentId = 0;
    let originalText = '';
    $(document).on('click', '.edit-btn', function () {
        const $editButton = $(this);
        const $card = $editButton.closest('.card');
        const commentId = $card.data('id');
        const $textElement = $card.find('.comment-text');

        if (!isEditMode) {
            enterEditMode($textElement, $card, commentId);
        } else if (currentCommentId === commentId) {
            exitEditMode($textElement, $card, commentId);
        } else {
            $('#editWarningModal').modal('show');
        }
    });

    $(document).on('click', '.save-btn', function () {
        const $card = $(this).closest('.card');
        const commentId = $card.data('id');
        const $text = $card.find('.comment-text').val();
        editComment(commentId, $card, $text);
    });

    $(document).on('click', '[data-dismiss="modal"]', function () {
        $('#editWarningModal').modal('hide');
    });

    function enterEditMode($textElement, $card, commentId) {
        const $textarea = $('<textarea class="card-text comment-text form-control" rows="4" style="resize: none;"></textarea>')
            .val($textElement.text());
        $textElement.replaceWith($textarea);

        const $editPanel = $('<div class="d-flex justify-content-between align-items-center edit-panel">'
            + '<button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
            + '</div>');
        $textarea.after($editPanel);

        isEditMode = true;
        currentCommentId = commentId;
        originalText = $textElement.text();
    }

    function exitEditMode($textElement, $card, commentId, time) {
        const $textParagraph = $('<p class="card-text comment-text"></p>').text(originalText);
        $textElement.replaceWith($textParagraph);

        const $editPanel = $card.find('.edit-panel');
        $editPanel.remove();

        if (time != undefined) {
            $card.find('.time-change').text('Изменено: ' + time);
        }

        isEditMode = false;
        currentCommentId = 0;
        originalText = '';
    }

    function editComment(commentId, card, text) {

        if (text.trim().length > 0) {
            $.ajax({
                type: 'POST',
                url: '/Comment/EditComment',
                data: { commentId: commentId, text: text},
                success: function (response) {
                    const $textElement = card.find('.comment-text');
                    originalText = text;
                    exitEditMode($textElement, card, commentId, response.time);
                },
                error: function () {
                    console.error('Ошибка при редактировании комментария')
                }
            });
        }

    }
});