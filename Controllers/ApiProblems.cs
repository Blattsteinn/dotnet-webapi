using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers;

// Uniform error shape for controller actions. The service layer already reports
// outcomes as result enums rather than throwing, so an action's error branch just
// forwards the message here instead of returning a bare string. The framework's
// ProblemDetailsFactory fills in `type`, `title` and the status code, so every
// failure — these, the automatic [ApiController] validation responses, and the
// ones UseExceptionHandler/UseStatusCodePages produce — serializes the same way.
public static class ApiProblems
{
    public static IActionResult NotFoundProblem(this ControllerBase controller, string detail) =>
        controller.Problem(statusCode: StatusCodes.Status404NotFound, detail: detail);

    public static IActionResult BadRequestProblem(this ControllerBase controller, string? detail) =>
        controller.Problem(statusCode: StatusCodes.Status400BadRequest, detail: detail);

    public static IActionResult ConflictProblem(this ControllerBase controller, string detail) =>
        controller.Problem(statusCode: StatusCodes.Status409Conflict, detail: detail);

    public static IActionResult UnauthorizedProblem(this ControllerBase controller, string detail) =>
        controller.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: detail);
}
