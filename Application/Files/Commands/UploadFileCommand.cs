using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Files.Commands;

public class UploadFileCommand : IRequest<ResponseObjectJsonDto>
{
    public IFormFile File { get; set; } = default!;
    public int UserId { get; set; }
    public bool IsPublic { get; set; } = false;

    public UploadFileCommand(IFormFile file, int userId, bool isPublic = false)
    {
        File = file;
        UserId = userId;
        IsPublic = isPublic;
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cacheService;
    private readonly IDateTimeService _dateTimeService;

    public UploadFileCommandHandler(
        IFileRepository fileRepository, 
        IFileStorageService fileStorageService,
        ICacheService cacheService,
        IDateTimeService dateTimeService)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _cacheService = cacheService;
        _dateTimeService = dateTimeService;
    }

    public async Task<ResponseObjectJsonDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingFile = await _fileRepository.GetByNameAndUserIdAsync(request.File.FileName, request.UserId);
            
            if (existingFile != null)
            {
                await _fileStorageService.DeleteFileAsync(existingFile.StoragePath);

                if (existingFile.IsPublic)
                {
                    var cacheKey = $"file:{existingFile.Id}";
                    await _cacheService.RemoveAsync(cacheKey, cancellationToken);
                }

                using (var stream = request.File.OpenReadStream())
                {
                    var storagePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.UserId);
                    
                    existingFile.StoragePath = storagePath;
                    existingFile.SizeInBytes = request.File.Length;
                    existingFile.ContentType = request.File.ContentType;
                    existingFile.IsPublic = request.IsPublic;
                    existingFile.UpdatedAt = _dateTimeService.Now;

                    var updatedFile = await _fileRepository.UpdateAsync(existingFile);

                    var fileDto = new FileDto
                    {
                        Id = updatedFile.Id,
                        Name = updatedFile.Name,
                        SizeInBytes = updatedFile.SizeInBytes,
                        ContentType = updatedFile.ContentType,
                        IsPublic = updatedFile.IsPublic,
                        UploadedAt = updatedFile.UploadedAt,
                        UpdatedAt = updatedFile.UpdatedAt
                    };

                    return new ResponseObjectJsonDto
                    {
                        Message = "File updated successfully",
                        Code = 200,
                        Response = fileDto
                    };
                }
            }
            else
            {
                using (var stream = request.File.OpenReadStream())
                {
                    var storagePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.UserId);
                    
                    var fileEntity = new FileEntity
                    {
                        Name = request.File.FileName,
                        StoragePath = storagePath,
                        SizeInBytes = request.File.Length,
                        ContentType = request.File.ContentType,
                        UserId = request.UserId,
                        IsPublic = request.IsPublic
                    };

                    var createdFile = await _fileRepository.CreateAsync(fileEntity);

                    var fileDto = new FileDto
                    {
                        Id = createdFile.Id,
                        Name = createdFile.Name,
                        SizeInBytes = createdFile.SizeInBytes,
                        ContentType = createdFile.ContentType,
                        IsPublic = createdFile.IsPublic,
                        UploadedAt = createdFile.UploadedAt,
                        UpdatedAt = createdFile.UpdatedAt
                    };

                    return new ResponseObjectJsonDto
                    {
                        Message = "File uploaded successfully",
                        Code = 201,
                        Response = fileDto
                    };
                }
            }
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while processing the file",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
