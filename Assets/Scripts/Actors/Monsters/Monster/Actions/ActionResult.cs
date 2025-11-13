using System;

namespace Actors.Monsters.Actions
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

    public record ActionResult(
        ResultType ResultType,
        string Reason = null,
        Exception Exception = null)
    {
        public override string ToString()
        {
            var result = ResultType.ToString();

            if (!string.IsNullOrWhiteSpace(Reason))
                result += " - " + Reason;

            if (Exception != null)
                result += "\n" + Exception.ToString();

            return result;
        }

        public static implicit operator bool(ActionResult result) => result.ResultType == ResultType.Success;
    }
}
