using Application.Files.Commands;
using Application.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("upload_pdf")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var command = new UploadFileCommand(file);

        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet("get_all_files")]
    public async Task<IActionResult> GetAllFiles()
    {
        var query = new GetAllFilesQuery();

        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }
}
