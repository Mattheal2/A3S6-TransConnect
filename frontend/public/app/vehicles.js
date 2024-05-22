function fetchVehicles() {
    $.ajax({
        url: '/api/Vehicle/ListVehicles?order_field=brand&order_dir=ASC',
        type: 'GET',
        cache: false,
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                renderVehicles(resp.data);
            }
        }
    });
}

const STATE = {
    license_plate: null,
    op: null
};

const svgEditButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-edit"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M7 7h-1a2 2 0 0 0 -2 2v9a2 2 0 0 0 2 2h9a2 2 0 0 0 2 -2v-1" /><path d="M20.385 6.585a2.1 2.1 0 0 0 -2.97 -2.97l-8.415 8.385v3h3l8.385 -8.415z" /><path d="M16 5l3 3" /></svg>`;
const svgDeleteButton = `<svg  xmlns="http://www.w3.org/2000/svg"  width="24"  height="24"  viewBox="0 0 24 24"  fill="none"  stroke="currentColor"  stroke-width="2"  stroke-linecap="round"  stroke-linejoin="round"  class="icon icon-tabler icons-tabler-outline icon-tabler-trash-x"><path stroke="none" d="M0 0h24v24H0z" fill="none"/><path d="M4 7h16" /><path d="M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2 -2l1 -12" /><path d="M9 7v-3a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v3" /><path d="M10 12l4 4m0 -4l-4 4" /></svg>`;

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

function applyVehicleFilter() {
    selected = $('#modal-create-type').val();
    $('.not-if-any-vehicle').hide();
    $('.if-' + selected).show();
}

$("#modal-create-type").change(applyVehicleFilter);

async function renderVehicles(data) {
    var table = $('#vehiclestable');

    table.empty();
    await data.forEach(function (vehicle) {
        const vehicleData = getVehicleData(vehicle);
        const vehicleType = getVehicleType(vehicle);

        const row = $('<tr>');
        row.append($('<td>').text(vehicleType).addClass('sort-type'));
        row.append($('<td>').text(vehicleData.license_plate).addClass('sort-licenseplate'));
        row.append($('<td>').text(vehicleData.brand).addClass('sort-brand'));
        row.append($('<td>').text(vehicleData.model).addClass('sort-model'));
        row.append($('<td>').text(formatPrice(vehicleData.price)).addClass('sort-price').attr('data-price', vehicleData.price));
        row.append($('<td>').text(vehicleData.usage).addClass('sort-usage'));
        row.append($('<td>').text(vehicleData.seats).addClass('sort-seats'));
        row.append($('<td>').text(vehicleData.truck_type).addClass('sort-trucktype'));
        row.append($('<td>').text(vehicleData.volume).addClass('sort-volume'));


        const editButton = $('<span style="cursor: pointer" class="text-blue">').click(function() {
            STATE.license_plate = vehicleData.license_plate;
            STATE.op = 'edit';
            
            $('.if-edit').show();
            $('.if-create').hide();

            $('#modal-create-type').val(vehicleData.vehicle_type.toLowerCase());
            $('#modal-create-licenseplate').val(vehicleData.license_plate).prop('readonly', true);
            $('#modal-create-brand').val(vehicleData.brand);
            $('#modal-create-model').val(vehicleData.model);
            $('#modal-create-price').val(formatPrice(vehicleData.price));
            $('#modal-create-usage').val(vehicleData.usage);
            $('#modal-create-seats').val(vehicleData.seats);
            $('#modal-create-trucktype').val(vehicleData.truck_type);
            $('#modal-create-volume').val(vehicleData.volume);
            
            applyVehicleFilter();
            $('#modal-create').modal('show');
        }).append($(svgEditButton));

        const deleteButton = $('<span style="cursor: pointer" class="text-red">').click(function() {
            STATE.license_plate = vehicleData.license_plate;
            STATE.op = 'delete';
            $('#modal-delete-name').text(vehicleData.license_plate);
            $('#modal-delete').modal('show');
        }).append($(svgDeleteButton));

        row.append($('<td style="text-align: end">').append(editButton).append(deleteButton));

        table.append(row);
    });

    const list = new List('table-default', {
        sortClass: 'table-sort',
        listClass: 'table-tbody',
        valueNames: [
            'sort-type',
            'sort-licenseplate',
            'sort-brand',
            'sort-model',
            { name: 'sort-price', attr: 'data-price' },
            'sort-usage',
            'sort-seats',
            'sort-trucktype',
            'sort-volume'
        ]
    });
}


$(document).ready(fetchVehicles);

$("#modal-delete-confirm").click(function() {
    $.ajax({
        url: '/api/Vehicle/DeleteVehicle?license_plate=' + STATE.license_plate,
        type: 'POST',
        success: function (resp) {
            if (resp.error) {
                alert(resp.error.message);
            } else {
                $('#modal-delete').modal('hide');
                fetchVehicles();
            }
        }
    });
});

$("#navbar-actions-list").append($(`
<a href="#" class="btn btn-primary d-none d-sm-inline-block">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
    Nouveau véhicule
</a>
<a href="#" class="btn btn-primary d-sm-none btn-icon" data-bs-toggle="modal"aria-label="Nouveau véhicule">
    <!-- Download SVG icon from http://tabler-icons.io/i/plus -->
    <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"><path stroke="none" d="M0 0h24v24H0z" fill="none"></path><path d="M12 5l0 14"></path><path d="M5 12l14 0"></path></svg>
</a>
`).click(function() {
    $('.if-edit').hide();
    $('.if-create').show();
    $('#modal-create-type').val('car');
    $('#modal-create-type').change();
    applyVehicleFilter();
    $('#modal-create-licenseplate').val('').prop('readonly', false);
    $('#modal-create-brand').val('');
    $('#modal-create-model').val('');
    $('#modal-create-price').val('');
    $('#modal-create-usage').val('');
    $('#modal-create-seats').val('');
    $('#modal-create-trucktype').val('');
    $('#modal-create-volume').val('');
    $('#modal-create').modal('show');
    STATE.license_plate = null;
    STATE.op = 'create';
}));

$("#modal-create").submit(function(e) {
    e.preventDefault();
    const data = {
        'type': $('#modal-create-type').val(),
        'license_plate': $('#modal-create-licenseplate').val(),
        'brand': $('#modal-create-brand').val(),
        'model': $('#modal-create-model').val(),
        'price': parsePrice($('#modal-create-price').val()),
        'usage': $('#modal-create-usage').val(),
        'seats': parseInt($('#modal-create-seats').val()),
        'truck_type': $('#modal-create-trucktype').val(),
        'volume': parseInt($('#modal-create-volume').val())
    };

    $.ajax({
        url: STATE.op === 'edit' ? '/api/Vehicle/UpdateVehicle' : '/api/Vehicle/AddVehicle',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: encodeJSON(data),
        success: function (resp) {
            if (resp.error || resp.errors) {
                alert(resp.error.message || resp.errors["$"][0]);
            } else {
                $('#modal-create').modal('hide');
                fetchVehicles();
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

});