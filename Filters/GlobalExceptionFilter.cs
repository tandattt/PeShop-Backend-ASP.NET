using PeShop.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Common;

namespace PeShop.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            if (ex is NotFoundException)
            {
                context.Result = new NotFoundObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }));
            }
            else if (ex is UnauthorizedException)
            {
                context.Result = new UnauthorizedObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }));
            }
            else if (ex is ForBidenException)
            {
                context.Result = new ObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message })
                )
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            else if (ex is BadRequestException)
            {
                context.Result = new BadRequestObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }));
            }
            else if (ex is ConflictException)
            {
                context.Result = new ObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }))
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
            else if (ex is ValidationException)
            {
                context.Result = new BadRequestObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }));
            }
            else if (ex is NullDataException)
            {
                context.Result = new ObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }))
                {
                    StatusCode = StatusCodes.Status204NoContent // hoặc 404 nếu bạn thích
                };
            }
            else if (ex is InternalErrorException)
            {
                context.Result = new ObjectResult(
                    ResponseApi<ErrorDto>.Fail(new ErrorDto { Message = ex.Message }))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            else
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Result = new ObjectResult(
                    ResponseApi<SystemErrorDto>.Fail(new SystemErrorDto
                    {
                        Message = "Lỗi hệ thống",
                        Detail = ex.Message
                    }))
                {
                    StatusCode = 500
                };
            }

            context.ExceptionHandled = true;
        }
    }

}
