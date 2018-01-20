﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<CreateAuthorDto> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }

            var authors = Mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var author in authors)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author collection failed on save.");
            }

            var createdAuthors = Mapper.Map<IEnumerable<AuthorDto>>(authors);
            var createdIds = string.Join(",", createdAuthors.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection", new {ids = createdIds}, createdAuthors);
        }

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authors = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != authors.Count())
            {
                return NotFound();
            }

            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorDtos);
        }
    }
}