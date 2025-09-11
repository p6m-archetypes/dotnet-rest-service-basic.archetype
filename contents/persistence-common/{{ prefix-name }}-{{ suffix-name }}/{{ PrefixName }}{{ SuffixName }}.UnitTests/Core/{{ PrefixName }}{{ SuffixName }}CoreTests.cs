using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using {{ PrefixName }}{{ SuffixName }}.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.Core;

public class {{ PrefixName }}{{ SuffixName }}CoreTests
{
    private readonly Mock<I{{ PrefixName }}Repository> _mockRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>> _mockLogger;
    private readonly {{ PrefixName }}{{ SuffixName }}Core _service;

    public {{ PrefixName }}{{ SuffixName }}CoreTests()
    {
        _mockRepository = new Mock<I{{ PrefixName }}Repository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>>();
        
        // Setup validation service to allow valid inputs by default
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((id, field) => Guid.Parse(id));

        _service = new {{ PrefixName }}{{ SuffixName }}Core(
            _mockRepository.Object, 
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Create{{ PrefixName }}_ShouldReturnCreatedEntity_WhenValidRequest()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = "Test Entity" };
        var savedEntity = new {{ PrefixName }}EntityBuilder()
            .WithName(request.Name)
            .WithId(Guid.NewGuid())
            .Generate();

        _mockRepository.Setup(x => x.Save(It.IsAny<{{ PrefixName }}Entity>()))
            .Callback<{{ PrefixName }}Entity>(entity => entity.Id = savedEntity.Id);
        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create{{ PrefixName }}Async(request);

        // Assert
        result.Should().NotBeNull();
        result.{{ PrefixName }}.Should().NotBeNull();
        result.{{ PrefixName }}.Name.Should().Be(request.Name);
        result.{{ PrefixName }}.Id.Should().NotBeNullOrEmpty();

        _mockRepository.Verify(x => x.Save(It.Is<{{ PrefixName }}Entity>(e => e.Name == request.Name)), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Get{{ PrefixName }}s_ShouldReturnPagedResults_WhenValidRequest()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 };
        var entities = new {{ PrefixName }}EntityBuilder().Generate(5);
        var page = new Page<{{ PrefixName }}Entity>
        {
            Items = entities,
            TotalElements = 5
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.Get{{ PrefixName }}sAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalElements.Should().Be(5);
        result.{{ PrefixName }}s.Should().HaveCount(5);
        result.{{ PrefixName }}s.Should().AllSatisfy(dto => 
        {
            dto.Id.Should().NotBeNullOrEmpty();
            dto.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData(0, 10)] // StartPage 0 should be normalized to 1
    [InlineData(-1, 10)] // Negative StartPage should be normalized to 1
    [InlineData(1, 0)] // PageSize 0 should be normalized to 1
    [InlineData(1, 150)] // PageSize > 100 should be normalized to 100
    public async Task Get{{ PrefixName }}s_ShouldNormalizeParameters_WhenInvalidValues(
        int startPage, int pageSize)
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = startPage, PageSize = pageSize };
        var page = new Page<{{ PrefixName }}Entity>
        {
            Items = new List<{{ PrefixName }}Entity>(),
            TotalElements = 0
        };

        _mockRepository.Setup(x => x.FindAsync(It.IsAny<PageRequest>()))
            .Returns(Task.FromResult(page));

        // Act
        var result = await _service.Get{{ PrefixName }}sAsync(request);

        // Assert
        _mockRepository.Verify(x => x.FindAsync(It.Is<PageRequest>(pr => 
            pr.StartPage >= 1 && pr.PageSize >= 1 && pr.PageSize <= 100)), Times.Once);
    }
}