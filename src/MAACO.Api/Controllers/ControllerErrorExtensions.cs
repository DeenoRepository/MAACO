using MAACO.Api.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

public static class ControllerErrorExtensions
{
    public static ActionResult NotFoundError(this ControllerBase controller, string message) =>
        controller.NotFound(new ApiError("not_found", message, null, controller.HttpContext.TraceIdentifier));

    public static ActionResult ValidationError(this ControllerBase controller) =>
        controller.BadRequest(new ApiError(
            "validation_error",
            "Validation failed.",
            controller.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).ToArray()),
            controller.HttpContext.TraceIdentifier));
}
