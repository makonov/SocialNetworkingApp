$(document).ready(function () {
    // Обработчик изменения выбранного файла для элементов с идентификаторами, начинающимися с "input-image-"
    $(document).on('change', '[id^="input-image-"]', function (e) {
        var postId = this.id.split('-').pop(); // Получаем номер поста из идентификатора элемента
        var file = e.target.files[0];
        var $imagePreview = $('#imagePreview-' + postId);
        var $clearImage = $('#clear-image-' + postId);

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $imagePreview.empty(); // Очищаем содержимое предварительного просмотра

                // Создаем новый элемент <img> с атрибутами
                $('<img>').attr({
                    'src': e.target.result,
                    'class': 'img-fluid'
                }).appendTo($imagePreview);

                // Показываем кнопку "Сбросить"
                $clearImage.css('display', 'inline-block');
            };
            reader.readAsDataURL(file); // Читаем файл как URL-адрес данных
        } else {
            // Скрываем кнопку "Сбросить", если файл сброшен
            $clearImage.css('display', 'none');
        }
    });

    // Обработчик нажатия кнопки "Сбросить" для элементов с идентификаторами, начинающимися с "clear-image-"
    $(document).on('click', '[id^="clear-image-"]', function (e) {
        var postId = this.id.split('-').pop(); // Получаем номер поста из идентификатора элемента
        $('#imagePreview-' + postId).empty(); // Очищаем содержимое предварительного просмотра
        $('#input-image-' + postId).val(''); // Очищаем значение в input файле

        // Скрываем кнопку "Сбросить"
        $(this).css('display', 'none');
    });
});