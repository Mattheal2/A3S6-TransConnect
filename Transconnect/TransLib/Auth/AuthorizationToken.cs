using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Security.Cryptography;
using TransLib.Persons;

namespace TransLib.Auth;

/// <summary>
/// Class for handling authorization tokens/sessions.
/// </summary>
public static class AuthorizationToken {
    /// <summary>
    /// Generates 32-bytes random token.
    /// As string, it should be 44 characters long.
    /// </summary>
    /// <returns></returns>
    private static string generate_token() {
        byte[] token = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(token);
        }
        return System.Convert.ToBase64String(token);
    }

    /// <summary>
    /// Creates a new session for the user and returns the token.
    /// </summary>
    /// <param name="comp">Company</param>
    /// <param name="user">the Person to create a session for</param>
    /// <returns></returns>
    public static async Task<string> create_user_session(Company comp, Person user) {
        string token = generate_token();
        MySqlCommand cmd = new MySqlCommand($"INSERT INTO auth_tokens(token_id, user_id) VALUES(@token_id, @user_id)");
        cmd.Parameters.AddWithValue("@token_id", token);
        cmd.Parameters.AddWithValue("@user_id", user.user_id);
        await comp.query(cmd);
        return token;
    }

    /// <summary>
    /// Returns the user associated with the token, or null if the token is invalid.
    /// </summary>
    /// <param name="comp">Company</param>
    /// <param name="token">The cookie value</param>
    /// <returns></returns>
    public static async Task<Person?> get_user_from_token(Company comp, string token) {
        // with natural join on person table
        MySqlCommand cmd = new MySqlCommand($"SELECT * FROM auth_tokens NATURAL JOIN person WHERE token_id = @token_id LIMIT 1");
        cmd.Parameters.AddWithValue("@token_id", token);
        DbDataReader? reader = await comp.query(cmd);
        Person? authorized_user = await Person.from_reader_async(reader);

        return authorized_user;
    }

    /// <summary>
    /// Deletes the session associated with the token.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task delete_user_session(Company comp, string token) {
        MySqlCommand cmd = new MySqlCommand($"DELETE FROM auth_tokens WHERE token_id = @token_id");
        cmd.Parameters.AddWithValue("@token_id", token);
        await comp.query(cmd);
    }
}