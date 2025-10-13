using System;

public record ActionResult(
    ActionResult.ResultType Result,
    string Reason = null,
    Exception Exception = null)
{
    public enum ResultType
    {
        Success,
        AlreadyDoing,
        OtherActionDoing,
        NotFound,
        Interrupted,
        InvalidOperation,
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
