using System.Net;
using Microsoft.AspNetCore.Mvc;
using RetailSales.Application.Common;

namespace RetailSales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess) return Ok();

        return MapErrorToResponse(result.Error);
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);

        return MapErrorToResponse(result.Error);
    }

    private ActionResult MapErrorToResponse(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => BadRequest(CreateProblemDetails("Validation Error", (int)HttpStatusCode.BadRequest, error)),
            ErrorType.NotFound => NotFound(CreateProblemDetails("Not Found", (int)HttpStatusCode.NotFound, error)),
            ErrorType.Conflict => Conflict(CreateProblemDetails("Conflict", (int)HttpStatusCode.Conflict, error)),
            _ => BadRequest(CreateProblemDetails("Bad Request", (int)HttpStatusCode.BadRequest, error))
        };
    }

    private ProblemDetails CreateProblemDetails(string title, int status, Error error) =>
        new()
        {
            Title = title,
            Status = status,
            Detail = error.Message,
            Extensions = { { "code", error.Code } }
        };
}
