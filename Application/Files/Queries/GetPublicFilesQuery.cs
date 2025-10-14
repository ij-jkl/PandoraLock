using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Files.Queries;

public class GetPublicFilesQuery : IRequest<ResponseObjectJsonDto>
{
}

public class GetPublicFilesQueryHandler : IRequestHandler<GetPublicFilesQuery, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;

    public GetPublicFilesQueryHandler(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetPublicFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var publicFiles = await _fileRepository.GetAllPublicFilesAsync();

            var fileDtos = publicFiles.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                SizeInBytes = f.SizeInBytes,
                ContentType = f.ContentType,
                IsPublic = f.IsPublic,
                UploadedAt = f.UploadedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList();

            return new ResponseObjectJsonDto
            {
                Message = "List of public files",
                Code = 200,
                Response = fileDtos
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while retrieving public files",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
