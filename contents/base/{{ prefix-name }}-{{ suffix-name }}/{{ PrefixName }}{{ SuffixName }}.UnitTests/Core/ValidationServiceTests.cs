using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.Core;

public class ValidationServiceTests
{
    private readonly Mock<ILogger<ValidationService>> _mockLogger;
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ValidationService>>();
        _validationService = new ValidationService(_mockLogger.Object);
    }

    [Fact]
    public void ValidateCreateRequest_ShouldNotThrow_WhenRequestIsValid()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = "Valid Name" };

        // Act & Assert
        var act = () => _validationService.ValidateCreateRequest(request);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCreateRequest_ShouldThrowValidationException_WhenNameIsEmpty(string name)
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = name };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateCreateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Name");
        exception.ValidationErrors["Name"][0].Should().Be("Name is required and cannot be empty.");
    }

    [Fact]
    public void ValidateCreateRequest_ShouldThrowValidationException_WhenRequestIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateCreateRequest(null!));
        exception.ValidationErrors.Should().ContainKey("request");
        exception.ValidationErrors["request"][0].Should().Be("Request cannot be null.");
    }

    [Fact]
    public void ValidateCreateRequest_ShouldThrowValidationException_WhenNameIsTooLong()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = new string('a', 101) };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateCreateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Name");
        exception.ValidationErrors["Name"][0].Should().Be("Name cannot exceed 100 characters.");
    }

    [Theory]
    [InlineData("Valid Name")]
    [InlineData("Name-With_Dots.123")]
    [InlineData("Simple")]
    public void ValidateCreateRequest_ShouldNotThrow_WhenNameIsValid(string name)
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = name };

        // Act & Assert
        var act = () => _validationService.ValidateCreateRequest(request);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Name@WithSpecial")]
    [InlineData("Name#With!")]
    [InlineData("Name$WithSymbol")]
    public void ValidateCreateRequest_ShouldThrowValidationException_WhenNameContainsInvalidCharacters(string name)
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = name };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateCreateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Name");
        exception.ValidationErrors["Name"][0].Should().Contain("invalid characters");
    }

    [Fact]
    public void ValidateCreateRequest_ShouldThrowValidationException_WhenIdIsProvided()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Name = "Valid Name", Id = Guid.NewGuid().ToString() };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateCreateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("ID should not be provided for create requests.");
    }

    [Fact]
    public void ValidateUpdateRequest_ShouldNotThrow_WhenRequestIsValid()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Id = Guid.NewGuid().ToString(), Name = "Valid Name" };

        // Act & Assert
        var act = () => _validationService.ValidateUpdateRequest(request);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateUpdateRequest_ShouldThrowValidationException_WhenIdIsEmpty(string id)
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Id = id, Name = "Valid Name" };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateUpdateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("ID is required for update requests.");
    }

    [Fact]
    public void ValidateUpdateRequest_ShouldThrowValidationException_WhenIdIsInvalidGuid()
    {
        // Arrange
        var request = new {{ PrefixName }}Dto { Id = "invalid-guid", Name = "Valid Name" };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateUpdateRequest(request));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("ID must be a valid GUID format.");
    }

    [Fact]
    public void ValidatePaginationRequest_ShouldNotThrow_WhenRequestIsValid()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 10 };

        // Act & Assert
        var act = () => _validationService.ValidatePaginationRequest(request);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidatePaginationRequest_ShouldThrowValidationException_WhenStartPageIsInvalid(int startPage)
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = startPage, PageSize = 10 };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidatePaginationRequest(request));
        exception.ValidationErrors.Should().ContainKey("StartPage");
        exception.ValidationErrors["StartPage"][0].Should().Be("StartPage must be greater than 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidatePaginationRequest_ShouldThrowValidationException_WhenPageSizeIsInvalid(int pageSize)
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = pageSize };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidatePaginationRequest(request));
        exception.ValidationErrors.Should().ContainKey("PageSize");
        exception.ValidationErrors["PageSize"][0].Should().Be("PageSize must be greater than 0.");
    }

    [Fact]
    public void ValidatePaginationRequest_ShouldThrowValidationException_WhenPageSizeIsTooLarge()
    {
        // Arrange
        var request = new Get{{ PrefixName }}sRequest { StartPage = 1, PageSize = 1001 };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidatePaginationRequest(request));
        exception.ValidationErrors.Should().ContainKey("PageSize");
        exception.ValidationErrors["PageSize"][0].Should().Be("PageSize cannot exceed 1000.");
    }

    [Fact]
    public void ValidateAndParseId_ShouldReturnGuid_WhenIdIsValid()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var idString = validGuid.ToString();

        // Act
        var result = _validationService.ValidateAndParseId(idString);

        // Assert
        result.Should().Be(validGuid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndParseId_ShouldThrowValidationException_WhenIdIsEmpty(string id)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateAndParseId(id));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("Id is required and cannot be empty.");
    }

    [Fact]
    public void ValidateAndParseId_ShouldThrowValidationException_WhenIdIsInvalidFormat()
    {
        // Arrange
        var invalidId = "not-a-guid";

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateAndParseId(invalidId));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("Id must be a valid GUID format.");
    }

    [Fact]
    public void ValidateAndParseId_ShouldThrowValidationException_WhenIdIsEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty.ToString();

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateAndParseId(emptyGuid));
        exception.ValidationErrors.Should().ContainKey("Id");
        exception.ValidationErrors["Id"][0].Should().Be("Id cannot be an empty GUID.");
    }

    [Fact]
    public void ValidateAndParseId_ShouldUseCustomFieldName_WhenProvided()
    {
        // Arrange
        var invalidId = "not-a-guid";
        var fieldName = "CustomId";

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _validationService.ValidateAndParseId(invalidId, fieldName));
        exception.ValidationErrors.Should().ContainKey(fieldName);
        exception.ValidationErrors[fieldName][0].Should().Be($"{fieldName} must be a valid GUID format.");
    }
}