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

    public UploadFileCommand(IFormFile file)
    {
        File = file;
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public UploadFileCommandHandler(IFileRepository fileRepository, IFileStorageService fileStorageService)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<ResponseObjectJsonDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "File is required",
                    Code = 400,
                    Response = null
                };
            }

            if (request.File.Length > MaxFileSizeInBytes)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "File size exceeds the maximum allowed size of 10MB",
                    Code = 400,
                    Response = null
                };
            }

            using (var stream = request.File.OpenReadStream())
            {
                if (!_fileStorageService.IsPdfFile(stream))
                {
                    return new ResponseObjectJsonDto
                    {
                        Message = "Only valid PDF files are allowed",
                        Code = 400,
                        Response = null
                    };
                }
            }

            var existingFile = await _fileRepository.GetByNameAsync(request.File.FileName);
            
            if (existingFile != null)
            {
                await _fileStorageService.DeleteFileAsync(existingFile.StoragePath);

                using (var stream = request.File.OpenReadStream())
                {
                    var storagePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName);
                    
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
                    var storagePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName);
                    
                    var fileEntity = new FileEntity
                    {
                        Name = request.File.FileName,
                        StoragePath = storagePath,
                        SizeInBytes = request.File.Length,
                        ContentType = request.File.ContentType
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
