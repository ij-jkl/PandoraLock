using Application.DTOs;
using Application.Users.Commands;
using Application.Users.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    /// <summary>
    /// Creates a new user account in the system.
    /// </summary>
    /// <response code="201">User account created successfully.</response>
    /// <response code="400">Invalid input data or user already exists.</response>
    /// <response code="403">Insufficient permissions to create users.</response>
    [HttpPost("/create/user")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        var response = await _mediator.Send(new CreateUserCommand(request));
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <response code="200">Login successful, returns access token.</response>
    /// <response code="400">Invalid credentials or input data.</response>
    /// <response code="401">Authentication failed.</response>
    [HttpPost("/login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserDto request)
    {
        var command = new LoginUserCommand(request.UsernameOrEmail, request.Password);
        var response = await _mediator.Send(command);
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Initiates a password reset process by sending a reset token to the user's email.
    /// </summary>
    /// <response code="200">Password reset email sent successfully.</response>
    /// <response code="400">Invalid email address.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("/forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var response = await _mediator.Send(command);

        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid token, expired token, or password validation failed.</response>
    [HttpPost("/reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmNewPassword);
        var response = await _mediator.Send(command);

        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <response code="200">Returns the requested user.</response>
    /// <response code="403">Insufficient permissions to read user data.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("/get_by_{id:int}")]
    [Authorize(Policy = Permissions.Users.Read)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var query = new GetUserByIdQuery(id);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <response code="200">Returns the requested user.</response>
    /// <response code="403">Insufficient permissions to read user data.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("/get_by_username/{username}")]
    [Authorize(Policy = Permissions.Users.Read)]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var query = new GetUserByUsernameQuery(username);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <response code="200">Returns the requested user.</response>
    /// <response code="403">Insufficient permissions to read user data.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("/get_by_email/{email}")]
    [Authorize(Policy = Permissions.Users.Read)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery(email);
        
        var response = await _mediator.Send(query);
        
        return StatusCode(response.Code, response);
    }
}
