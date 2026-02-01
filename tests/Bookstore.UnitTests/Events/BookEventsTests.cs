using Bookstore.Core.Entities;
using Bookstore.Core.Events;
using FluentAssertions;
using Xunit;

namespace Bookstore.UnitTests.Events;

public class BookEventsTests
{
    [Fact]
    public void BookCreatedEvent_ShouldHaveCorrectType()
    {
        // Arrange
        var @event = new BookCreatedEvent();

        // Act & Assert
        @event.Type.Should().Be("com.bookstore.book.created");
        @event.SpecVersion.Should().Be("1.0");
    }

    [Fact]
    public void BookCreatedEventData_FromBook_ShouldMapAllFields()
    {
        // Arrange
        var book = new Book
        {
            Id = "book-123",
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Isbn = "978-0-134-49416-6",
            Category = "Software Engineering",
            Price = 34.99m,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var eventData = BookCreatedEventData.FromBook(book);

        // Assert
        eventData.BookId.Should().Be("book-123");
        eventData.Title.Should().Be("Clean Architecture");
        eventData.Author.Should().Be("Robert C. Martin");
        eventData.Isbn.Should().Be("978-0-134-49416-6");
        eventData.Category.Should().Be("Software Engineering");
        eventData.Price.Should().Be(34.99m);
        eventData.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void BookUpdatedEvent_ShouldHaveCorrectType()
    {
        // Arrange
        var @event = new BookUpdatedEvent();

        // Act & Assert
        @event.Type.Should().Be("com.bookstore.book.updated");
    }

    [Fact]
    public void BookDeletedEvent_ShouldHaveCorrectType()
    {
        // Arrange
        var @event = new BookDeletedEvent();

        // Act & Assert
        @event.Type.Should().Be("com.bookstore.book.deleted");
    }

    [Fact]
    public void BookEventBase_ShouldGenerateUniqueIds()
    {
        // Arrange
        var event1 = new BookCreatedEvent();
        var event2 = new BookCreatedEvent();

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
