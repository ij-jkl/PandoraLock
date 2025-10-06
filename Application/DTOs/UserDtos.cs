namespace Application.DTOs;

public class CreateUserDto
{
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}

public class LoginUserDto
{
    public string UsernameOrEmail { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = default!;
    public UserDto User { get; set; } = default!;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = default!;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmNewPassword { get; set; } = default!;
}

public class ForgotPasswordResponseDto
{
    public string ResetLink { get; set; } = default!;
}
