/* Common TransConnect JS */

const AUTH = function() {
    const user_id = document.cookie.split('; ').find(row => row.startsWith('user_id=')).split('=')[1];

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

        localStorage.setItem('employee_data', JSON.stringify({
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