﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = _libraryRepository.GetBooksForAuthor(authorId);

            return Ok(Mapper.Map<IEnumerable<BookDto>>(books));
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<BookDto>(book));
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] CreateBookDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for {authorId} failed on save.");
            }

            var createdBook = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new {id = createdBook.Id}, createdBook);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(book);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Delete book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] UpdateBookDto model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
            {
                return NotFound();
            }

            Mapper.Map(model, book);

            _libraryRepository.UpdateBookForAuthor(book);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Update book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }
    }
}