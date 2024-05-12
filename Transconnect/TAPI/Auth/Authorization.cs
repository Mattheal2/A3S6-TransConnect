namespace TAPI.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;

public class Authorization {
    private Person? user;
    private Authorization(Person? user) {
        this.user = user;
    }

    public override string ToString() {
        return user == null ? "Unauthorized" : user.ToString();
    }

    public async Task logout(AppConfig cfg, HttpContext context) {
        string? token = context.Request.Cookies["session_id"];
        if (token == null) return;
        context.Response.Cookies.Delete("session_id");
        context.Response.Cookies.Delete("user_id");
        await AuthorizationToken.delete_user_session(cfg, token);
    }

    public static async Task<Authorization> obtain(AppConfig cfg, HttpContext context) {
        string? token = context.Request.Cookies["session_id"];
        if (token == null) return new Authorization(null);
        Person? user = await AuthorizationToken.get_user_from_token(cfg, token);
        return new Authorization(user);
    }

    public bool is_employee() {
        return user != null && user is Employee;
    }

    public ApiResponse<T> get_unauthorized_error<T>() {
        if (is_employee()) throw new Exception("Invalid call to get_unauthorized_error");
        else return ApiResponse<T>.Failure(401, "auth.unauthorized", "Unauthorized");
    }

    public Employee get_employee() {
        if (user == null || !(user is Employee)) throw new Exception("Unauthorized");
        return (Employee)user;
    }
}