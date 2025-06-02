﻿using System.Net;
using System.Text.Json.Serialization;

namespace api_ratelimiting.Utils
{
    public class BaseResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = null!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        public static BaseResponse<T> Success(string message = "Success")
        {
            return new BaseResponse<T>
            {
                Message = message,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public static BaseResponse<T> Success(T data, string message = "Success")
        {
            return new BaseResponse<T>
            {
                Data = data,
                IsSuccess = true,
                Message = message,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public static BaseResponse<T> Fail(
            string message = "Fail.",
            HttpStatusCode statusCode = HttpStatusCode.BadRequest
        )
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
            };
        }

        public static BaseResponse<T> Fail(Exception ex)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = ex.ToString(),
                StatusCode = HttpStatusCode.InternalServerError,
            };
        }

        public static BaseResponse<T> NotFound(string message = "No data found.")
        {
            return BaseResponse<T>.Fail(message, HttpStatusCode.NotFound);
        }

        public static BaseResponse<T> Duplicate(string message = "Duplicate data.")
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = HttpStatusCode.Conflict,
            };
        }
    }
}
