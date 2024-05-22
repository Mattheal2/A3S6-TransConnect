function fetchEmployees() {
    $.ajax({
        url: '/api/Employees/GetEmployees?order_field=last_name&order_dir=ASC',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                renderEmployees(resp.data);
            }
        }
    });
}

const STATE = {
    user_id: null,
    op: null
};

function convertTime(time) {
    var date = new Date(time * 1000);
    return date.toLocaleDateString();
}

function parseTime(localeDateString) {
    const parts = localeDateString.split('/');
    const date = new Date(parts[2], parts[0] - 1, parts[1]);
    return date.getTime() / 1000;
}

const svgEditButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-edit"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M7 7h-1a2 2 0 0 0 -2 2v9a2 2 0 0 0 2 2h9a2 2 0 0 0 2 -2v-1" /><path d="M20.385 6.585a2.1 2.1 0 0 0 -2.97 -2.97l-8.415 8.385v3h3l8.385 -8.415z" /><path d="M16 5l3 3" /></svg>`;
const svgDeleteButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-trash-x"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 7h16" /><path d="M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2 -2l1 -12" /><path d="M9 7v-3a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v3" /><path d="M10 12l4 4m0 -4l-4 4" /></svg>`;

async function renderEmployees(data) {
    var table = $('#employeestable');

    var employees_datalist = $('#liste-employes');
    employees_datalist.empty();
    table.empty();
    await data.forEach(function (employee) {
        var row = $('<tr>');
        row.append($('<td>').text(employee.last_name).addClass('sort-lastname'));
        row.append($('<td>').text(employee.first_name).addClass('sort-firstname'));
        row.append($('<td>').text(employee.position).addClass('sort-poste'));
        row.append($('<td>').text(employee.city).addClass('sort-ville'));
        row.append($('<td>').text(employee.email).addClass('sort-email'));
        row.append($('<td>').text(employee.phone).addClass('sort-tel'));
        row.append($('<td>').text(convertTime(employee.hire_time)).addClass('sort-date').attr('data-date', employee.hire_time));
        row.append($('<td>').text(employee.salary).addClass('sort-salaire'));
        row.append($('<td>').text(employee.license_type).addClass('sort-licensetype'));
        row.append($('<td>').text(convertTime(employee.birth_time)).addClass('sort-birthdate').attr('data-date', employee.birth_time));

        const editButton = $('<span style="cursor: pointer" class="text-blue">').click(function() {
            STATE.user_id = employee.user_id;
            STATE.op = 'edit';
            $('#modal-edit-firstname').val(employee.first_name);
            $('#modal-edit-lastname').val(employee.last_name);
            $('#modal-edit-position').val(employee.position);
            $('#modal-edit-city').val(employee.city);
            $('#modal-edit-address').val(employee.address);
            $('#modal-edit-email').val(employee.email);
            $('#modal-edit-phone').val(employee.phone);
            $('#modal-edit-hiredate').val(convertTime(employee.hire_time));
            $('#modal-edit-salary').val(employee.salary);
            $('#modal-edit-licensetype').val(employee.license_type);
            $('#modal-edit-birthdate').val(convertTime(employee.birth_time));
            $('#modal-edit-password').val('');
            $('#modal-edit-orgchart').prop('checked', employee.show_on_org_chart);
            const supervisor = data.find(e => e.user_id === employee.supervisor_id);
            $('#modal-edit-supervisor').val(supervisor ? supervisor.first_name + ' ' + supervisor.last_name : '');
            
            $('#modal-edit').modal('show');
        }).append($(svgEditButton));

        const deleteButton = $('<span style="cursor: pointer" class="text-red">').click(function() {
            STATE.user_id = employee.user_id;
            STATE.op = 'delete';
            $('#modal-delete-name').text(employee.first_name + ' ' + employee.last_name);
            $('#modal-delete').modal('show');
        }).append($(svgDeleteButton));

        row.append($('<td>').append(editButton).append(deleteButton));

        table.append(row);

        employees_datalist.append($('<option>').attr('value', employee.first_name + ' ' + employee.last_name).attr('data-id', employee.user_id));
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
            { name: 'sort-date', attr: 'data-date' },
            'sort-salaire',
            'sort-licensetype',
            { name: 'sort-birthdate', attr: 'data-date' }
        ]
    });
}


$(document).ready(fetchEmployees);

$("#modal-delete-confirm").click(function() {
    $.ajax({
        url: '/api/Employees/DeleteEmployee?user_id=' + STATE.user_id,
        type: 'POST',
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                $('#modal-delete').modal('hide');
                fetchEmployees();
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
        'position': $('#modal-edit-position').val(),
        'city': $('#modal-edit-city').val(),
        'address': $('#modal-edit-address').val(),
        'email': $('#modal-edit-email').val(),
        'phone': $('#modal-edit-phone').val(),
        'hire_time': parseTime($('#modal-edit-hiredate').val()),
        'salary': $('#modal-edit-salary').val(),
        'license_type': $('#modal-edit-licensetype').val(),
        'birth_time': parseTime($('#modal-edit-birthdate').val()),
        'password': $('#modal-edit-password').val(),
        'show_on_org_chart': $('#modal-edit-orgchart').prop('checked'),
        'supervisor_id': $('#liste-employes option[value="' + $('#modal-edit-supervisor').val() + '"]').attr('data-id')
    };

    $.ajax({
        url: '/api/Employees/UpdateEmployee',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-edit').modal('hide');
                fetchEmployees();
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
    Nouvel employé
</a>
<a href="#" class="btn btn-primary d-sm-none btn-icon" data-bs-toggle="modal"aria-label="Nouvel employé">
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
        'position': $('#modal-create-position').val(),
        'city': $('#modal-create-city').val(),
        'address': $('#modal-create-address').val(),
        'email': $('#modal-create-email').val(),
        'phone': $('#modal-create-phone').val(),
        'hire_time': parseTime($('#modal-create-hiredate').val()),
        'salary': $('#modal-create-salary').val(),
        'license_type': $('#modal-create-licensetype').val(),
        'birth_time': parseTime($('#modal-create-birthdate').val()),
        'password': $('#modal-create-password').val(),
        'show_on_org_chart': $('#modal-create-orgchart').prop('checked'),
        'supervisor_id': $('#liste-employes option[value="' + $('#modal-create-supervisor').val() + '"]').attr('data-id')
    };

    $.ajax({
        url: '/api/Employees/CreateEmployee',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-create').modal('hide');
                fetchEmployees();
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

});