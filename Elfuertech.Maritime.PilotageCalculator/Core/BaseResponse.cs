namespace Elfuertech.Maritime.PilotageCalculator.Core;

public class BaseResponse
{
    public bool Succeeded { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorDetail { get; set; }
    
    public BaseResponse()
    {
        Succeeded = true;
    }

    public BaseResponse(string errorCode, string errorDetail = null)
    {
        Succeeded = false;
        ErrorCode = errorCode;
        ErrorDetail = errorDetail;
    }
}