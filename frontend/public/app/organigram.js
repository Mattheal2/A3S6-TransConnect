$(document).ready(function() {
    $.ajax({
        url: '/api/Employees/GetEmployeesOrgChart',
        method: 'POST',
        cache: false,
        success: function(response) {
            if (response.error) {
                alert(response.error.message);
                return;
            }

            renderOrganigram(response.data);
        },
        error: alert
    });
});

function recGenStruct(employee) {
    const val = {
        text: {
            name: employee.value.first_name + ' ' + employee.value.last_name,
            title: employee.value.position
        },
        children: []
    }

    if (!employee.value.show_on_org_chart) {
        val.text.name = 'Employé masqué';
    }

    for (const child of employee.children) {
        const processed = recGenStruct(child);
        if (processed) val.children.push(processed);
    }

    return val;
}

function renderOrganigram(data) {
    const children = [];
    for (const child of data) {
        const processed = recGenStruct(child);
        if (processed) children.push(processed);
    }

    var config = {
        chart: {
            container: "#chart"
        },
        
        nodeStructure: {
            text: { 
                name: "Transconnect"
            },
            children: children
        }
    };

    console.log(config);

    
    new Treant( config );
}

if (!AUTH.isAuthenticated) {
    window.location.href = "/sign-in";
}