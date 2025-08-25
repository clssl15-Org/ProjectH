using System;

public readonly struct ActionResult
{
    public enum ResultType
    {
        Success,
        AlreadyDoing,
        OtherActionExecuting,
        NotFound,
        Interrupted
    }

    public ResultType Result { get; }
    public string Reason { get; }
    public Exception Exception { get; }


    public ActionResult(ResultType result, string reason = null, Exception exception = null)
    {
        Result = result;
        Reason = reason ?? string.Empty;
        Exception = exception;
    }

    public override string ToString()
    {
        var result = Result.ToString();

        if (!string.IsNullOrWhiteSpace(Reason))
            result += " - " + Reason;

        if (Exception != null)
            result += "\n" + Exception.ToString();

        return result;
    }

    public static implicit operator bool(ActionResult result) => result.Result == ResultType.Success;
}

