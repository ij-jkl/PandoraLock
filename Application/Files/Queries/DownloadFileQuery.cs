using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Files.Queries;

public class DownloadFileQuery : IRequest<ResponseObjectJsonDto>
{
    public int FileId { get; set; }
    public int UserId { get; set; }

    public DownloadFileQuery(int fileId, int userId)
    {
        FileId = fileId;
        UserId = userId;
    }
}

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISharedFileAccessRepository _sharedFileAccessRepository;

    public DownloadFileQueryHandler(
        IFileRepository fileRepository, 
        IFileStorageService fileStorageService,
        ISharedFileAccessRepository sharedFileAccessRepository)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _sharedFileAccessRepository = sharedFileAccessRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
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

            var isOwner = file.UserId == request.UserId;
            var isPublic = file.IsPublic;
            var hasSharedAccess = await _sharedFileAccessRepository.GetByFileIdAndUserIdAsync(request.FileId, request.UserId) != null;

            if (!isOwner && !isPublic && !hasSharedAccess)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Access denied. You do not have permission to download this file",
                    Code = 403,
                    Response = null
                };
            }

            var fileData = await _fileStorageService.GetFileAsync(file.StoragePath);

            if (fileData == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "File content not found in storage",
                    Code = 404,
                    Response = null
                };
            }

            var downloadDto = new FileDownloadDto
            {
                FileStream = fileData.Value.fileStream,
                ContentType = file.ContentType,
                FileName = file.Name
            };

            return new ResponseObjectJsonDto
            {
                Message = "File retrieved successfully",
                Code = 200,
                Response = downloadDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while downloading the file",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
