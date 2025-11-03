using Application.Files.Commands;
using Application.Files.Queries;
using Application.DTOs;
using Domain.Constants;
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

    /// <summary>
    /// Uploads a new file to the system.
    /// </summary>
    /// <response code="201">File uploaded successfully.</response>
    /// <response code="400">Invalid file or validation failed.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions to upload files.</response>
    [HttpPost("upload")]
    [Authorize(Policy = Permissions.Files.Create)]
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

    /// <summary>
    /// Shares a file with another user via email.
    /// </summary>
    /// <response code="200">File shared successfully.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions to share files.</response>
    /// <response code="404">File not found or user cannot share this file.</response>
    [HttpPost("{fileId}/share")]
    [Authorize(Policy = Permissions.Files.Update)]
    public async Task<IActionResult> ShareFile(int fileId, [FromBody] ShareFileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { Message = "User not authenticated", Code = 401 });
        }

        var command = new ShareFileCommand(fileId, request.SharedWithEmail, userId, request.ExpirationHours ?? 0, request.MaxDownloads);
        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Retrieves all files owned by the authenticated user.
    /// </summary>
    /// <response code="200">Returns the list of user's files.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions to read files.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Files.Read)]
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

    /// <summary>
    /// Retrieves all publicly accessible files.
    /// </summary>
    /// <response code="200">Returns the list of public files.</response>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicFiles()
    {
        var query = new GetPublicFilesQuery();
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Retrieves all files that have been shared with the authenticated user.
    /// </summary>
    /// <response code="200">Returns the list of shared files.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions to read files.</response>
    [HttpGet("shared-with-me")]
    [Authorize(Policy = Permissions.Files.Read)]
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

    /// <summary>
    /// Downloads a file by its identifier.
    /// </summary>
    /// <response code="200">Returns the file content.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions to download this file.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("{id}/download")]
    [Authorize(Policy = Permissions.Files.Read)]
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
    public int? ExpirationHours { get; set; }
    public int? MaxDownloads { get; set; }
}
