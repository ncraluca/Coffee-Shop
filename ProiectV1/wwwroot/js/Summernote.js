$(document).ready(function () {
    $('.summernote').summernote({
        height: 300,
        minHeight: 200,
        focus: true,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['insert', ['link']]
        ]
    });
});
