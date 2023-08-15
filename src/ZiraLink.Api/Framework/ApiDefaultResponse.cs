using ZiraLink.Api.Application.Framework;

namespace ZiraLink.Api.Framework
{
    public class ApiDefaultResponse : ApiResponse<object>
    {
        public new static ApiDefaultResponse CreateSuccessResponse()
        {
            return new ApiDefaultResponse
            {
                Data = default(object),
                Status = true,
                ErrorCode = 0,
                ErrorMessage = String.Empty
            };
        }

        public new static ApiDefaultResponse CreateSuccessResponse(object data)
        {
            return new ApiDefaultResponse
            {
                Data = data,
                Status = true,
                ErrorCode = 0,
                ErrorMessage = String.Empty
            };
        }

        public new static ApiDefaultResponse CreateFailureResponse()
        {
            return new ApiDefaultResponse
            {
                Data = default(object),
                Status = false,
                ErrorCode = 9000,
                ErrorMessage = String.Empty
            };
        }

        public new static ApiDefaultResponse CreateFailureResponse(string errorMessage)
        {
            return new ApiDefaultResponse
            {
                Data = default(object),
                Status = false,
                ErrorCode = 9000,
                ErrorMessage = errorMessage
            };
        }

        public new static ApiDefaultResponse CreateFailureResponse(int errorCode)
        {
            return new ApiDefaultResponse
            {
                Data = default(object),
                Status = false,
                ErrorCode = errorCode,
                ErrorMessage = String.Empty
            };
        }

        public new static ApiDefaultResponse CreateFailureResponse(string errorMessage, int errorCode)
        {
            return new ApiDefaultResponse
            {
                Data = default(object),
                Status = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
