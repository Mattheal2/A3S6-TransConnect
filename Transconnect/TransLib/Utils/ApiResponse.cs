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

    public static ApiResponse<T> Success(T data) {
        ApiResponse<T> resp = new ApiResponse<T>(200);
        resp.data = data;
        return resp;
    }

    public static ApiResponse<T> Failure(int status_code, string code, string message) {
        ApiResponse<T> resp = new ApiResponse<T>(status_code);
        resp.error = new Error(message, code);
        return resp;
    }
}