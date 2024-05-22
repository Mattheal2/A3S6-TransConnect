function fetchClients() {
    $.ajax({
        url: '/api/Clients/GetClients?order_field=last_name&order_dir=ASC',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                renderClients(resp.data);
            }
        }
    });
}

const STATE = {
    user_id: null,
    op: null
};

const svgEditButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-edit"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M7 7h-1a2 2 0 0 0 -2 2v9a2 2 0 0 0 2 2h9a2 2 0 0 0 2 -2v-1" /><path d="M20.385 6.585a2.1 2.1 0 0 0 -2.97 -2.97l-8.415 8.385v3h3l8.385 -8.415z" /><path d="M16 5l3 3" /></svg>`;
const svgDeleteButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-trash-x"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 7h16" /><path d="M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2 -2l1 -12" /><path d="M9 7v-3a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v3" /><path d="M10 12l4 4m0 -4l-4 4" /></svg>`;

async function renderClients(data) {
    var table = $('#clientstable');


    table.empty();
    await data.forEach(function (client) {
        var row = $('<tr>');
        row.append($('<td>').text(client.last_name).addClass('sort-lastname'));
        row.append($('<td>').text(client.first_name).addClass('sort-firstname'));
        row.append($('<td>').text(client.position).addClass('sort-poste'));
        row.append($('<td>').text(client.city).addClass('sort-ville'));
        row.append($('<td>').text(client.email).addClass('sort-email'));
        row.append($('<td>').text(client.phone).addClass('sort-tel'));
        row.append($('<td>').text(formatPrice(client.total_spent)).addClass('sort-totalspent').attr('data-totalspent', client.total_spent));
        row.append($('<td>').text(convertTime(client.birth_time)).addClass('sort-birthdate').attr('data-date', client.birth_time));

        const editButton = $('<span style="cursor: pointer" class="text-blue">').click(function() {
            STATE.user_id = client.user_id;
            STATE.op = 'edit';
            $('#modal-edit-firstname').val(client.first_name);
            $('#modal-edit-lastname').val(client.last_name);
            $('#modal-edit-city').val(client.city);
            $('#modal-edit-address').val(client.address);
            $('#modal-edit-email').val(client.email);
            $('#modal-edit-phone').val(client.phone);
            $('#modal-edit-birthdate').val(convertTime(client.birth_time));

            
            $('#modal-edit').modal('show');
        }).append($(svgEditButton));

        const deleteButton = $('<span style="cursor: pointer" class="text-red">').click(function() {
            STATE.user_id = client.user_id;
            STATE.op = 'delete';
            $('#modal-delete-name').text(client.first_name + ' ' + client.last_name);
            $('#modal-delete').modal('show');
        }).append($(svgDeleteButton));

        row.append($('<td style="text-align: end;white-space: pre;">').append(editButton).append(deleteButton));

        table.append(row);
    });

    const list = new List('table-default', {
        sortClass: 'table-sort',
        listClass: 'table-tbody',
        valueNames: [
            'sort-lastname',
            'sort-firstname',
            'sort-poste',
            'sort-ville',
            'sort-adresse',
            'sort-email',
            'sort-tel',
            { name: 'sort-totalspent', attr: 'data-totalspent' },
            { name: 'sort-birthdate', attr: 'data-date' }
        ]
    });
}


$(document).ready(fetchClients);

$("#modal-delete-confirm").click(function() {
    $.ajax({
        url: '/api/Clients/DeleteClient?user_id=' + STATE.user_id,
        type: 'POST',
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                $('#modal-delete').modal('hide');
                fetchClients();
            }
        }
    });
});


$("#modal-edit").submit(function(e) {
    e.preventDefault();
    const data = {
        'user_id': STATE.user_id,
        'first_name': $('#modal-edit-firstname').val(),
        'last_name': $('#modal-edit-lastname').val(),
        'city': $('#modal-edit-city').val(),
        'address': $('#modal-edit-address').val(),
        'email': $('#modal-edit-email').val(),
        'phone': $('#modal-edit-phone').val(),
        'birth_time': parseTime($('#modal-edit-birthdate').val()),
    };

    $.ajax({
        url: '/api/Clients/UpdateClient',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-edit').modal('hide');
                fetchClients();
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
});

$("#navbar-actions-list").append($(`
<a href="#" class="btn btn-primary d-none d-sm-inline-block">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
    Nouveau client
</a>
<a href="#" class="btn btn-primary d-sm-none btn-icon" data-bs-toggle="modal"aria-label="Nouveau client">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
</a>
`).click(function() {
    $('#modal-create').modal('show');
}));

$("#modal-create").submit(function(e) {
    e.preventDefault();
    const data = {
        'first_name': $('#modal-create-firstname').val(),
        'last_name': $('#modal-create-lastname').val(),
        'city': $('#modal-create-city').val(),
        'address': $('#modal-create-address').val(),
        'email': $('#modal-create-email').val(),
        'phone': $('#modal-create-phone').val(),
        'birth_time': parseTime($('#modal-create-birthdate').val()),
    };

    $.ajax({
        url: '/api/Clients/CreateClient',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-create').modal('hide');
                fetchClients();
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

});