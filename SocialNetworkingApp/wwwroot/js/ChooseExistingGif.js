
$(document).ready(function () {
    $(document).on('shown.bs.modal', '[id^="modalToggle-post-"]', function () {
        var postId = this.id.split('-').pop();
        $.ajax({
            url: '/Gif/LoadGifs',
            type: 'GET',
            success: function (response) {

                var gifs = response.data;
                var container = $('#gifContainer-' + postId);
                container.empty();
                gifs.forEach(function (gif) {
                    var html = '<div class="col-md-4 mb-4">' +
                        '<div class="gif-item">' +
                        '<label class="btn btn-light btn-lg gif-button" data-gif="' + gif + '">' +
                        '<img src="\\' + gif + '" class="gif-img-square img-thumbnail" alt="GIF">' +
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

    $(document).on('click', '.gif-button', function () {
        var gifPath = $(this).data('gif');
        const $modal = $(this).closest('.modal');
        const postId = $modal.attr('id').split('-').pop();
        $('#take-image-' + postId).val(gifPath).trigger('change');
        $('#modalToggle-post-' + postId).modal('hide'); 
    });
});


                  
    
