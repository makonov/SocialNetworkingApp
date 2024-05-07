$(document).ready(function () {
    let isEditMode = false;
    let currentPostId = 0;
    let originalText = '';
    let $originalImage;
    let $imagePath = '';

    $(document).on('click', '.edit-btn', function (event) {
        const $editButton = $(this);
        const $card = $editButton.closest('.card');
        const postId = $card.data('id');
        const $textElement = $card.find('.post-text');

        if (!isEditMode) {
            enterEditMode($textElement, $card, postId);
        } else if (currentPostId === postId) {
            exitEditMode($textElement, $card, postId);
        } else {
            $('#editWarningModal').modal('show');
        }
    });

    $(document).on('click', '.save-btn', function () {
        const $card = $(this).closest('.card');
        const postId = $card.data('id');
        const $text = $card.find('.post-text').val();
        var inputFile = $card.find('input[type=file]');
        var file = inputFile[0].files[0];
        /*var gifPathValue = $('#take-image-' + postId).val();*/
        var gifPathValue = $('#imagePreview-' + postId + ' img').attr('src');
        editPost(postId, $card, $text, file, gifPathValue);
    });

    $(document).on('click', '[data-dismiss="modal"]', function () {
        $('#editWarningModal').modal('hide');
    });

    function enterEditMode($textElement, $card, postId) {
        const $textarea = $('<textarea class="card-text post-text form-control" rows="4" style="resize: none;"></textarea>')
            .val($textElement.text());
        $textElement.replaceWith($textarea);

        const $editPanel = $('<div class="d-flex justify-content-between align-items-center edit-panel">'
            + '<button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
            + '<a class="btn btn-light btn-sm" data-toggle="collapse" href="#collapse-' + postId + '" role="button" aria-expanded="false" aria-controls="collapse-' + postId + '">'
            + '<i class="fas fa-camera" ></i>'
            + '</a>'
            + '<div class="collapse" id="collapse-' + postId + '">'
            + '<label for="input-image-' + postId + '" class="btn btn-light btn-sm" style="margin-top:5px; ">'
            + 'Загрузить фото с устройства'
            + '<input asp-for="Gif" type="file" hidden id="input-image-' + postId + '" accept=".gif" style="display: none">'
            + '</label>'
            + '<label for="take-image-' + postId + '" class="btn btn-light btn-sm" style="margin-top:5px;" data-bs-toggle="modal" data-bs-target="#modalToggle-post-' + postId + '">'
            + '<input asp-for="GifPath" hidden id="take-image-' + postId + '" accept=".gif" style="display: none">'
            + 'Выбрать фото из альбома'
            + '</label>'
            + '</div>'
            + '</div>');
        $textarea.after($editPanel);

        $originalImage = $card.find('img');
        if ($originalImage.length > 0) {
            $imagePath = $originalImage.attr('src');
            const $clearImage = $(`#clear-image-${postId}`);
            $clearImage.css('display', 'inline-block');
        } else {
            const $imagePreview = $('<div class="image-preview-' + postId + '">'
                + '<span id="imagePreview-' + postId + '"></span>'
                + '<button type="button" class="btn btn-light btn-sm" id="clear-image-' + postId + '" style="display:none;"><i class="bi bi-x"></i></button>'
                + '</div>');
            $textarea.before($imagePreview);
        }

        isEditMode = true;
        currentPostId = postId;
        originalText = $textElement.text();
    }

    function exitEditMode($textElement, $card, postId, time) {
        const $textParagraph = $('<p class="card-text post-text"></p>').text(originalText);
        $textElement.replaceWith($textParagraph);

        const $editPanel = $card.find('.edit-panel');
        $editPanel.remove();

        if ($imagePath) {
            const $imagePreview = $card.find('[id^="imagePreview-"]');
            const $imgInside = $imagePreview.find('img');
            if ($imgInside.length > 0) {
                $imgInside.remove();
            }
            $('<img>').attr({
                'src': $imagePath,
                'class': 'img-fluid'
            }).appendTo($imagePreview);
            const $clearImage = $(`#clear-image-${postId}`);
            $clearImage.css('display', 'none');
        } else {
            const $imagePreview = $card.find('[class*="image-preview"]');
            $imagePreview.remove();
        }

        if (time != undefined) {
            $card.find('.time-change').text('Изменено: ' + time);
        }
        
        isEditMode = false;
        currentPostId = 0;
        originalText = '';
        $imagePath = '';
    }

    function editPost(postId, card, text, file, gifPathValue) {
        var formData = new FormData();
        formData.append('text', text);
        formData.append('postId', postId);
        if (file) {
            formData.append('inputFile', file);
        }

        if (gifPathValue) {
            formData.append('existingGif', gifPathValue);
        }

        if (text.trim().length > 0 || file != null || gifPathValue != null) {
            $.ajax({
                type: 'POST',
                url: '/Feed/EditPost',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    const $textElement = card.find('.post-text');
                    originalText = text;
                    $imagePath = response.imagePath;
                    exitEditMode($textElement, card, postId, response.time);
                },
                error: function () {
                    console.error('Ошибка при редактировании поста')
                }
            });
        }
        
    }
});