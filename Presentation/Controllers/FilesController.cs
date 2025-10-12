using Application.Files.Commands;
using Application.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var command = new UploadFileCommand(file, userId);
        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFiles()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var query = new GetAllFilesQuery(userId);
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }
}
