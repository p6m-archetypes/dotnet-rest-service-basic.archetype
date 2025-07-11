using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using {{ PrefixName }}{{ SuffixName }}.UnitTests.TestBuilders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.Core;

public class ErrorHandlingTests
{
    private readonly Mock<I{{ PrefixName }}Repository> _mockRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>> _mockLogger;
    private readonly {{ PrefixName }}{{ SuffixName }}Core _service;

    public ErrorHandlingTests()
    {
        _mockRepository = new Mock<I{{ PrefixName }}Repository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<{{ PrefixName }}{{ SuffixName }}Core>>();
        
        _service = new {{ PrefixName }}{{ SuffixName }}Core(
            _mockRepository.Object, 
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Create{{ PrefixName }}_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = "" };
        _mockValidationService
            .Setup(x => x.ValidateCreateRequest(It.IsAny<{{ PrefixName }}Dto>()))
            .Throws(new ValidationException("Name", "Name is required"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.Create{{ PrefixName }}Async(request));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Get{{ PrefixName }}_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<{{ PrefixName }}Entity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Get{{ PrefixName }}Async(id));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("{{ PrefixName }}");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task Get{{ PrefixName }}_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        // Arrange
        var id = "invalid-guid";
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ValidationException("Id", "Invalid GUID format"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.Get{{ PrefixName }}Async(id));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("Id");
    }

    [Fact]
    public async Task Update{{ PrefixName }}_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Id = Guid.NewGuid().ToString(), Name = "Updated Name" };
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateUpdateRequest(It.IsAny<{{ PrefixName }}Dto>()));
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<{{ PrefixName }}Entity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Update{{ PrefixName }}Async(request));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("{{ PrefixName }}");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task Delete{{ PrefixName }}_ShouldThrowEntityNotFoundException_WhenEntityNotFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var entityId = Guid.NewGuid();
        
        _mockValidationService
            .Setup(x => x.ValidateAndParseId(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(entityId);
        
        _mockRepository
            .Setup(x => x.FindByIdAsync(entityId))
            .Returns(Task.FromResult<{{ PrefixName }}Entity?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Delete{{ PrefixName }}Async(id));
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
        exception.EntityType.Should().Be("{{ PrefixName }}");
        exception.EntityId.Should().Be(entityId.ToString());
    }

    [Fact]
    public async Task Get{{ PrefixName }}s_ShouldThrowValidationException_WhenPaginationIsInvalid()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = -1, PageSize = 0 };
        
        _mockValidationService
            .Setup(x => x.ValidatePaginationRequest(It.IsAny<Get{{ PrefixName }}sRequest>()))
            .Throws(new ValidationException(new Dictionary<string, string[]>
            {
                { "StartPage", new[] { "StartPage must be greater than 0" } },
                { "PageSize", new[] { "PageSize must be greater than 0" } }
            }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.Get{{ PrefixName }}sAsync(request));
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.ValidationErrors.Should().ContainKey("StartPage");
        exception.ValidationErrors.Should().ContainKey("PageSize");
    }
}