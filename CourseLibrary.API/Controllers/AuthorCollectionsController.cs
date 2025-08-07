using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace CourseLibrary.API.Controllers
{

    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;


        public AuthorCollectionsController(
            ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }




        [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            [FromRoute] IEnumerable<Guid> authorIds)
        {
            if (authorIds == null || !authorIds.Any())
            {
                return BadRequest();
            }
            var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);
            if (authorEntities.Count() != authorIds.Count())
            {
                return NotFound();
            }
            var authorDtosToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorDtosToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection(
            IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null || !authorCollection.Any())
            {
                return BadRequest();
            }

            var authorEntities = _mapper.Map<IEnumerable<Entities.Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            await _courseLibraryRepository.SaveAsync();

            var authorDtosToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            var authorIds = string.Join(",",
                authorDtosToReturn.Select(a => a.Id));

            // return the created authors with a 201 Created response
            return CreatedAtRoute("GetAuthorCollection", new {authorIds = authorIds }, authorDtosToReturn);
        }





    }
}
