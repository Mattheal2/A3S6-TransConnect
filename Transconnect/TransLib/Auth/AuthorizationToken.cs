using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Security.Cryptography;

namespace TransLib.Auth;

public static class AuthorizationToken {
    public static string generate_token() {
        byte[] token = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(token);
        }
        return System.Convert.ToBase64String(token);
    }

    public static async Task<string> create_user_session(Company comp, int user_id) {
        string token = generate_token();
        MySqlCommand cmd = new MySqlCommand($"INSERT INTO auth_tokens(token_id, user_id) VALUES(@token_id, @user_id)");
        cmd.Parameters.AddWithValue("@token_id", token);
        cmd.Parameters.AddWithValue("@user_id", user_id);
        await comp.query(cmd);
        return token;
    }

    public static async Task<int?> get_user_id_from_token(Company comp, string token) {
        MySqlCommand cmd = new MySqlCommand($"SELECT user_id FROM auth_tokens WHERE token_id = @token_id");
        cmd.Parameters.AddWithValue("@token_id", token);
        DbDataReader? reader = await comp.query(cmd);
        if (reader == null) throw new Exception("reader is null");
        using (reader)
        {
            if (await reader.ReadAsync()) {
                return reader.GetInt32(0);
            } else {
                return null;
            }
        }
    }
}