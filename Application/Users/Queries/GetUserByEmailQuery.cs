using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Users.Queries;

public class GetUserByEmailQuery : IRequest<ResponseObjectJsonDto>
{
    public string Email { get; set; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
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
