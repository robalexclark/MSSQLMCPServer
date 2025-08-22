using Xunit;

namespace MSSQLMCPServer.IntegrationTests.Infrastructure;

/// <summary>
/// Test collection definition that ensures all integration tests share the same database fixture.
/// This improves performance by reusing the in-memory database across tests.
/// </summary>
[CollectionDefinition("Database")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class DatabaseTestCollection : ICollectionFixture<EfInMemoryTestFixture>
{
    // This class has no code, it exists solely to define the collection
} 
