using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using {{ PrefixName }}{{ SuffixName }}.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.Persistence;

public class {{ PrefixName }}RepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly {{ PrefixName }}Repository _repository;
    private readonly ILogger<{{ PrefixName }}Repository> _logger;

    public {{ PrefixName }}RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _logger = new Mock<ILogger<{{ PrefixName }}Repository>>().Object;
        _repository = new {{ PrefixName }}Repository(_context, _logger);
    }

    [Fact]
    public void Save_ShouldAddEntityToContext()
    {
        // Arrange
        var entity = new {{ PrefixName }}EntityBuilder().Generate();

        // Act
        _repository.Save(entity);

        // Assert
        _context.Entry(entity).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntity()
    {
        // Arrange
        var entity = new {{ PrefixName }}EntityBuilder().Generate();
        _repository.Save(entity);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var savedEntity = await _context.{{ PrefixName }}s.FindAsync(entity.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var entities = new {{ PrefixName }}EntityBuilder().Generate(15);
        await _context.{{ PrefixName }}s.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var pageRequest = new PageRequest { StartPage = 1, PageSize = 10 };

        // Act
        var result = await _repository.FindAsync(pageRequest);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalElements.Should().Be(15);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmptyPage_WhenNoEntities()
    {
        // Arrange
        var pageRequest = new PageRequest { StartPage = 1, PageSize = 10 };

        // Act
        var result = await _repository.FindAsync(pageRequest);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalElements.Should().Be(0);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var entity = new {{ PrefixName }}EntityBuilder().Generate();
        await _context.{{ PrefixName }}s.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.FindByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}