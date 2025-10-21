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
    private readonly IDateTimeService _dateTimeService;
    private readonly ICacheService _cacheService;

    public DownloadFileQueryHandler(
        IFileRepository fileRepository, 
        IFileStorageService fileStorageService,
        ISharedFileAccessRepository sharedFileAccessRepository,
        IDateTimeService dateTimeService,
        ICacheService cacheService)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _sharedFileAccessRepository = sharedFileAccessRepository;
        _dateTimeService = dateTimeService;
        _cacheService = cacheService;
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
            var sharedAccess = await _sharedFileAccessRepository.GetByFileIdAndUserIdAsync(request.FileId, request.UserId);
            var hasSharedAccess = sharedAccess != null;

            if (hasSharedAccess && sharedAccess!.ExpiresAt != null && _dateTimeService.Now > sharedAccess.ExpiresAt)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Access denied. Share link has expired",
                    Code = 403,
                    Response = null
                };
            }

            if (hasSharedAccess && sharedAccess!.MaxDownloads.HasValue && sharedAccess.DownloadCount >= sharedAccess.MaxDownloads.Value)
            {
                return new ResponseObjectJsonDto
                {
                    Message = $"Access denied. Download limit of {sharedAccess.MaxDownloads.Value} has been reached",
                    Code = 403,
                    Response = null
                };
            }

            if (!isOwner && !isPublic && !hasSharedAccess)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Access denied. You do not have permission to download this file",
                    Code = 403,
                    Response = null
                };
            }

            Stream? fileStream = null;
            byte[]? fileBytes = null;

            if (isPublic)
            {
                var cacheKey = $"file:{request.FileId}";
                var cachedFile = await _cacheService.GetAsync<CachedFileDto>(cacheKey, cancellationToken);

                if (cachedFile != null)
                {
                    fileStream = new MemoryStream(cachedFile.FileContent);
                }
                else
                {
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

                    using (var memoryStream = new MemoryStream())
                    {
                        await fileData.Value.fileStream.CopyToAsync(memoryStream, cancellationToken);
                        fileBytes = memoryStream.ToArray();
                    }

                    var cachedFileDto = new CachedFileDto
                    {
                        FileContent = fileBytes,
                        ContentType = file.ContentType,
                        FileName = file.Name
                    };

                    await _cacheService.SetAsync(cacheKey, cachedFileDto, null, cancellationToken);

                    fileStream = new MemoryStream(fileBytes);
                }
            }
            else
            {
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

                fileStream = fileData.Value.fileStream;
            }

            if (hasSharedAccess && sharedAccess != null)
            {
                sharedAccess.DownloadCount++;
                await _sharedFileAccessRepository.UpdateAsync(sharedAccess);
            }

            var downloadDto = new FileDownloadDto
            {
                FileStream = fileStream,
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
