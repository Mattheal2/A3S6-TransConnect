using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransLib.Auth;
using TransLib.Persons;

namespace TAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost(Name = "Login")]
    public async Task<PasswordAuthenticator.PasswordAuthenticationResult> PostLogin() {
        // fetch the user from the database
        string? email = Request.Form["email"];
        string? password = Request.Form["password"];

        if (email == null || password == null)
            return new PasswordAuthenticator.PasswordAuthenticationResult(false, "Email or password not provided");

        Person? login_person = await Person.get_person_by_email(Config.transconnect, email);
        if (login_person == null)
            return new PasswordAuthenticator.PasswordAuthenticationResult(false, "User not found");
    

        if (!login_person.check_password(password))
            return new PasswordAuthenticator.PasswordAuthenticationResult(false, "Incorrect password");

        // OK - Create session
        string session_id = await AuthorizationToken.create_user_session(Config.transconnect, login_person.user_id, login_person.user_type);
        
        Response.Cookies.Append("session_id", session_id, new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
        Response.Cookies.Append("user_id", login_person.user_id.ToString(), new CookieOptions() {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });

        return new PasswordAuthenticator.PasswordAuthenticationResult(true, "Login successful");
    }

    [HttpPost(Name = "Logout")]
    public PasswordAuthenticator.PasswordAuthenticationResult PostLogout() {
        Response.Cookies.Delete("session_id");
        Response.Cookies.Delete("user_id");
        return new PasswordAuthenticator.PasswordAuthenticationResult(true, "Logout successful");
    }
}