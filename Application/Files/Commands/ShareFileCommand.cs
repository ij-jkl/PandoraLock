using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Files.Commands;

public class ShareFileCommand : IRequest<ResponseObjectJsonDto>
{
    public int FileId { get; set; }
    public string SharedWithEmail { get; set; } = default!;
    public int OwnerId { get; set; }

    public ShareFileCommand(int fileId, string sharedWithEmail, int ownerId)
    {
        FileId = fileId;
        SharedWithEmail = sharedWithEmail;
        OwnerId = ownerId;
    }
}

public class ShareFileCommandHandler : IRequestHandler<ShareFileCommand, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISharedFileAccessRepository _sharedFileAccessRepository;

    public ShareFileCommandHandler(IFileRepository fileRepository,IUserRepository userRepository,
        ISharedFileAccessRepository sharedFileAccessRepository)
    {
        _fileRepository = fileRepository;
        _userRepository = userRepository;
        _sharedFileAccessRepository = sharedFileAccessRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(ShareFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(request.FileId);

            if (file == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "File not found",
                    Code = 404,
                    Response = null
                };
            }

            if (file.UserId != request.OwnerId)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "You do not have permission to share this file",
                    Code = 403,
                    Response = null
                };
            }

            if (file.IsPublic)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Cannot share a public file. Public files are accessible to all users",
                    Code = 400,
                    Response = null
                };
            }

            var userToShareWith = await _userRepository.GetByEmailAsync(request.SharedWithEmail);

            if (userToShareWith == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "User with specified email not found",
                    Code = 404,
                    Response = null
                };
            }

            if (userToShareWith.Id == request.OwnerId)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "You cannot share a file with yourself",
                    Code = 400,
                    Response = null
                };
            }

            var existingAccess = await _sharedFileAccessRepository.GetByFileIdAndUserIdAsync(request.FileId, userToShareWith.Id);

            if (existingAccess != null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "File is already shared with this user",
                    Code = 400,
                    Response = null
                };
            }

            var sharedAccess = new SharedFileAccessEntity
            {
                FileId = request.FileId,
                SharedWithUserId = userToShareWith.Id
            };

            var created = await _sharedFileAccessRepository.CreateAsync(sharedAccess);

            var sharedAccessDto = new SharedFileAccessDto
            {
                Id = created.Id,
                FileId = created.FileId,
                SharedWithUserEmail = userToShareWith.Email,
                SharedAt = created.SharedAt
            };

            return new ResponseObjectJsonDto
            {
                Message = "File shared successfully",
                Code = 200,
                Response = sharedAccessDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while sharing the file",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
