// Функция для отправки формы
function submitForm(formId, url, listId) {
    const $form = $('#' + formId);
    const formData = new FormData($form[0]);

    const description = $form.find('textarea[name="description"]').val().trim();
    if (!description) {
        alert('Описание не может быть пустым');
        return;
    }

    $.ajax({
        url: url,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            const $list = $('#' + listId);
            $list.append(response);
            if (formId == 'changeFormElement') {
                toggleChangeForm();
            }
            else {
                toggleAnnouncementForm();
            }
            $form[0].reset();
        },
        error: function (xhr, status, error) {
            console.error('Ошибка при отправке формы:', error);
        }
    });
}

// Функция для показа/скрытия формы добавления участника
function toggleAddAdminForm() {
    const form = document.getElementById('addAdminForm');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
}

// Функция для отправки формы добавления администратора
function submitAddAdminForm(url, listId) {
    const form = $('#addAdminFormElement');
    const formData = new FormData(form[0]);

    // Проверяем, что выбрали студента и ввели роль
    if (!formData.get('studentData')) {
        alert('Пожалуйста, выберите студента.');
        return;
    }

    // Отправляем данные формы через AJAX
    $.ajax({
        url: url,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            const adminList = $('#' + listId);
            adminList.append(response);
            form[0].reset();
        },
        error: function () {
            alert('Такого человека нет или он уже является администратором сообщества.');
        }
    });
}

// Функция для обновления списка участников
function updateAdminList(newAdminHtml, listId) {
    const adminList = $('#' + listId);
    adminList.append(newAdminHtml);
}

// Делегируем событие удаления
$('#adminList').on('click', '.deleteAdmin-btn', function () {
    const adminId = $(this).data('adminid');

    $.ajax({
        url: '/Community/DeleteAdmin',
        type: 'POST',
        data: { adminId: adminId },
        success: function () {
            $(`[data-adminId=${adminId}]`).remove();
        },
        error: function () {
            alert('Ошибка при удалении администратора.');
        }
    });
});