using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MSSQLMCPServer.IntegrationTests.Infrastructure;

/// <summary>
/// EF Core InMemory-backed test fixture for integration tests.
/// Provides a shared in-memory database and seed/reset helpers.
/// </summary>
public class EfInMemoryTestFixture : IAsyncLifetime
{
    public TestDbContext Db { get; private set; } = null!;
    private DbContextOptions<TestDbContext> _options = null!;

    public Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"Tests_{Guid.NewGuid():N}")
            .Options;

        Db = new TestDbContext(_options);
        Seed();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Db.Dispose();
        return Task.CompletedTask;
    }

    public void Reset()
    {
        Db.Users.RemoveRange(Db.Users);
        Db.Orders.RemoveRange(Db.Orders);
        Db.Products.RemoveRange(Db.Products);
        Db.SaveChanges();
        Seed();
    }

    private void Seed()
    {
        Db.Users.AddRange(
            new User { Name = "John Doe", Email = "john@example.com", IsActive = true },
            new User { Name = "Jane Smith", Email = "jane@example.com", IsActive = true },
            new User { Name = "Bob Johnson", Email = "bob@example.com", IsActive = true }
        );
        Db.SaveChanges();

        Db.Orders.AddRange(
            new Order { UserId = Db.Users.First(u => u.Email == "john@example.com").Id, Total = 99.99m, Status = "Completed" },
            new Order { UserId = Db.Users.First(u => u.Email == "john@example.com").Id, Total = 149.99m, Status = "Pending" },
            new Order { UserId = Db.Users.First(u => u.Email == "jane@example.com").Id, Total = 75.50m, Status = "Completed" },
            new Order { UserId = Db.Users.First(u => u.Email == "bob@example.com").Id, Total = 200.00m, Status = "Cancelled" }
        );
        Db.Products.AddRange(
            new Product { Name = "Laptop", Price = 999.99m, Category = "Electronics", InStock = true },
            new Product { Name = "Mouse", Price = 29.99m, Category = "Electronics", InStock = true },
            new Product { Name = "Book", Price = 15.99m, Category = "Education", InStock = true }
        );
        Db.SaveChanges();
    }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public bool InStock { get; set; } = true;
}