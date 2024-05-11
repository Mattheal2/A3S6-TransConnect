namespace TransLib.Auth;

public struct AuthorizedUser {
    public int user_id { get; }
    public string user_type { get; }
    public string token { get; }

    public AuthorizedUser(int user_id, string user_type, string token) {
        this.user_id = user_id;
        this.user_type = user_type;
        this.token = token;
    }

    public override string ToString() {
        string token_hash = token.Substring(0, 2) + "..." + token.Substring(token.Length - 2);
        return $"AuthorizedUser(user_id: {user_id}, user_type: {user_type}, token: {token_hash})";
    }
}