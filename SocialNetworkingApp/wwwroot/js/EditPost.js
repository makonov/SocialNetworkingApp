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
        var imagePathValue = $('#imagePreview-' + postId + ' img').attr('src');
        editPost(postId, $card, $text, file, imagePathValue);
    });

    $(document).on('click', '[data-dismiss="modal"]', function () {
        $('#editWarningModal').modal('hide');
    });

    function enterEditMode($textElement, $card, postId) {
        const formattedText = $textElement.html().replace(/<br\s*\/?>/g, '\n').replace(/\n\n/g, '\n'); 
        const $textarea = $('<textarea class="card-text post-text form-control" rows="4" style="resize: none;"></textarea>')
            .val(formattedText);

        $textElement.replaceWith($textarea);

        const $editPanel = $('<div class="edit-panel d-flex flex-column justify-content-between align-items-start" style="width: 100%;">'
            + '<div class="d-flex justify-content-between align-items-center w-100">'
            + '    <button class="btn-sm btn-primary save-btn" style="margin-top: 5px;">Сохранить</button>'
            + '    <div class="d-flex align-items-center ml-auto">'
            + '        <a class="btn btn-light btn-sm" data-toggle="collapse" href="#collapse-' + postId + '" role="button" aria-expanded="false" aria-controls="collapse-' + postId + '">'
            + '            <i class="fas fa-camera"></i>'
            + '        </a>'
            + '        <button type="button" class="emoji-btn btn btn-light btn-sm" style="margin-left: 10px;">😊</button>'
            + '    </div>'
            + '</div>'
            + '<div class="collapse" id="collapse-' + postId + '" style="padding-top: 10px; width: 100%;">'
            + '    <label for="input-image-' + postId + '" class="btn btn-light btn-sm" style="margin-top:5px;">'
            + '        Загрузить фото с устройства'
            + '        <input type="file" hidden id="input-image-' + postId + '" accept=".jpeg,.jpg,.png,.gif">'
            + '    </label>'
            + '    <label for="take-image-' + postId + '" class="btn btn-light btn-sm" style="margin-top:5px;" data-bs-toggle="modal" data-bs-target="#modalToggle-post-' + postId + '">'
            + '        Выбрать фото из альбома'
            + '        <input asp-for="ImagePath" hidden id="take-image-' + postId + '" accept=".jpeg,.jpg,.png,.gif"" style="display: none">'
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
        originalText = formattedText; 
    }

    function exitEditMode($textElement, $card, postId, time) {
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
                'class': 'img-fluid imagepreview-' + postId
            }).appendTo($imagePreview);
            const $clearImage = $(`#clear-image-${postId}`);
            $clearImage.css('display', 'none');
        } else {
            console.log($card.find('img'));
            console.log($('img').length);
            const $imagePreview = $card.find('[class*="image-preview-"]');
            $imagePreview.remove();
            console.log($card.find('img'));
            console.log($('img').length);
        }

        if (time != undefined) {
            $card.find('.time-change').text('Изменено: ' + time);
        }

        isEditMode = false;
        currentPostId = 0;
        originalText = '';
        $imagePath = '';
    }

    $(document).on('click', '.delete-btn', function (event) {
        event.preventDefault(); 
        let card = $(this).closest('.card');
        let cardId = card.data('id');

        if (String(cardId) === String(currentPostId)) {
            isEditMode = false;
        }
    });



    function editPost(postId, card, text, file, imagePathValue) {
        var formData = new FormData();
        formData.append('text', text);
        formData.append('postId', postId);
        if (file) {
            formData.append('inputFile', file);
        }

        if (imagePathValue) {
            formData.append('existingImage', imagePathValue);
        }

        if (text.trim().length > 0 || file != null || imagePathValue != null) {
            $.ajax({
                type: 'POST',
                url: '/Post/EditPost',
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


