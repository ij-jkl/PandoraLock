using Application.Files.Commands;
using Application.Files.Queries;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] bool isPublic = false)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var command = new UploadFileCommand(file, userId, isPublic);
        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    [HttpPost("{fileId}/share")]
    [Authorize]
    public async Task<IActionResult> ShareFile(int fileId, [FromBody] ShareFileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var command = new ShareFileCommand(fileId, request.SharedWithEmail, userId);
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

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicFiles()
    {
        var query = new GetPublicFilesQuery();
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet("shared-with-me")]
    [Authorize]
    public async Task<IActionResult> GetSharedWithMeFiles()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var query = new GetSharedWithMeFilesQuery(userId);
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet("{id}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var query = new DownloadFileQuery(id, userId);
        var response = await _mediator.Send(query);

        if (response.Code != 200)
        {
            return StatusCode(response.Code, response);
        }

        var fileData = response.Response as FileDownloadDto;

        if (fileData == null)
        {
            return StatusCode(500, new { Message = "Error retrieving file data", Code = 500 });
        }

        return File(fileData.FileStream, fileData.ContentType, fileData.FileName);
    }
}

public class ShareFileRequest
{
    public string SharedWithEmail { get; set; } = default!;
}
