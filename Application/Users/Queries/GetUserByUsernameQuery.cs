using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Users.Queries;

public class GetUserByUsernameQuery : IRequest<ResponseObjectJsonDto>
{
    public string Username { get; set; }

    public GetUserByUsernameQuery(string username)
    {
        Username = username;
    }
}

public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            
            if (user == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "User not found",
                    Code = 404,
                    Response = null
                };
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return new ResponseObjectJsonDto
            {
                Message = "User retrieved successfully",
                Code = 200,
                Response = userDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception during user retrieval: " + ex.Message,
                Response = null
            };
        }
    }
}
