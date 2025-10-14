using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Files.Queries;

public class GetSharedWithMeFilesQuery : IRequest<ResponseObjectJsonDto>
{
    public int UserId { get; set; }

    public GetSharedWithMeFilesQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetSharedWithMeFilesQueryHandler : IRequestHandler<GetSharedWithMeFilesQuery, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;

    public GetSharedWithMeFilesQueryHandler(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetSharedWithMeFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sharedFiles = await _fileRepository.GetFilesSharedWithUserAsync(request.UserId);

            var fileDtos = sharedFiles.Select(f => new FileDto
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
                Message = "List of files shared with you",
                Code = 200,
                Response = fileDtos
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while retrieving shared files",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
