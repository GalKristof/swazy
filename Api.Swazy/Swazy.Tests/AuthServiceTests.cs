using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Providers;
using Api.Swazy.Services.Auth;
using Api.Swazy.Services.Users;
using AutoFixture;
using FluentAssertions;
using Moq;

namespace Swazy.Tests;

public class AuthServiceTests : BaseTest
{
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IHashingProvider> _mockHashingProvider;
    private readonly Mock<IJwtTokenProvider> _mockJwtTokenProvider;
    private readonly Mock<IUserService> _mockUserService;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockHashingProvider = new Mock<IHashingProvider>();
        _mockJwtTokenProvider = new Mock<IJwtTokenProvider>();
        _mockUserService = new Mock<IUserService>();
        _sut = new AuthService(
            _mockUserRepository.Object,
            _mockHashingProvider.Object,
            _mockJwtTokenProvider.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // arrange
        var loginDto = Fixture.Create<LoginUserDto>();
        var user = Fixture.Build<User>().With(u => u.Email, loginDto.Email).Create();
        var token = "test_token";

        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == loginDto.Email)).ReturnsAsync(user);
        _mockHashingProvider.Setup(h => h.ValidatePassword(loginDto.Password, user.HashedPassword)).Returns(true);
        _mockJwtTokenProvider.Setup(j => j.GenerateAccessToken(It.IsAny<TokenDto>())).Returns(token);

        // act
        var result = await _sut.LoginUserAsync(loginDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().Be(token);
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnInvalidCredentials_WhenUserNotFound()
    {
        // arrange
        var loginDto = Fixture.Create<LoginUserDto>();
        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == loginDto.Email)).ReturnsAsync((User)null);

        // act
        var result = await _sut.LoginUserAsync(loginDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.InvalidCredentials);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnInvalidCredentials_WhenPasswordIsInvalid()
    {
        // arrange
        var loginDto = Fixture.Create<LoginUserDto>();
        var user = Fixture.Build<User>().With(u => u.Email, loginDto.Email).Create();

        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == loginDto.Email)).ReturnsAsync(user);
        _mockHashingProvider.Setup(h => h.ValidatePassword(loginDto.Password, user.HashedPassword)).Returns(false);

        // act
        var result = await _sut.LoginUserAsync(loginDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.InvalidCredentials);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task LoginUserAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var loginDto = Fixture.Create<LoginUserDto>();
        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == loginDto.Email)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.LoginUserAsync(loginDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnToken_WhenRegistrationIsSuccessful()
    {
        // arrange
        var registerDto = Fixture.Create<RegisterUserDto>();
        var user = Fixture.Build<User>().With(u => u.Email, registerDto.Email).Create();
        var token = "test_token";
        var createUserResponse = new CommonResponse<User?> { Result = CommonResult.Success, Value = user };

        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == registerDto.Email)).ReturnsAsync((User)null);
        _mockUserService.Setup(s => s.CreateEntityAsync(It.IsAny<CreateUserDto>())).ReturnsAsync(createUserResponse);
        _mockJwtTokenProvider.Setup(j => j.GenerateAccessToken(It.IsAny<TokenDto>())).Returns(token);

        // act
        var result = await _sut.RegisterUserAsync(registerDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().Be(token);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnUserAlreadyExists_WhenUserAlreadyExists()
    {
        // arrange
        var registerDto = Fixture.Create<RegisterUserDto>();
        var existingUser = Fixture.Build<User>().With(u => u.Email, registerDto.Email).Create();
        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == registerDto.Email)).ReturnsAsync(existingUser);

        // act
        var result = await _sut.RegisterUserAsync(registerDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UserAlreadyExists);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnError_WhenUserServiceFails()
    {
        // arrange
        var registerDto = Fixture.Create<RegisterUserDto>();
        var createUserResponse = new CommonResponse<User?> { Result = CommonResult.UnknownError, Value = null };

        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == registerDto.Email)).ReturnsAsync((User)null);
        _mockUserService.Setup(s => s.CreateEntityAsync(It.IsAny<CreateUserDto>())).ReturnsAsync(createUserResponse);

        // act
        var result = await _sut.RegisterUserAsync(registerDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
    }
    
    [Fact]
    public async Task RegisterUserAsync_ShouldReturnUnknownError_WhenCreateUserSucceedsButReturnsNullUser()
    {
        // arrange
        var registerDto = Fixture.Create<RegisterUserDto>();
        // Simulate UserService returning Success but with a null User object
        var createUserResponse = new CommonResponse<User?> { Result = CommonResult.Success, Value = null };

        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == registerDto.Email)).ReturnsAsync((User)null);
        _mockUserService.Setup(s => s.CreateEntityAsync(It.IsAny<CreateUserDto>())).ReturnsAsync(createUserResponse);

        // act
        var result = await _sut.RegisterUserAsync(registerDto);

        // assert
        result.Should().NotBeNull();
        // Based on AuthService logic, if CreateEntityAsync returns Success but Value is null, it maps to UnknownError for RegisterUserAsync
        result.Result.Should().Be(CommonResult.UnknownError); 
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var registerDto = Fixture.Create<RegisterUserDto>();
        _mockUserRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Email == registerDto.Email)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.RegisterUserAsync(registerDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
    }
}
