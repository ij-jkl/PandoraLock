using Application.DTOs;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/create/user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        var response = await _mediator.Send(new CreateUserCommand(request));
        
        return StatusCode(response.Code, response);
    }

    [HttpPost("/login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserDto request)
    {
        var command = new LoginUserCommand(request.UsernameOrEmail, request.Password);
        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    [HttpPost("/forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var response = await _mediator.Send(command);

        return StatusCode(response.Code, response);
    }

    [HttpPost("/reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmNewPassword);
        var response = await _mediator.Send(command);

        return StatusCode(response.Code, response);
    }

    [HttpGet("/get_by_{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var query = new GetUserByIdQuery(id);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet("/get_by_username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var query = new GetUserByUsernameQuery(username);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    [HttpGet("/get_by_email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery(email);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }
}
