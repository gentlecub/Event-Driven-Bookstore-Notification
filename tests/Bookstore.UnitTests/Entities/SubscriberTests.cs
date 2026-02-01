using Bookstore.Core.Entities;
using FluentAssertions;
using Xunit;

namespace Bookstore.UnitTests.Entities;

public class SubscriberTests
{
    [Fact]
    public void Subscriber_WhenCreated_ShouldHaveGeneratedId()
    {
        // Arrange & Act
        var subscriber = new Subscriber
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Assert
        subscriber.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(subscriber.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Subscriber_PartitionKey_ShouldReturnId()
    {
        // Arrange
        var subscriber = new Subscriber
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var partitionKey = subscriber.PartitionKey;

        // Assert
        partitionKey.Should().Be(subscriber.Id);
    }

    [Fact]
    public void Subscriber_IsConfirmed_ShouldBeFalseByDefault()
    {
        // Arrange
        var subscriber = new Subscriber
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act & Assert
        subscriber.IsConfirmed.Should().BeFalse();
        subscriber.ConfirmedAt.Should().BeNull();
    }

    [Fact]
    public void Subscriber_IsConfirmed_ShouldBeTrueWhenConfirmedAtIsSet()
    {
        // Arrange
        var subscriber = new Subscriber
        {
            Email = "test@example.com",
            Name = "Test User",
            ConfirmedAt = DateTime.UtcNow
        };

        // Act & Assert
        subscriber.IsConfirmed.Should().BeTrue();
    }

    [Fact]
    public void Subscriber_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var subscriber = new Subscriber
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Assert
        subscriber.IsActive.Should().BeTrue();
        subscriber.SubscribedCategories.Should().BeEmpty();
        subscriber.NotificationPreference.Should().Be(NotificationPreference.Email);
        subscriber.NotificationCount.Should().Be(0);
    }
}
