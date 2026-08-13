using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Assignments;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/assignments")]
public sealed class AssignmentsController(IAssignmentService assignmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var result = await assignmentService.SearchAsync(
            new AssignmentQuery(page, pageSize, search, isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.GetByIdAsync(id, cancellationToken);

        return assignment is null ? NotFound() : Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.DeactivateAsync(id, cancellationToken);

        return assignment is null ? NotFound() : Ok(assignment);
    }
}

