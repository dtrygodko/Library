using System;
using System.Collections.Generic;
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
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
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

            var authorsDtos = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorsDtos.ShapeData(authorsResourceParameters.Fields));
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

            return Ok(Mapper.Map<AuthorDto>(author).ShapeData(fields));
        }

        [HttpPost]
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

            return CreatedAtRoute("GetAuthor", new { id = createdAutor.Id }, createdAutor);
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

        [HttpDelete("{id}")]
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
    }
}