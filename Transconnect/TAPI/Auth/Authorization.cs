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

    /// <summary>
    /// Disconnect the user from the session.
    /// </summary>
    /// <param name="cfg">The CFG.</param>
    /// <param name="context">The context.</param>
    public async Task logout(AppConfig cfg, HttpContext context) {
        string? token = context.Request.Cookies["session_id"];
        if (token == null) return;
        context.Response.Cookies.Delete("session_id", new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        context.Response.Cookies.Delete("user_id", new CookieOptions() {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        await AuthorizationToken.delete_user_session(cfg, token);
    }

    /// <summary>
    /// Obtains the authorization from the context.
    /// </summary>
    /// <param name="cfg">The CFG.</param>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public static async Task<Authorization> obtain(AppConfig cfg, HttpContext context) {
        string? token = context.Request.Cookies["session_id"];
        if (token == null) return new Authorization(null);
        Person? user = await AuthorizationToken.get_user_from_token(cfg, token);
        return new Authorization(user);
    }

    /// <summary>
    /// Determines whether this instance is a valid employee.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance is employee; otherwise, <c>false</c>.
    /// </returns>
    public bool is_employee() {
        return user != null && user is Employee;
    }

    /// <summary>
    /// Returns an unauthorized error if the user is not an employee.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="System.Exception">Invalid call to get_unauthorized_error</exception>
    public ApiResponse<T> get_unauthorized_error<T>() {
        if (is_employee()) throw new Exception("Invalid call to get_unauthorized_error");
        else return ApiResponse<T>.Failure(401, "auth.unauthorized", "Unauthorized");
    }

    /// <summary>
    /// Casts the user as an employee.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.Exception">Unauthorized</exception>
    public Employee get_employee() {
        if (user == null || !(user is Employee)) throw new Exception("Unauthorized");
        return (Employee)user;
    }
}