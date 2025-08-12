
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController] 
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropertyMappingService _propertyMappingService;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper,
        IPropertyMappingService propertyMappingService)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService;
    }


    private string? CreateAuthorsResourceUri(
        AuthorResourceParams authorParams,
        ResourceUriType type)
    {
        if (authorParams == null)
        {
            throw new ArgumentNullException(nameof(authorParams));
        }
        return type switch
        {
            ResourceUriType.PreviousPage => Url.Link("GetAuthors",
                new
                {
                    fields = authorParams.Fields,
                    orderBy = authorParams.OrderBy,
                    pageNumber = authorParams.PageNumber - 1,
                    pageSize = authorParams.PageSize,
                    mainCategory = authorParams.MainCategory,
                    searchQuery = authorParams.SearchQuery
                }),
            ResourceUriType.NextPage => Url.Link("GetAuthors",
                new
                {
                    fields = authorParams.Fields,
                    orderBy = authorParams.OrderBy,
                    pageNumber = authorParams.PageNumber + 1,
                    pageSize = authorParams.PageSize,
                    mainCategory = authorParams.MainCategory,
                    searchQuery = authorParams.SearchQuery
                }),
            _ => Url.Link("GetAuthors",
                new
                {
                    fields = authorParams.Fields,
                    orderBy = authorParams.OrderBy,
                    pageNumber = authorParams.PageNumber,
                    pageSize = authorParams.PageSize,
                    mainCategory = authorParams.MainCategory,
                    searchQuery = authorParams.SearchQuery
                })
        };
    }


    [HttpGet(Name = "GetAuthors")] 
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorResourceParams authorParams)
    {


        if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>(authorParams.OrderBy))
        {
            return BadRequest();
        }



        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorParams);

        var previousPageLink = authorsFromRepo.HasPrevious
            ? CreateAuthorsResourceUri(authorParams, ResourceUriType.PreviousPage)
            : null;

        var nextPageLink = authorsFromRepo.HasNext
            ? CreateAuthorsResourceUri(authorParams, ResourceUriType.NextPage)
            : null;

        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.PageNumber,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink,
            nextPageLink
        };

        Response.Headers.Append("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));


        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
            .ShapeData(authorParams.Fields));
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }
}
