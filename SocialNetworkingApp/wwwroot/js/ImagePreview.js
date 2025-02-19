$(document).ready(function () {
    $(document).on('change', '[id^="input-image-"]', function (e) {
        var postId = this.id.split('-').pop(); 
        var file = e.target.files[0];
        var $imagePreview = $('#imagePreview-' + postId);
        var $clearImage = $('#clear-image-' + postId);
        $('#take-image-' + postId).val('');

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $imagePreview.empty();

                $('<img>').attr({
                    'src': e.target.result,
                    'class': 'img-fluid'
                }).appendTo($imagePreview);

                $clearImage.css('display', 'inline-block');
            };
            reader.readAsDataURL(file); 
        } else {
            $clearImage.css('display', 'none');
        }
    });

    $(document).on('click', '[id^="clear-image-"]', function (e) {
        var postId = this.id.split('-').pop(); 
        $('#imagePreview-' + postId).empty();
        $('#input-image-' + postId).val('');

        $(this).css('display', 'none');
    });


    $(document).on('change', '[id^="take-image-"]', function () {
        var postId = this.id.split('-').pop(); 
        var $imagePreview = $('#imagePreview-' + postId);
        var $clearImage = $('#clear-image-' + postId);
        var imagePath = $('#take-image-' + postId).val();
        $('#input-image-' + postId).val('');
        if (imagePath) {
            $imagePreview.empty();

            $('<img>').attr({
                'src': window.location.origin + "/" + imagePath,
                'class': 'img-fluid'
            }).appendTo($imagePreview);

            $clearImage.css('display', 'inline-block');
        } else {
            $clearImage.css('display', 'none');
        }
    });

    $('#chooseProfilePicModal').on('hidden.bs.modal', function (event) {
        if ($(event.target).is('#chooseProfilePicModal')) {
            $('#imagePreview-profilepic').empty();
            $('#input-image-profilepic').val('');
            $('#take-image-profilepic').val('');
        }
    });

});