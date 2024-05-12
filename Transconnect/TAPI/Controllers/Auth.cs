using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;

namespace TAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost(Name = "Login")]
    public async Task<ApiResponse<bool>> Login() {
        // fetch the user from the database
        string? email = Request.Form["email"];
        string? password = Request.Form["password"];

        if (email == null || password == null)
            return ApiResponse<bool>.Failure(400, "auth.invalid_request", "Email or password not provided");

        Person? login_person = await Person.get_person_by_email(Config.cfg, email);
        if (login_person == null)
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect email or password");
    

        if (!login_person.check_password(password))
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect email or password");

        // OK - Create session
        string session_id = await AuthorizationToken.create_user_session(Config.cfg, login_person);
        
        Response.Cookies.Append("session_id", session_id, new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
        Response.Cookies.Append("user_id", login_person.user_id.ToString(), new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });

        return ApiResponse<bool>.Success(true);
    }

    [HttpPost(Name = "Logout")]
    public async Task<ApiResponse<bool>> Logout() {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_authorized()) return auth.get_unauthorized_error<bool>();

        await auth.logout(Config.cfg, Request.HttpContext);
        return ApiResponse<bool>.Success(true);
    }

    [HttpGet(Name = "GetLoggedInUser")]
    public async Task<ApiResponse<Person>> GetLoggedInUser() {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_authorized()) return auth.get_unauthorized_error<Person>();
        Person user = auth.get_user();

        return ApiResponse<Person>.Success(user);
    }

    [HttpPost(Name = "ChangePassword")]
    public async Task<ApiResponse<bool>> ChangePassword() {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_authorized()) return auth.get_unauthorized_error<bool>();
        Person user = auth.get_user();

        string? old_password = Request.Form["old_password"];
        string? new_password = Request.Form["new_password"];

        if (old_password == null || new_password == null)
            return ApiResponse<bool>.Failure(400, "auth.invalid_request", "Old or new password not provided");
        

        if (!user.check_password(old_password))
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect password");

        await user.update_password(Config.cfg, new_password);

        return ApiResponse<bool>.Success(true);
    }
}