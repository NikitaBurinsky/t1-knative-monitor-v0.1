using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace T1_KNative_Administrator_v03.Core.OpResult
{
    public record OpResult
    {
        public bool Succeeded;
        public string? Message;
        public HttpStatusCode? ErrCode;
        public bool Failed
        {
            get { return !Succeeded; }
        }
        public OpResult(bool succeeded, HttpStatusCode? errCode_ = null, string? errorText = null)
        {
            Succeeded = succeeded;
            Message = errorText;
            ErrCode = errCode_;
        }

        public static OpResult Success() => new OpResult(true);
        public static OpResult SuccessMessage(string message) => new OpResult(true, null, message);
        public static OpResult Error(string errString, HttpStatusCode? statusCode)
            => new OpResult(false, statusCode, errString);
        public virtual ObjectResult ToProblemDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Operation Failure",
                Detail = Message,
                Status = (int)ErrCode,
            };
            return new ObjectResult(problemDetails) { StatusCode = (int)ErrCode };
        }
    }
    public record OpResult<TData> : OpResult
    {
        public TData? Returns { get; }

        private OpResult(bool succeeded, HttpStatusCode? statusCode = null, string? errorText = null, TData? data = default)
            : base(succeeded, statusCode, errorText)
        {
            Returns = data;
        }

        public static OpResult<TData> Success(TData data) => new(true, null, null, data);

        public static new OpResult<TData> Error(string errorText, HttpStatusCode? statusCode)
            => new(false, statusCode, errorText);
        public static new OpResult<TData> Error(TData data, string errorText, HttpStatusCode? statusCode)
            => new(false, statusCode, errorText, data);

    }

}
