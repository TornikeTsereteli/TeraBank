using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Response;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    
    public Object Data { get; set; }
    public ApiResponse(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
    public ApiResponse(int statusCode, string message, Object data)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }
    
    

    public ApiResponse(int statusCode, ModelStateDictionary modelState)
    {
        StatusCode = statusCode;
        Message = string.Join("; ", modelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage));
    }
}