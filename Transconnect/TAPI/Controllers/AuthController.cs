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
    public struct LoginRequest {
        public string email { get; set; }
        public string password { get; set; }
    }

    [HttpPost(Name = "Login")]
    public async Task<ApiResponse<bool>> Login([FromBody] LoginRequest body) {
        Person? login_person = await Person.get_person_by_email(Config.cfg, body.email);
        if (login_person == null)
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect email or password");
    

        if (!login_person.check_password(body.password))
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect email or password");

        if (!(login_person is Employee))
            return ApiResponse<bool>.Failure(401, "auth.login_forbidden", "Login forbidden for this user type");

        // OK - Create session
        string session_id = await AuthorizationToken.create_user_session(Config.cfg, login_person);
        
        Response.Cookies.Append("session_id", session_id, new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        Response.Cookies.Append("user_id", login_person.user_id.ToString(), new CookieOptions() {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        return ApiResponse<bool>.Success(true);
    }

    [HttpPost(Name = "Logout")]
    public async Task<ApiResponse<bool>> Logout() {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<bool>();

        await auth.logout(Config.cfg, Request.HttpContext);
        return ApiResponse<bool>.Success(true);
    }

    [HttpGet(Name = "GetLoggedInUser")]
    public async Task<ApiResponse<Employee>> GetLoggedInEmployee() {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Employee>();
        Employee user = auth.get_employee();

        return ApiResponse<Employee>.Success(user);
    }
    
    public struct ChangePasswordRequest {
        public string old_password { get; set; }
        public string new_password { get; set; }
    }

    [HttpPost(Name = "ChangePassword")]
    public async Task<ApiResponse<bool>> ChangePassword([FromBody] ChangePasswordRequest body) {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<bool>();
        Employee user = auth.get_employee();

        if (body.old_password == body.new_password)
            return ApiResponse<bool>.Failure(400, "auth.invalid_password", "New password cannot be the same as the old password");

        if (!user.check_password(body.old_password))
            return ApiResponse<bool>.Failure(401, "auth.invalid_credentials", "Incorrect password");

        await user.update_password(Config.cfg, body.new_password);

        return ApiResponse<bool>.Success(true);
    }
}