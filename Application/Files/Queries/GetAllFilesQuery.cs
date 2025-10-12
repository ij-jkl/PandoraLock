using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Files.Queries;

public class GetAllFilesQuery : IRequest<ResponseObjectJsonDto>
{
    public int UserId { get; set; }

    public GetAllFilesQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAllFilesQueryHandler : IRequestHandler<GetAllFilesQuery, ResponseObjectJsonDto>
{
    private readonly IFileRepository _fileRepository;

    public GetAllFilesQueryHandler(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetAllFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var files = await _fileRepository.GetAllByUserIdAsync(request.UserId);

            var fileDtos = files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                SizeInBytes = f.SizeInBytes,
                ContentType = f.ContentType,
                UploadedAt = f.UploadedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList();

            return new ResponseObjectJsonDto
            {
                Message = "List of files : ",
                Code = 200,
                Response = fileDtos
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Message = "An error occurred while retrieving files",
                Code = 500,
                Response = ex.Message
            };
        }
    }
}
