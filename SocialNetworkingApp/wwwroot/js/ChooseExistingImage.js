$(document).ready(function () {
    $(document).on('shown.bs.modal', '[id^="modalToggle-"]', function () {
        var entityId = this.id.split('-').pop();
        var modalType = this.id.split('-')[1]; 
        $.ajax({
            url: '/Image/LoadImages',
            type: 'GET',
            success: function (response) {
                var images = response.data;
                var container = $('#imageContainer-' + entityId);
                container.empty();
                images.forEach(function (image) {
                    var html = '<div class="col-md-4 mb-4">' +
                        '<div class="image-item">' +
                        '<label class="btn btn-light btn-lg image-button" data-image="' + image + '">' +
                        '<img src="\\' + image + '" class="img-square img-thumbnail" alt="Изображение">' +
                        '</label> ' +
                        '</div>' +
                        '</div>';
                    container.append(html);
                });
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        });
    });

    $(document).on('click', '.image-button', function () {
        var imagePath = $(this).data('image');
        const $modal = $(this).closest('.modal');
        const modalId = $modal.attr('id');
        const entityId = modalId.split('-').pop();
        $('#take-image-' + entityId).val(imagePath).trigger('change');
        $modal.modal('hide');
    });
});


                  
    
