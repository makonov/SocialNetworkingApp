document.getElementById('inputGroupFile').addEventListener('change', function (e) {
    var file = e.target.files[0];

    if (file) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var imagePreview = document.getElementById('imagePreview');
            imagePreview.innerHTML = '';
            var imgElement = document.createElement('img');
            imgElement.src = e.target.result;
            imgElement.className = 'img-fluid';
            imagePreview.appendChild(imgElement);
        };
        reader.readAsDataURL(file);

        // Показываем кнопку "Сбросить"
        document.getElementById('clearImage').style.display = 'inline-block';
    } else {
        // Скрываем кнопку "Сбросить", если файл сброшен
        document.getElementById('clearImage').style.display = 'none';
    }
});

document.getElementById('clearImage').addEventListener('click', function () {
    var imagePreview = document.getElementById('imagePreview');
    imagePreview.innerHTML = '';

    var inputGroupFile = document.getElementById('inputGroupFile');
    inputGroupFile.value = ''; // Очищаем значение в input файле

    // Скрываем кнопку "Сбросить", так как файл был сброшен
    document.getElementById('clearImage').style.display = 'none';
});