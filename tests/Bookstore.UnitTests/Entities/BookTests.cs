using Bookstore.Core.Entities;
using FluentAssertions;
using Xunit;

namespace Bookstore.UnitTests.Entities;

public class BookTests
{
    [Fact]
    public void Book_WhenCreated_ShouldHaveGeneratedId()
    {
        // Arrange & Act
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "978-0-123456-78-9",
            Category = "Technology"
        };

        // Assert
        book.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(book.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Book_PartitionKey_ShouldReturnCategory()
    {
        // Arrange
        var book = new Book
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "978-0-132350-88-4",
            Category = "Software Engineering"
        };

        // Act
        var partitionKey = book.PartitionKey;

        // Assert
        partitionKey.Should().Be("Software Engineering");
    }

    [Fact]
    public void Book_Type_ShouldReturnBookClassName()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test",
            Author = "Author",
            Isbn = "123",
            Category = "Test"
        };

        // Act
        var type = book.Type;

        // Assert
        type.Should().Be("Book");
    }

    [Fact]
    public void Book_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var book = new Book
        {
            Title = "Test",
            Author = "Author",
            Isbn = "123",
            Category = "Test"
        };

        // Assert
        book.IsAvailable.Should().BeTrue();
        book.StockQuantity.Should().Be(0);
        book.Tags.Should().BeEmpty();
        book.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        book.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
