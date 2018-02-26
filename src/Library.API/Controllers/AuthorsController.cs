using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authors = _libraryRepository.GetAuthors(authorsResourceParameters);

            var authorsDtos = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var metadata = new
                {
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                var links = CreateLinksForAuthors(authorsResourceParameters, authors.HasNext, authors.HasPrevious);

                var shapedData = authorsDtos.ShapeData(authorsResourceParameters.Fields);

                var shapedDataWithLinks = shapedData.Select(author =>
                {
                    var dictionary = author as IDictionary<string, object>;

                    var authorsLinks = CreateLinksForAuthor((Guid) dictionary["Id"], authorsResourceParameters.Fields);

                    dictionary.Add("links", authorsLinks);

                    return dictionary;
                });

                var objectToReturn = new
                {
                    value = shapedDataWithLinks,
                    links
                };

                return Ok(objectToReturn);

            }
            else
            {
                var previousPage = authors.HasPrevious
                    ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
                    : null;

                var nextPage = authors.HasNext
                    ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
                    : null;

                var metadata = new
                {
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages,
                    previousPageLink = previousPage,
                    nextPageLink = nextPage
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                return Ok(authorsDtos.ShapeData(authorsResourceParameters.Fields));
            }
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = parameters.PageNumber - 1,
                        pageSize = parameters.PageSize,
                        genre = parameters.Genre,
                        searchQuery = parameters.SearchQuery,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        genre = parameters.Genre,
                        searchQuery = parameters.SearchQuery,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    });
                case ResourceUriType.CurrentPage:
                default:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        pageNumber = parameters.PageNumber,
                        pageSize = parameters.PageSize,
                        genre = parameters.Genre,
                        searchQuery = parameters.SearchQuery,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var author = _libraryRepository.GetAuthor(id);

            if (author == null)
            {
                return NotFound();
            }

            var authorDto = Mapper.Map<AuthorDto>(author);

            var links = CreateLinksForAuthor(id, fields);

            var resourceToReturn = authorDto.ShapeData(fields) as IDictionary<string, object>;

            resourceToReturn.Add("links", links);

            return Ok(resourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        [RequestHeaderMatchesMediaType("Content-Type", new []{"application/vnd.marvin.author.full+json"})]
        public IActionResult CreateAuthor([FromBody] CreateAuthorDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var createdAutor = Mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(createdAutor.Id, null);

            var resourceToReturn = createdAutor.ShapeData(null) as IDictionary<string, object>;

            resourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { id = resourceToReturn["Id"] }, resourceToReturn);
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-Type", new[]
        {
            "application/vnd.marvin.authorwithdateofdeath.full+json",
            "application/vnd.marvin.authorwithdateofdeath.full+xml"
        })]
        //[RequestHeaderMatchesMediaType("Accept", new[]
        //{
        //    "application/vnd.marvin.authorwithdateofdeath.full+json"
        //})]
        public IActionResult CreateAuthorWithDateOfDeath([FromBody] CreateAuthorWithDateOfDeathDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var createdAutor = Mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(createdAutor.Id, null);

            var resourceToReturn = createdAutor.ShapeData(null) as IDictionary<string, object>;

            resourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { id = resourceToReturn["Id"] }, resourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteAuthor")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _libraryRepository.GetAuthor(id);

            if (author == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Delete author {id} failed on save.");
            }

            return NoContent();
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(_urlHelper.Link("GetAuthor", new {id}), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(_urlHelper.Link("GetAuthor", new { id, fields }), "self", "GET"));
            }

            links.Add(new LinkDto(_urlHelper.Link("DeleteAuthor", new { id }), "delete_author", "DELETE"));
            links.Add(new LinkDto(_urlHelper.Link("CreateBookForAuthor", new { authorId = id }), "create_book_for_author", "POST"));
            links.Add(new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { authorId = id }), "books", "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters parameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                new LinkDto(CreateAuthorsResourceUri(parameters, ResourceUriType.CurrentPage), "self", "GET")
            };


            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }
    }
}