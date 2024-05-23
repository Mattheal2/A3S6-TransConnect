function fetchOrders(callback = null) {
    $.ajax({
        url: '/api/Order/ListOrder',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                STATE._orders = resp.data;
                if (callback) callback(resp.data);
                else renderOrders(resp.data);
            }
        }
    });
}

function fetchEmployees(callback) {
    $.ajax({
        url: '/api/Employees/GetEmployees?order_field=last_name&order_dir=ASC',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                STATE._employees = resp.data;

                if (callback) callback();
            }
        }
    });
}

function fetchClients(callback) {
    $.ajax({
        url: '/api/Clients/GetClients?order_field=last_name&order_dir=ASC',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                STATE._clients = resp.data;

                const clients_datalist = $('#liste-clients');
                resp.data.forEach(function(client) {
                    clients_datalist.append($('<option>').attr('value', client.first_name + ' ' + client.last_name).attr('data-id', client.user_id));
                });

                if (callback) callback();
            }
        }
    });
}

function initPage() {
    fetchOrders(trackPageInit);
    fetchEmployees(trackPageInit);
    fetchClients(trackPageInit);
}

function trackPageInit() {
    STATE._progress++;
    if (STATE._progress === 3) {
        renderOrders(STATE._orders);
    }
}
    



function getVehicleData(vehicle) {
    if (vehicle.car !== null) return vehicle.car;
    if (vehicle.truck !== null) return vehicle.truck;
    if (vehicle.van !== null) return vehicle.van;

    throw new Error('Vehicle type not found');
}

function getVehicleType(vehicle) {
    if (vehicle.car !== null) return 'Voiture';
    if (vehicle.truck !== null) return 'Camion';
    if (vehicle.van !== null) return 'Fourgon';

    throw new Error('Vehicle type not found');
}


const STATE = {
    order_id: null,
    op: null,
    _clients: null,
    _vehicles: null,
    _employees: null,
    _orders: null,
    _progress: 0
};

`<th><button class="table-sort" data-sort="sort-id">Numéro</button></th>
<th><button class="table-sort" data-sort="sort-client">Client</button></th>
<th><button class="table-sort" data-sort="sort-departure-time">Début</button></th>
<th><button class="table-sort" data-sort="sort-arrival-time">Fin</button></th>
<th><button class="table-sort" data-sort="sort-departure-city">Départ</button></th>
<th><button class="table-sort" data-sort="sort-arrival-city">Arrivée</button></th>
<th><button class="table-sort" data-sort="sort-vehicle">Véhicule</button></th>
<th><button class="table-sort" data-sort="sort-driver">Conducteur</button></th>
<th><button class="table-sort" data-sort="sort-price">Prix</button></th>
<th><button class="table-sort" data-sort="sort-kmprice">Prix au km</button></th>
<th><button class="table-sort" data-sort="sort-distance">Distance</button></th>
<th><button class="table-sort" data-sort="sort-toll">Péage</button></th>
<th><button class="table-sort" data-sort="sort-duration">Durée</button></th>`

const svgEditButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-edit"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M7 7h-1a2 2 0 0 0 -2 2v9a2 2 0 0 0 2 2h9a2 2 0 0 0 2 -2v-1" /><path d="M20.385 6.585a2.1 2.1 0 0 0 -2.97 -2.97l-8.415 8.385v3h3l8.385 -8.415z" /><path d="M16 5l3 3" /></svg>`;
const svgDeleteButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-trash-x"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 7h16" /><path d="M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2 -2l1 -12" /><path d="M9 7v-3a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v3" /><path d="M10 12l4 4m0 -4l-4 4" /></svg>`;

function formatDuration(duration) {
    const hours = Math.floor(duration / 3600);
    const minutes = Math.floor((duration % 3600) / 60);
    const seconds = duration % 60;

    let result = '';
    if (hours > 0) result += hours + 'h ';
    if (minutes > 0) result += minutes + 'm ';
    if (seconds > 0) result += seconds + 's';
    return result;
}

function formatDistance(distance) {
    return (distance/1000).toFixed(2) + ' km';
}

function applyVehicleFilter() {
    selected = $('#modal-create-vehicletype').val();
    $('.not-if-any-vehicle').hide();
    $('.if-' + selected).show();
}


$("#modal-create-vehicletype").change(applyVehicleFilter);

async function renderOrders(data) {
    const pageFilters = document.location.search.substring(1).split('&').map(e => e.split('=')).reduce((acc, e) => { acc[e[0]] = e[1]; return acc; }, {});

    var table = $('#orderstable');

    table.empty();
    await data.forEach(function (order) {
        if (pageFilters.client_id && order.client_id !== parseInt(pageFilters.client_id)) return;
        if (pageFilters.driver_id && order.driver_id !== parseInt(pageFilters.driver_id)) return;
        if (order.route === null) order.route = { distance: 0, cost: 0, time: 0 };
        const client = STATE._clients.find(c => c.user_id === order.client_id) || { first_name: 'Client', last_name: 'Inconnu' };
        const employee = STATE._employees.find(e => e.user_id === order.driver_id) || { first_name: 'Conducteur', last_name: 'Inconnu' };

        const row = $('<tr>');
        row.append($('<td>').text(client.first_name + ' ' + client.last_name).addClass('sort-client'));
        row.append($('<td>').text(convertTimeFull(order.departure_time)).addClass('sort-departure-time').attr('data-date', order.departure_time));
        row.append($('<td>').text(convertTimeFull(order.arrival_time)).addClass('sort-arrival-time').attr('data-date', order.arrival_time));
        row.append($('<td>').text(order.departure_city).addClass('sort-departure-city'));
        row.append($('<td>').text(order.arrival_city).addClass('sort-arrival-city'));
        row.append($('<td>').text(order.vehicle_license_plate).addClass('sort-vehicle'));
        row.append($('<td>').text(employee.first_name + ' ' + employee.last_name).addClass('sort-driver'));
        row.append($('<td>').text(formatPrice(order.total_price)).addClass('sort-price').attr('data-price', order.price));
        row.append($('<td>').text(formatPrice(order.price_per_km)).addClass('sort-kmprice').attr('data-price', order.km_price));
        row.append($('<td>').text(formatDistance(order.route.distance)).addClass('sort-distance'));
        row.append($('<td>').text(formatPrice(order.route.cost)).addClass('sort-toll').attr('data-price', order.toll));
        row.append($('<td>').text(formatDuration(order.route.time)).addClass('sort-duration').attr('data-duration', order.duration));



        // const editButton = $('<span style="cursor: pointer" class="text-blue">').click(function() {
        //     STATE.order_id = order.order_id;
        //     STATE.op = 'edit';
            
        //     $('.if-edit').show();
        //     $('.if-create').hide();

        //     $('#modal-create-client').val(client.first_name + ' ' + client.last_name);

        //     $('#modal-create-departuretime').val(convertTimeISO(order.departure_time));
        //     $('#modal-create-departurecity').val(order.departure_city);
        //     $('#modal-create-arrivalcity').val(order.arrival_city);
        //     $('#modal-create-trucktype').val(order.vehicle_license_plate);

            
        //     applyVehicleFilter();
        //     $('#modal-create').modal('show');
        // }).append($(svgEditButton));

        const deleteButton = $('<span style="cursor: pointer" class="text-red">').click(function() {
            STATE.order_id = order.order_id;
            STATE.op = 'delete';
            $('#modal-delete-name').text(client.first_name + ' ' + client.last_name);
            $('#modal-delete').modal('show');
        }).append($(svgDeleteButton));

        row.append($('<td style="text-align: end;white-space: pre;">').append(deleteButton));

        table.append(row);
    });

    const list = new List('table-default', {
        sortClass: 'table-sort',
        listClass: 'table-tbody',
        valueNames: [
            'sort-client',
            { name: 'sort-departure-time', attr: 'data-date' },
            { name: 'sort-arrival-time', attr: 'data-date' },
            'sort-departure-city',
            'sort-arrival-city',
            'sort-vehicle',
            'sort-driver',
            { name: 'sort-price', attr: 'data-price' },
            { name: 'sort-kmprice', attr: 'data-price' },
            { name: 'sort-distance', attr: 'data-distance'},
            { name: 'sort-toll', attr: 'data-price' },
            'sort-duration'
        ]
    });
}


$(document).ready(initPage);

$("#modal-delete-confirm").click(function() {
    $.ajax({
        url: '/api/Order/DeleteOrder?order_id=' + STATE.order_id,
        type: 'POST',
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                $('#modal-delete').modal('hide');
                fetchOrders();
            }
        }
    });
});

$("#navbar-actions-list").append($(`
<a href="#" class="btn btn-primary d-none d-sm-inline-block">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
    Nouvelle commande
</a>
<a href="#" class="btn btn-primary d-sm-none btn-icon" data-bs-toggle="modal"aria-label="Nouvelle commande">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
</a>
`).click(function() {
    $('.if-edit').hide();
    $('.if-create').show();
    $('#modal-create-vehicletype').val('car');
    $('#modal-create-vehicletype').change();
    applyVehicleFilter();
    $('#modal-create-client').val('');
    $('#modal-create-departuretime').val('');
    $('#modal-create-departurecity').val('');
    $('#modal-create-arrivalcity').val('');
    $('#modal-create-trucktype').val('');

    $('#modal-create').modal('show');
    STATE.order_id = null;
    STATE.op = 'create';
}));

$("#modal-create").submit(function(e) {
    e.preventDefault();
    const data = {
        'order_id': STATE.order_id,
        'client_id': $('#liste-clients option[value="' + $('#modal-create-client').val() + '"]').attr('data-id'),
        'vehicle_type': $('#modal-create-vehicletype').val(),
        'truck_type': $('#modal-create-trucktype').val(),
        'departure_time': parseTimeISO($('#modal-create-departuretime').val()),
        'departure_city': $('#modal-create-departurecity').val(),
        'arrival_city': $('#modal-create-arrivalcity').val(),
    };

    $.ajax({
        url: STATE.op === 'edit' ? '/api/Order/UpdateOrder' : '/api/Order/CreateOrder',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-create').modal('hide');
                fetchOrders();
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

});

if (!AUTH.isAuthenticated) {
    window.location.href = "/sign-in";
}