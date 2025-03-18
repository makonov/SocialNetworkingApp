$(document).ready(function () {
    const emojis = ['😊', '😂', '😍', '😎', '😢', '🥺', '🤔', '😡', '😱', '😴', '😜', '🙃', '😅', '🤯', '😇', '😭', '🤩', '🥳', '😤', '🤬', '🙄', '😶', '🤗', '🫡', '💡', '📌', '🤖', '💻', '🛠', '🔹', '✅','❌', '🔍', '⚙️', '💾', '🧠', '📖', '🔑', '📊', '📈', '📉', '🖥️', '⌨️', '🖱️', '🔗', '🛜', '🎮', '📡', '🔄', '⏳', '💽', '📂', '🗂️', '📝', '📚', '📑', '💿', '🏆', '🔥', '⚡', '🚀', '🎯', '🏅', '🎉', '🎓', '🥇', '🥈', '🥉', '🎖️', '📢', '💬', '🧐', '👀', '👨‍💻', '👩‍💻', '🔬', '🎥', '🌟', '💪', '🔝', '📅', '📍', '⌛', '⏰', '🗓️', '🕒', '📆', '⏱️', '🕹️', '🛑', '🚦', '🔜', '⚠️', '🐛', '🚨', '🚧', '🔨', '🧩', '🔎', '🔬', '🔭', '📄', '🖊️'];

    $(document).on('click', '.emoji-btn', function () {
        const card = $(this).closest('.card');
        const emojiContainer = card.find('.emoji-picker');

        if (emojiContainer.is(':hidden')) {
            emojiContainer.empty();

            emojis.forEach(emoji => {
                const emojiButton = $('<button>')
                    .addClass('btn btn-light btn-sm')
                    .text(emoji);
                emojiButton.on('click', function () {
                    let textInput = card.find('textarea');
                    if (!textInput.length) {
                        textInput = $('#messageInput');
                    }

                    const cursorPos = textInput.prop('selectionStart');
                    const text = textInput.val();
                    const newText = text.slice(0, cursorPos) + emoji + text.slice(cursorPos);

                    textInput.val(newText);

                    textInput.prop('selectionStart', cursorPos + emoji.length);
                    textInput.prop('selectionEnd', cursorPos + emoji.length);
                    textInput.focus();
                });

                emojiContainer.append(emojiButton);
            });
        }
        emojiContainer.toggle();
    });
});


