using BookApi.Controllers;
using BookApi.Models;
using BookApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookApi.Tests.Unit
{
    public class BooksControllerTests
    {
        private readonly Mock<ICommonRepository<Book>> _mockBookRepository;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookRepository = new Mock<ICommonRepository<Book>>();
            _controller = new BooksController(_mockBookRepository.Object);
        }

        // Helper method to create test books
        private List<Book> GetTestBooks()
        {
            return new List<Book>
            {
                new Book { Id = 1, Title = "Book 1", Author = "Author 1", Year = 2001 },
                new Book { Id = 2, Title = "Book 2", Author = "Author 2", Year = 2002 },
                new Book { Id = 3, Title = "Book 3", Author = "Author 3", Year = 2003 }
            };
        }

        private Book GetTestBook()
        {
            return new Book { Id = 1, Title = "Test Book", Author = "Test Author", Year = 2000 };
        }

        #region GetAllBooks Tests

        [Fact]
        public async Task GetAllBooks_WhenBooksExist_ReturnsOkResultWithBooks()
        {
            // Arrange
            var testBooks = GetTestBooks();
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(testBooks);

            // Act
            var result = await _controller.GetAllBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBooks = Assert.IsType<List<Book>>(okResult.Value);
            Assert.Equal(testBooks.Count, returnedBooks.Count);
            _mockBookRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllBooks_WhenNoBooksExist_ReturnsNotFound()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _controller.GetAllBooks();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No books found.", notFoundResult.Value);
            _mockBookRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllBooks_WhenRepositoryReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync((List<Book>)null);

            // Act
            var result = await _controller.GetAllBooks();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No books found.", notFoundResult.Value);
        }

        #endregion


        #region GetBookById Tests

        [Fact]
        public async Task GetBookById_WhenBookExists_ReturnsOkResultWithBook()
        {
            // Arrange
            var testBook = GetTestBook();
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(testBook);

            // Act
            var result = await _controller.GetBookById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBook = Assert.IsType<Book>(okResult.Value);
            Assert.Equal(testBook.Id, returnedBook.Id);
            Assert.Equal(testBook.Title, returnedBook.Title);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetBookById_WhenBookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.GetBookById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Book with ID 999 not found.", notFoundResult.Value);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetBookById_WithInvalidId_ReturnsNotFound(int invalidId)
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(invalidId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.GetBookById(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Book with ID {invalidId} not found.", notFoundResult.Value);
        }

        #endregion

        #region CreateBook Tests


        [Fact]
        public async Task CreateBook_WithValidBook_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newBook = new Book { Title = "New Book", Author = "New Author", Year = 2000 };
            var createdBook = new Book { Id = 4, Title = "New Book", Author = "New Author", Year = 2000 };

            _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _controller.CreateBook(newBook);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(BooksController.GetBookById), createdAtActionResult.ActionName);
            Assert.Equal(createdBook.Id, createdAtActionResult.RouteValues["id"]);
            var returnedBook = Assert.IsType<Book>(createdAtActionResult.Value);
            Assert.Equal(createdBook.Id, returnedBook.Id);
            _mockBookRepository.Verify(repo => repo.CreateAsync(newBook), Times.Once);
        }

        [Fact]
        public async Task CreateBook_WithNullBook_ThrowsException()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.CreateAsync(null))
                .ThrowsAsync(new ArgumentNullException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.CreateBook(null));
        }

        #endregion

        #region UpdateBook Tests

        [Fact]
        public async Task UpdateBook_WithValidIdAndBook_ReturnsOkResultWithUpdatedBook()
        {
            // Arrange
            var existingBook = GetTestBook();
            var updatedBook = new Book { Id = 1, Title = "Updated Book", Author = "Updated Author", Year = 2000 };

            _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<int>(), It.IsAny<Book>()))
                .ReturnsAsync(updatedBook);

            // Act
            var result = await _controller.UpdateBook(1, updatedBook);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBook = Assert.IsType<Book>(okResult.Value);
            Assert.Equal(updatedBook.Title, returnedBook.Title);
            Assert.Equal(updatedBook.Author, returnedBook.Author);
            _mockBookRepository.Verify(repo => repo.UpdateAsync(1, updatedBook), Times.Once);
        }

        [Fact]
        public async Task UpdateBook_WhenBookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var bookToUpdate = new Book { Id = 999, Title = "Non-existent Book", Author = "Author", Year = 2000 };

            _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<int>(), It.IsAny<Book>()))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.UpdateBook(999, bookToUpdate);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Book with ID 999 not found.", notFoundResult.Value);
            _mockBookRepository.Verify(repo => repo.UpdateAsync(999, bookToUpdate), Times.Once);
        }

        [Fact]
        public async Task UpdateBook_WithIdMismatch_ReturnsNotFound()
        {
            // Arrange
            var bookToUpdate = new Book { Id = 1, Title = "Book", Author = "Author", Year = 2000 };

            _mockBookRepository.Setup(repo => repo.UpdateAsync(2, bookToUpdate))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.UpdateBook(2, bookToUpdate);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Book with ID 2 not found.", notFoundResult.Value);
        }

        #endregion

        #region DeleteBook Tests

        [Fact]
        public async Task DeleteBook_WhenBookExists_ReturnsNoContentResult()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBook(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteBook_WhenBookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBook(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Book with ID 999 not found.", notFoundResult.Value);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteBook_WithInvalidId_ReturnsNotFound(int invalidId)
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.DeleteAsync(invalidId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBook(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Book with ID {invalidId} not found.", notFoundResult.Value);
        }

        #endregion
    }
}
