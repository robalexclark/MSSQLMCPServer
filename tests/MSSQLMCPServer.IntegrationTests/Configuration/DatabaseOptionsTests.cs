using MSSQLMCPServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace MSSQLMCPServer.IntegrationTests.Configuration;

/// <summary>
/// Tests for DatabaseOptions configuration validation.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseOptionsTests
{
    [Fact]
    public void DatabaseOptionsValidator_WithValidConnectionString_ReturnsSuccess()
    {
        // Arrange
        DatabaseOptionsValidator validator = new DatabaseOptionsValidator();
        DatabaseOptions options = new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=Test;Trusted_Connection=true;"
        };

        // Act
        ValidateOptionsResult result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void DatabaseOptionsValidator_WithEmptyConnectionString_ReturnsFail()
    {
        // Arrange
        DatabaseOptionsValidator validator = new DatabaseOptionsValidator();
        DatabaseOptions options = new DatabaseOptions
        {
            ConnectionString = ""
        };

        // Act
        ValidateOptionsResult result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("MSSQL_CONNECTION_STRING", result.FailureMessage);
    }

    [Fact]
    public void DatabaseOptionsValidator_WithNullConnectionString_ReturnsFail()
    {
        // Arrange
        DatabaseOptionsValidator validator = new DatabaseOptionsValidator();
        DatabaseOptions options = new DatabaseOptions
        {
            ConnectionString = null!
        };

        // Act
        ValidateOptionsResult result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("MSSQL_CONNECTION_STRING", result.FailureMessage);
    }

    [Fact]
    public void DatabaseOptionsValidator_WithWhitespaceConnectionString_ReturnsFail()
    {
        // Arrange
        DatabaseOptionsValidator validator = new DatabaseOptionsValidator();
        DatabaseOptions options = new DatabaseOptions
        {
            ConnectionString = "   "
        };

        // Act
        ValidateOptionsResult result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("MSSQL_CONNECTION_STRING", result.FailureMessage);
    }

    [Fact]
    public void DatabaseOptions_SectionName_IsCorrect()
    {
        // Assert
        Assert.Equal("Database", DatabaseOptions.SectionName);
    }

    [Fact]
    public void DatabaseOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        DatabaseOptions options = new DatabaseOptions();

        // Assert
        Assert.Equal(string.Empty, options.ConnectionString);
    }

    [Fact]
    public void DatabaseOptions_Integration_WithDependencyInjection()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        
        // Simulate configuration
        string connectionString = "Server=localhost;Database=Test;Trusted_Connection=true;";
        
        services.Configure<DatabaseOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });
        
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IOptions<DatabaseOptions> options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(connectionString, options.Value.ConnectionString);
    }
} 
