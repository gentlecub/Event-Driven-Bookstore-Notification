using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using System.Collections.Concurrent;

namespace Bookstore.Api.Demo.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<string, Book> _books = new();

    public InMemoryBookRepository()
    {
        // Seed with sample data
        SeedData();
    }

    private void SeedData()
    {
        var sampleBooks = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "978-0132350884",
                Category = "Technology",
                Description = "A Handbook of Agile Software Craftsmanship",
                Price = 39.99m,
                PublishedDate = new DateTime(2008, 8, 1),
                Publisher = "Prentice Hall",
                PageCount = 464,
                StockQuantity = 50,
                IsAvailable = true,
                Tags = ["programming", "best-practices", "software-engineering"]
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "The Pragmatic Programmer",
                Author = "David Thomas, Andrew Hunt",
                Isbn = "978-0135957059",
                Category = "Technology",
                Description = "Your Journey to Mastery, 20th Anniversary Edition",
                Price = 49.99m,
                PublishedDate = new DateTime(2019, 9, 13),
                Publisher = "Addison-Wesley",
                PageCount = 352,
                StockQuantity = 35,
                IsAvailable = true,
                Tags = ["programming", "career", "software-engineering"]
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Designing Data-Intensive Applications",
                Author = "Martin Kleppmann",
                Isbn = "978-1449373320",
                Category = "Technology",
                Description = "The Big Ideas Behind Reliable, Scalable, and Maintainable Systems",
                Price = 54.99m,
                PublishedDate = new DateTime(2017, 3, 16),
                Publisher = "O'Reilly Media",
                PageCount = 616,
                StockQuantity = 28,
                IsAvailable = true,
                Tags = ["distributed-systems", "databases", "architecture"]
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "1984",
                Author = "George Orwell",
                Isbn = "978-0451524935",
                Category = "Fiction",
                Description = "A dystopian social science fiction novel",
                Price = 15.99m,
                PublishedDate = new DateTime(1949, 6, 8),
                Publisher = "Signet Classic",
                PageCount = 328,
                StockQuantity = 100,
                IsAvailable = true,
                Tags = ["dystopia", "classic", "political"]
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Sapiens: A Brief History of Humankind",
                Author = "Yuval Noah Harari",
                Isbn = "978-0062316097",
                Category = "Non-Fiction",
                Description = "A narrative of humanity's creation and evolution",
                Price = 24.99m,
                PublishedDate = new DateTime(2015, 2, 10),
                Publisher = "Harper",
                PageCount = 464,
                StockQuantity = 75,
                IsAvailable = true,
                Tags = ["history", "anthropology", "science"]
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Azure for Architects",
                Author = "Ritesh Modi",
                Isbn = "978-1839215865",
                Category = "Technology",
                Description = "Design and implement solutions on Microsoft Azure",
                Price = 44.99m,
                PublishedDate = new DateTime(2020, 9, 4),
                Publisher = "Packt Publishing",
                PageCount = 442,
                StockQuantity = 20,
                IsAvailable = true,
                Tags = ["azure", "cloud", "architecture"]
            }
        };

        foreach (var book in sampleBooks)
        {
            _books.TryAdd(book.Id, book);
        }
    }

    public Task<Book?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _books.TryGetValue(id, out var book);
        return Task.FromResult(book);
    }

    public Task<IEnumerable<Book>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        var books = _books.Values.Where(b => b.Category == partitionKey);
        return Task.FromResult(books);
    }

    public Task<Book> CreateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _books.TryAdd(entity.Id, entity);
        return Task.FromResult(entity);
    }

    public Task<Book> UpdateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _books[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _books.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_books.ContainsKey(id));
    }

    public Task<IEnumerable<Book>> QueryAsync(Func<IQueryable<Book>, IQueryable<Book>> predicate, CancellationToken cancellationToken = default)
    {
        var result = predicate(_books.Values.AsQueryable()).AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<IEnumerable<Book>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var books = _books.Values.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(books);
    }

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var book = _books.Values.FirstOrDefault(b => b.Isbn == isbn);
        return Task.FromResult(book);
    }

    public Task<IEnumerable<Book>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();
        var books = _books.Values.Where(b =>
            b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (b.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        return Task.FromResult(books);
    }

    public Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default)
    {
        var books = _books.Values.Where(b => b.IsAvailable && b.StockQuantity > 0);
        return Task.FromResult(books);
    }

    public Task<IEnumerable<Book>> GetRecentBooksAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var books = _books.Values.OrderByDescending(b => b.CreatedAt).Take(count);
        return Task.FromResult(books);
    }

    public Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = _books.Values.Select(b => b.Category).Distinct();
        return Task.FromResult(categories);
    }
}
