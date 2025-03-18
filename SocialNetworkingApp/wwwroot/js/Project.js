// Функция для показа/скрытия формы изменения
function toggleChangeForm() {
    const form = document.getElementById('changeForm');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
}

// Функция для показа/скрытия формы объявления
function toggleAnnouncementForm() {
    const form = document.getElementById('announcementForm');
    form.removeAttribute('hidden');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
}

// Отправка формы
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

// Удаление изменения
$(document).on('click', '.deleteChange-btn', function () {
    const changeId = $(this).data('changeid');
    $.ajax({
        url: '/Project/DeleteChange',
        method: 'POST',
        data: { changeId },
        success: function (response) {
            if (response.success) {
                $(`li[data-changeid="${changeId}"]`).remove();
            } else {
                alert(response.message || 'Ошибка при удалении изменения.');
            }
        },
        error: function () {
            alert('Ошибка при удалении изменения.');
        }
    });
});

// Открытие модального окна для редактирования
$(document).on('click', '.editChange-btn', function () {
    const changeId = $(this).data('changeid');
    const description = $(`li[data-changeid="${changeId}"] .description`).text().trim();

    $('#editChangeId').val(changeId);
    $('#editChangeDescription').val(description);
    $('#editChangeModal').modal('show');
});

// Сохранение изменений после редактирования
function submitEditForm() {
    const changeId = $('#editChangeId').val();
    const description = $('#editChangeDescription').val().trim();

    if (!description) {
        alert('Описание не может быть пустым.');
        return;
    }

    $.ajax({
        url: '/Project/EditChange',
        method: 'POST',
        data: { changeId, description },
        success: function (response) {
            if (response.success) {
                $(`li[data-changeid="${changeId}"] div`).find('.description').text(response.updatedDescription);
                $('#editChangeModal').modal('hide');
            } else {
                alert(response.message || 'Ошибка при редактировании изменения.');
            }
        },
        error: function () {
            alert('Ошибка при редактировании изменения.');
        }
    });
}

// Удаление объявления
$(document).on('click', '.deleteAnnouncement-btn', function () {
    const announcementId = $(this).data('announcementid');
    $.ajax({
        url: '/Project/DeleteAnnouncement',
        method: 'POST',
        data: { announcementId },
        success: function (response) {
            if (response.success) {
                $(`li[data-announcementid="${announcementId}"]`).remove();
            } else {
                alert(response.message || 'Ошибка при удалении объявления.');
            }
        },
        error: function () {
            alert('Ошибка при удалении объявления.');
        }
    });
});

// Открытие модального окна для редактирования объявления
$(document).on('click', '.editAnnouncement-btn', function () {
    const announcementId = $(this).data('announcementid');
    const description = $(`li[data-announcementid="${announcementId}"] .description`).contents().filter(function () {
        return this.nodeType === Node.TEXT_NODE;
    }).text().trim();

    $('#editAnnouncementId').val(announcementId);
    $('#editAnnouncementDescription').val(description);
    $('#editAnnouncementModal').modal('show');
});

// Сохранение изменений после редактирования объявления
function submitEditAnnouncementForm() {
    const announcementId = $('#editAnnouncementId').val();
    const description = $('#editAnnouncementDescription').val().trim();

    if (!description) {
        alert('Описание не может быть пустым.');
        return;
    }

    $.ajax({
        url: '/Project/EditAnnouncement',
        method: 'POST',
        data: { announcementId, description },
        success: function (response) {
            if (response.success) {
                $(`li[data-announcementid="${announcementId}"] div`).find('.description').text(response.updatedDescription);
                $('#editAnnouncementModal').modal('hide');
            } else {
                alert(response.message || 'Ошибка при редактировании объявления.');
            }
        },
        error: function () {
            alert('Ошибка при редактировании объявления.');
        }
    });
}

// Подписка и отписка
$(document).ready(function () {
    $('#subscriptionButton').click(function () {
        const projectId = $(this).data('projectid');
        const isSubscribed = $(this).hasClass('btn-unsubscribe');

        $.ajax({
            url: isSubscribed ? '/Project/Unsubscribe' : '/Project/Subscribe',
            method: 'POST',
            data: { projectId: projectId },
            success: function (response) {
                if (response.success) {
                    $('#subscriberCount').text(`${response.subscriberCount}`);
                    if (isSubscribed) {
                        $('#subscriptionButton').removeClass('btn-unsubscribe').addClass('btn-subscribe').text('Подписаться');
                    } else {
                        $('#subscriptionButton').removeClass('btn-subscribe').addClass('btn-unsubscribe').text('Отписаться');
                    }
                } else {
                    alert(response.message || 'Ошибка при подписке/отписке.');
                }
            },
            error: function () {
                alert('Произошла ошибка при выполнении запроса.');
            }
        });
    });
});

// Функция для показа/скрытия формы добавления участника
function toggleAddMemberForm() {
    const form = document.getElementById('addMemberForm');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
}

function submitAddMemberForm(url, listId) {
    const form = $('#addMemberFormElement');
    const formData = new FormData(form[0]);

    if (!formData.get('studentData') || !formData.get('role')) {
        alert('Пожалуйста, выберите студента и введите роль.');
        return;
    }

    $.ajax({
        url: url,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            const memberList = $('#' + listId);
            memberList.append(response);
            form[0].reset();
        },
        error: function () {
            alert('Такого человека нет или он уже является членом проекта.');
        }
    });
}

// Обновление списка участников
function updateMemberList(newMemberHtml, listId) {
    const memberList = $('#' + listId);
    memberList.append(newMemberHtml);
}

// Функция для обновления видимости полей Fundraising
$(document).ready(function () {
    function toggleFundraisingFields() {
        var selectedType = $('#ProjectType option:selected').text().trim();
        if (selectedType === 'Стартап') {
            $('#FundraisingFields').show();
        } else {
            $('#FundraisingFields').hide();
        }
    }

    toggleFundraisingFields();

    $('#ProjectType').change(function () {
        toggleFundraisingFields();
    });
});

// Отправка формы редактирования роли
function submitEditMemberForm() {
    const memberId = $('#editMemberId').val();
    const newRole = $('#editMemberRole').val();

    $.ajax({
        url: '/Project/EditMemberRole',
        type: 'POST',
        data: {
            memberId: memberId,
            role: newRole
        },
        success: function () {
            $(`[data-memberId=${memberId}] .badge`).text(newRole);
            $('#editMemberModal').modal('hide');
        },
        error: function () {
            alert('Ошибка при редактировании роли.');
        }
    });
}

// Редактирование и удаление участников
$('#memberList').on('click', '.editMember-btn', function () {
    const memberId = $(this).data('memberid');
    const currentRole = $(this).data('role');

    $('#editMemberId').val(memberId);
    $('#editMemberRole').val(currentRole);

    $('#editMemberModal').modal('show');
});

$('#memberList').on('click', '.deleteMember-btn', function () {
    const memberId = $(this).data('memberid');

    $.ajax({
        url: '/Project/DeleteMember',
        type: 'POST',
        data: { memberId: memberId },
        success: function () {
            $(`li[data-memberid=${memberId}]`).remove();
        },
        error: function () {
            alert('Ошибка при удалении участника.');
        }
    });
});
