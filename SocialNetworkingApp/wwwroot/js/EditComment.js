//$(document).ready(function () {
//    let isEditMode = false;
//    let currentCommentId = 0;
//    let originalText = '';
//    $(document).on('click', '.edit-btn', function () {
//        const $editButton = $(this);
//        const $card = $editButton.closest('.card');
//        const commentId = $card.data('id');
//        const $textElement = $card.find('.comment-text');

//        if (!isEditMode) {
//            enterEditMode($textElement, $card, commentId);
//        } else if (currentCommentId === commentId) {
//            exitEditMode($textElement, $card, commentId);
//        } else {
//            $('#editWarningModal').modal('show');
//        }
//    });

//    $(document).on('click', '.save-btn', function () {
//        const $card = $(this).closest('.card');
//        const commentId = $card.data('id');
//        const $text = $card.find('.comment-text').val();
//        editComment(commentId, $card, $text);
//    });

//    $(document).on('click', '[data-dismiss="modal"]', function () {
//        $('#editWarningModal').modal('hide');
//    });

//    function enterEditMode($textElement, $card, commentId) {
//        const $textarea = $('<textarea class="card-text comment-text form-control" rows="4" style="resize: none;"></textarea>')
//            .val($textElement.text());
//        $textElement.replaceWith($textarea);

//        const $editPanel = $('<div class="d-flex justify-content-between align-items-center edit-panel">'
//            + '<button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
//            + '</div>');
//        $textarea.after($editPanel);

//        isEditMode = true;
//        currentCommentId = commentId;
//        originalText = $textElement.text();
//    }

//    function exitEditMode($textElement, $card, commentId, time) {
//        const $textParagraph = $('<p class="card-text comment-text"></p>').text(originalText);
//        $textElement.replaceWith($textParagraph);

//        const $editPanel = $card.find('.edit-panel');
//        $editPanel.remove();

//        if (time != undefined) {
//            $card.find('.time-change').text('Изменено: ' + time);
//        }

//        isEditMode = false;
//        currentCommentId = 0;
//        originalText = '';
//    }

//    function editComment(commentId, card, text) {

//        if (text.trim().length > 0) {
//            $.ajax({
//                type: 'POST',
//                url: '/Comment/EditComment',
//                data: { commentId: commentId, text: text},
//                success: function (response) {
//                    const $textElement = card.find('.comment-text');
//                    originalText = text;
//                    exitEditMode($textElement, card, commentId, response.time);
//                },
//                error: function () {
//                    console.error('Ошибка при редактировании комментария')
//                }
//            });
//        }

//    }
//});

//$(document).ready(function () {
//    let isEditMode = false;
//    let currentCommentId = 0;
//    let originalText = '';
//    let $originalImage;
//    let $imagePath = '';

//    $(document).on('click', '.edit-btn', function (event) {
//        const $editButton = $(this);
//        const $card = $editButton.closest('.card');
//        const commentId = $card.data('id');
//        const $textElement = $card.find('.comment-text');

//        if (!isEditMode) {
//            enterEditMode($textElement, $card, commentId);
//        } else if (currentCommentId === commentId) {
//            exitEditMode($textElement, $card, commentId);
//        } else {
//            $('#editWarningModal').modal('show');
//        }
//    });

//    $(document).on('click', '.save-btn', function () {
//        const $card = $(this).closest('.card');
//        const commentId = $card.data('id');
//        const $text = $card.find('.comment-text').val();
//        var inputFile = $card.find('input[type=file]');
//        var file = inputFile[0].files[0];
//        var imagePathValue = $('#imagePreview-' + commentId + ' img').attr('src');
//        editComment(commentId, $card, $text, file, imagePathValue);
//    });

//    $(document).on('click', '[data-dismiss="modal"]', function () {
//        $('#editWarningModal').modal('hide');
//    });

//    function enterEditMode($textElement, $card, commentId) {
//        const $textarea = $('<textarea class="card-text comment-text form-control" rows="4" style="resize: none;"></textarea>')
//            .val($textElement.text());
//        $textElement.replaceWith($textarea);

//        const $editPanel = $('<div class="d-flex justify-content-between align-items-center edit-panel">'
//            + '<button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
//            + '<a class="btn btn-light btn-sm" data-toggle="collapse" href="#collapse-' + commentId + '" role="button" aria-expanded="false" aria-controls="collapse-' + commentId + '">'
//            + '<i class="fas fa-camera" ></i>'
//            + '</a>'
//            + '<div class="collapse" id="collapse-' + commentId + '">'
//            + '<label for="input-image-' + commentId + '" class="btn btn-light btn-sm" style="margin-top:5px; ">'
//            + 'Загрузить фото с устройства'
//            + '<input asp-for="Image" type="file" hidden id="input-image-' + commentId + '" accept=".jpeg,.jpg,.png,.gif" style="display: none">'
//            + '</label>'
//            + '<label for="take-image-' + commentId + '" class="btn btn-light btn-sm" style="margin-top:5px;" data-bs-toggle="modal" data-bs-target="#modalToggle-comment-' + commentId + '">'
//            + '<input asp-for="ImagePath" hidden id="take-image-' + commentId + '" accept=".jpeg,.jpg,.png,.gif"" style="display: none">'
//            + 'Выбрать фото из альбома'
//            + '</label>'
//            + '</div>'
//            + '</div>');
//        $textarea.after($editPanel);

//        $originalImage = $card.find('img');
//        if ($originalImage.length > 0) {
//            $imagePath = $originalImage.attr('src');
//            const $clearImage = $(`#clear-image-${commentId}`);
//            $clearImage.css('display', 'inline-block');
//        } else {
//            const $imagePreview = $('<div class="image-preview-' + commentId + '">'
//                + '<span id="imagePreview-' + commentId + '"></span>'
//                + '<button type="button" class="btn btn-light btn-sm" id="clear-image-' + commentId + '" style="display:none;"><i class="bi bi-x"></i></button>'
//                + '</div>');
//            $textarea.before($imagePreview);
//        }

//        isEditMode = true;
//        currentCommentId = commentId;
//        originalText = $textElement.text();
//    }

//    function exitEditMode($textElement, $card, commentId, time) {
//        const formattedText = originalText.replace(/\n/g, '<br/>');
//        const $textParagraph = $('<p class="card-text post-text"></p>').html(formattedText);
//        $textElement.replaceWith($textParagraph);

//        const $textParagraph = $('<p class="card-text comment-text"></p>').text(originalText);
//        $textElement.replaceWith($textParagraph);

//        const $editPanel = $card.find('.edit-panel');
//        $editPanel.remove();

//        if ($imagePath) {
//            const $imagePreview = $card.find('[id^="imagePreview-"]');
//            const $imgInside = $imagePreview.find('img');
//            if ($imgInside.length > 0) {
//                $imgInside.remove();
//            }
//            $('<img>').attr({
//                'src': window.location.origin + "/" + $imagePath,
//                'class': 'img-fluid'
//            }).appendTo($imagePreview);
//            const $clearImage = $(`#clear-image-${commentId}`);
//            $clearImage.css('display', 'none');
//        } else {
//            const $imagePreview = $card.find('[class*="image-preview"]');
//            $imagePreview.remove();
//        }

//        if (time != undefined) {
//            $card.find('.time-change').text('Изменено: ' + time);
//        }

//        isEditMode = false;
//        currentCommentId = 0;
//        originalText = '';
//        $imagePath = '';
//    }

//    function editComment(commentId, card, text, file, imagePathValue) {
//        var formData = new FormData();
//        formData.append('text', text);
//        formData.append('commentId', commentId);
//        if (file) {
//            formData.append('inputFile', file);
//        }

//        if (imagePathValue) {
//            formData.append('existingImage', imagePathValue);
//        }

//        if (text.trim().length > 0 || file != null || imagePathValue != null) {
//            $.ajax({
//                type: 'POST',
//                url: '/Comment/EditComment',
//                data: formData,
//                contentType: false,
//                processData: false,
//                success: function (response) {
//                    const $textElement = card.find('.comment-text');
//                    originalText = text;
//                    $imagePath = response.imagePath;
//                    exitEditMode($textElement, card, commentId, response.time);
//                },
//                error: function () {
//                    console.error('Ошибка при редактировании комментария')
//                }
//            });
//        }

//    }
//});
$(document).ready(function () {
    let isEditMode = false;
    let currentCommentId = 0;
    let originalText = '';
    let $originalImage;
    let $imagePath = '';

    $(document).on('click', '.edit-btn', function (event) {
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
        var inputFile = $card.find('input[type=file]');
        var file = inputFile[0].files[0];
        var imagePathValue = $('#imagePreview-' + commentId + ' img').attr('src');
        editComment(commentId, $card, $text, file, imagePathValue);
    });

    $(document).on('click', '[data-dismiss="modal"]', function () {
        $('#editWarningModal').modal('hide');
    });

    function enterEditMode($textElement, $card, commentId) {
        const formattedText = $textElement.html().replace(/<br\s*\/?>/g, '\n').replace(/\n{3,}/g, '\n\n');
        const $textarea = $('<textarea class="card-text post-text form-control" rows="4" style="resize: none;"></textarea>')
            .val(formattedText);

        $textElement.replaceWith($textarea);

        const $editPanel = $('<div class="edit-panel d-flex flex-column justify-content-between align-items-start" style="width: 100%;">'
            + '<div class="d-flex justify-content-between align-items-center w-100">'
            + '    <button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
            + '    <div class="d-flex align-items-center ml-auto">'
            + '        <a class="btn btn-light btn-sm" data-toggle="collapse" href="#collapse-' + commentId + '" role="button" aria-expanded="false" aria-controls="collapse-' + commentId + '">'
            + '            <i class="fas fa-camera"></i>'
            + '        </a>'
            + '        <button type="button" class="emoji-btn btn btn-light btn-sm" style="margin-left: 10px;">😊</button>'
            + '    </div>'
            + '</div>'
            + '<div class="collapse" id="collapse-' + commentId + '" style="padding-top: 10px; width: 100%;">'
            + '    <label for="input-image-' + commentId + '" class="btn btn-light btn-sm" style="margin-top:5px;">'
            + '        Загрузить фото с устройства'
            + '        <input type="file" hidden id="input-image-' + commentId + '" accept=".jpeg,.jpg,.png,.gif">'
            + '    </label>'
            + '    <label for="take-image-' + commentId + '" class="btn btn-light btn-sm" style="margin-top:5px;" data-bs-toggle="modal" data-bs-target="#modalToggle-comment-' + commentId + '">'
            + '        Выбрать фото из альбома'
            + '        <input asp-for="ImagePath" hidden id="take-image-' + commentId + '" accept=".jpeg,.jpg,.png,.gif"" style="display: none">'
            + '    </label>'
            + '</div>'
            + '<div class="emoji-picker" style="display: none; position: relative; background: white; border: 1px solid #ccc; padding: 5px; width: 250px; height: 200px; overflow-y: auto; border-radius: 5px; box-shadow: 2px 2px 10px rgba(0,0,0,0.2); margin-top: 10px; width: 100%;">'
            + '    <!-- Смайлики будут добавляться здесь через JS -->'
            + '</div>'
            + '</div>');

        $textarea.after($editPanel);

        $originalImage = $card.find('.card-body img');
        if ($originalImage.length > 0) {
            $imagePath = $originalImage.attr('src');
            const $clearImage = $(`#clear-image-${commentId}`);
            $clearImage.css('display', 'inline-block');
        } else {
            const $imagePreview = $('<div class="image-preview-' + commentId + '">'
                + '<span id="imagePreview-' + commentId + '"></span>'
                + '<button type="button" class="btn btn-light btn-sm" id="clear-image-' + commentId + '" style="display:none;"><i class="bi bi-x"></i></button>'
                + '</div>');
            $textarea.before($imagePreview);
        }

        isEditMode = true;
        currentCommentId = commentId;
        originalText = $textElement.text();
    }

    function exitEditMode($textElement, $card, commentId) {
        const formattedText = originalText.replace(/\n/g, '<br/>').replace(/\n{3,}/g, '\n\n');
        const $textParagraph = $('<p class="card-text post-text"></p>').html(formattedText);
        $textElement.replaceWith($textParagraph);

        const $editPanel = $card.find('.edit-panel');
        $editPanel.remove();

        if ($imagePath) {
            const $imagePreview = $card.find('[id^="imagePreview-"]');
            const $imgInside = $imagePreview.find('img');
            if ($imgInside.length > 0) {
                $imgInside.remove();
            }
            const imageSrc = $imagePath.startsWith('data') || $imagePath.startsWith('/data')
                ? '/' + $imagePath.replace(/^.*?data\//, 'data/') 
                : window.location.origin + "/" + $imagePath.replace(/^.*?data\//, 'data/');

            $('<img>').attr({
                'src': imageSrc,
                'class': 'img-fluid'
            }).appendTo($imagePreview);
            const $clearImage = $(`#clear-image-${commentId}`);
            $clearImage.css('display', 'none');
        } else {
            const $imagePreview = $card.find('[class*="image-preview"]');
            $imagePreview.remove();
        }

        isEditMode = false;
        currentCommentId = 0;
        originalText = '';
        $imagePath = '';
    }

    function editComment(commentId, card, text, file, imagePathValue) {
        var formData = new FormData();
        formData.append('text', text);
        formData.append('commentId', commentId);
        if (file) {
            formData.append('inputFile', file);
        }

        if (imagePathValue) {
            formData.append('existingImage', imagePathValue);
        }

        if (text.trim().length > 0 || file != null || imagePathValue != null) {
            $.ajax({
                type: 'POST',
                url: '/Comment/EditComment',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    const $textElement = card.find('.comment-text');
                    originalText = text;
                    $imagePath = response.imagePath;
                    exitEditMode($textElement, card, commentId);
                },
                error: function () {
                    console.error('Ошибка при редактировании комментария')
                }
            });
        }
    }
});
