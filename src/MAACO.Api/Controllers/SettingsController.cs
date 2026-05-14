using FluentValidation;
using FluentValidation.AspNetCore;
using MAACO.Api.Contracts.Settings;
using MAACO.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController(
    ISettingsService settingsService,
    IValidator<UpdateSettingsRequest> updateSettingsRequestValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut]
    [ProducesResponseType(typeof(SettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SettingsDto>> UpdateSettings(
        [FromBody] UpdateSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await updateSettingsRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var settings = await settingsService.UpdateAsync(request, cancellationToken);
        return Ok(settings);
    }
}
