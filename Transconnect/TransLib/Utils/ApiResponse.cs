namespace TransLib;

public class ApiResponse<T> {

    public class Error {
        public string message { get; }
        public string code { get; }
        public Error(string message, string code) {
            this.message = message;
            this.code = code;
        }
    }

    public int status_code {get; }
    public Error? error { get; private set; }
    public T? data { get; private set; }
    
    public ApiResponse(int status_code) {
        this.status_code = status_code;
    }

    /// <summary>
    /// Returns a success response (code 200) with the given data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public static ApiResponse<T> Success(T data) {
        ApiResponse<T> resp = new ApiResponse<T>(200);
        resp.data = data;
        return resp;
    }

    /// <summary>
    /// Returns a failure response with the given status code, code and message.
    /// </summary>
    /// <param name="status_code">The status code.</param>
    /// <param name="code">The code.</param>
    /// <param name="message">The message.</param>
    /// <returns></returns>
    public static ApiResponse<T> Failure(int status_code, string code, string message) {
        ApiResponse<T> resp = new ApiResponse<T>(status_code);
        resp.error = new Error(message, code);
        return resp;
    }
}