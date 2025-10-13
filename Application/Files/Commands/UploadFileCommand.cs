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

    public UploadFileCommand(IFormFile file, int userId)
    {
        File = file;
        UserId = userId;
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;

    public UploadFileCommandHandler(IFileRepository fileRepository, IFileStorageService fileStorageService)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<ResponseObjectJsonDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingFile = await _fileRepository.GetByNameAndUserIdAsync(request.File.FileName, request.UserId);
            
            if (existingFile != null)
            {
                await _fileStorageService.DeleteFileAsync(existingFile.StoragePath);

                using (var stream = request.File.OpenReadStream())
                {
                    var storagePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.UserId);
                    
                    existingFile.StoragePath = storagePath;
                    existingFile.SizeInBytes = request.File.Length;
                    existingFile.ContentType = request.File.ContentType;
                    existingFile.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

                    var updatedFile = await _fileRepository.UpdateAsync(existingFile);

                    var fileDto = new FileDto
                    {
                        Id = updatedFile.Id,
                        Name = updatedFile.Name,
                        SizeInBytes = updatedFile.SizeInBytes,
                        ContentType = updatedFile.ContentType,
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
                        UserId = request.UserId
                    };

                    var createdFile = await _fileRepository.CreateAsync(fileEntity);

                    var fileDto = new FileDto
                    {
                        Id = createdFile.Id,
                        Name = createdFile.Name,
                        SizeInBytes = createdFile.SizeInBytes,
                        ContentType = createdFile.ContentType,
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
