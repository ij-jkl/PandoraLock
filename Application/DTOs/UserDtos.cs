namespace Application.DTOs;

public class CreateUserDto
{
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
