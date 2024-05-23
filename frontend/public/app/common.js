/* Common TransConnect JS */

const AUTH = function() {
    let user_id = document.cookie.split('; ').find(row => row.startsWith('user_id='))?.split('=')[1];

    const stored = JSON.parse(localStorage.getItem('employee_data'));
    let employee = null;
    // 5 minutes cache on employee data
    // if (stored !== null && stored.timestamp > new Date().getTime() - 300 * 1000) {
    //     employee = stored.data;
    // }

    return {
        isAuthenticated: user_id !== undefined,
        user_id: user_id,
        employee: employee
    };
}();

$(document).ready(function() {
    function onLoginSuccess(response) {
        if (response.error) {
            if (response.error.code === "auth.unauthorized") {
                document.cookie = 'user_id=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
                window.location.href = '/sign-in.html';
                return;
            }
            alert(response.error.message);
            return;
        }

        AUTH.employee = response.data;

        localStorage.setItem('employee_data', encodeJSON({
            data: AUTH.employee,
            timestamp: new Date().getTime()
        }));

        onReady();
    }

    function onReady() {
        $(".tcdata-auth-fullname").text(AUTH.employee.first_name + ' ' + AUTH.employee.last_name);
        $(".tcdata-auth-role").text(AUTH.employee.position);

        $(".tcbtn-auth-logout").click(function() {
            $.ajax({
                url: '/api/Auth/Logout',
                method: 'POST',
                cache: false,
                success: function() {
                    window.location.href = '/sign-in.html';
                    document.cookie = 'user_id=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
                    localStorage.removeItem('employee_data');
                },
                error: alert
            });
        });
    }

    if (AUTH.isAuthenticated) {
        if (AUTH.employee !== null) {
            onReady();
        } else {
            $.ajax({
                url: '/api/Auth/GetLoggedInEmployee',
                method: 'GET',
                cache: false,
                success: onLoginSuccess,
                error: alert
            });
        }
    }
});

function encodeJSON(data) {
    const ser = JSON.stringify(data);
    return ser.replace(/[\u007F-\uFFFF]/g, function(chr) {
        return "\\u" + ("0000" + chr.charCodeAt(0).toString(16)).substr(-4)
    });
}

function convertTime(time) {
    var date = new Date(time * 1000);
    // DD/MM/YYYY
    return date.toLocaleDateString('fr-FR');
}

function parseTime(localeDateString) {
    const parts = localeDateString.split('/');
    const date = new Date(parts[2], parts[1]-1, parts[0]);
    return date.getTime() / 1000;
}

function convertTimeFull(time) {
    var date = new Date(time * 1000);
    // DD/MM/YYYY HH:MM:SS
    return date.toLocaleString('fr-FR');
}

function parseTimeFull(localeString) {
    const parts = localeString.split(' ');
    const date = parseTime(parts[0]);
    const time = parts[1].split(':');
    return date + parseInt(time[0]) * 3600 + parseInt(time[1]) * 60 + parseInt(time[2]);
}

function parseTimeISO(universalString) {
    // DD/MM/YYYY HH:MM
    const date = new Date(universalString);
    return date.getTime() / 1000;
}

function convertTimeISO(time) {
    var date = new Date(time * 1000);
    
    return date.toISOString().slice(0, 16);
}


function formatPrice(price) {
    const integer = Math.floor(price / 100);
    const decimal = price % 100;
    return `${integer}.${decimal.toString().padStart(2, '0')} €`;
}
function parsePrice(price) {
    const parts = price.split('.');
    const integer = parseInt(parts[0]) || 0;
    const decimal = parseInt(parts[1]) || 0;
    return integer * 100 + decimal;
}
