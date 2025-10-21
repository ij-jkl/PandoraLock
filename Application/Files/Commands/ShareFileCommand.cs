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
    public int ExpirationHours { get; set; } = 0;
    public int? MaxDownloads { get; set; }

    public ShareFileCommand(int fileId, string sharedWithEmail, int ownerId, int expirationHours = 0, int? maxDownloads = null)
    {
        FileId = fileId;
        SharedWithEmail = sharedWithEmail;
        OwnerId = ownerId;
        ExpirationHours = expirationHours;
        MaxDownloads = maxDownloads;
    }
}

public class ShareFileCommandHandler : IRequestHandler<ShareFileCommand, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISharedFileAccessRepository _sharedFileAccessRepository;
    private readonly IDateTimeService _dateTimeService;

    public ShareFileCommandHandler(IFileRepository fileRepository,IUserRepository userRepository,
        ISharedFileAccessRepository sharedFileAccessRepository, IDateTimeService dateTimeService)
    {
        _fileRepository = fileRepository;
        _userRepository = userRepository;
        _sharedFileAccessRepository = sharedFileAccessRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task<ResponseObjectJsonDto> Handle(ShareFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ExpirationHours < 0 || request.ExpirationHours > 720)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Expiration hours must be between 0 (forever) and 720 (30 days)",
                    Code = 400,
                    Response = null
                };
            }

            if (request.MaxDownloads.HasValue && request.MaxDownloads.Value <= 0)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Max downloads must be greater than 0",
                    Code = 400,
                    Response = null
                };
            }

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
                SharedWithUserId = userToShareWith.Id,
                ExpiresAt = request.ExpirationHours > 0 ? _dateTimeService.Now.AddHours(request.ExpirationHours) : null,
                MaxDownloads = request.MaxDownloads
            };

            var created = await _sharedFileAccessRepository.CreateAsync(sharedAccess);

            var sharedAccessDto = new SharedFileAccessDto
            {
                Id = created.Id,
                FileId = created.FileId,
                SharedWithUserEmail = userToShareWith.Email,
                SharedAt = created.SharedAt,
                ExpiresAt = created.ExpiresAt,
                DownloadCount = created.DownloadCount,
                MaxDownloads = created.MaxDownloads
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
