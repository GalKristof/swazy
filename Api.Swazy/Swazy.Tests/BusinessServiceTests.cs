using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Businesses;
using Api.Swazy.Services.Users;
using Api.Swazy.Types;
using AutoFixture;
using FluentAssertions;
using Moq;

namespace Swazy.Tests;

public class BusinessServiceTests : BaseTest
{
    private readonly Mock<IRepository<Business>> _mockBusinessRepository;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly BusinessService _sut;

    public BusinessServiceTests()
    {
        _mockBusinessRepository = new Mock<IRepository<Business>>();
        _mockUserService = new Mock<IUserService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _sut = new BusinessService(
            _mockBusinessRepository.Object,
            _mockUserService.Object,
            Mapper);
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnSuccess_WhenEmployeeIsAdded()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        var business = Fixture.Build<Business>().With(b => b.Id, addEmployeeDto.BusinessId).Create();
        var user = Fixture.Build<User>().With(u => u.Email, addEmployeeDto.UserEmail).Create();
        var userResponse = new CommonResponse<User?> { Result = CommonResult.Success, Value = user };

        _mockBusinessRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(s => s.GetEntityByPropertyAsync(x => x.Email == addEmployeeDto.UserEmail)).ReturnsAsync(userResponse);
        _mockBusinessRepository.Setup(r => r.UpdateAsync(It.IsAny<Business>())).Returns(Task.CompletedTask);

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Employees.Should().ContainKey(user.Id);
        result.Value.Employees[user.Id].Should().Be(BusinessRole.Manager);
        _mockBusinessRepository.Verify(r => r.UpdateAsync(business), Times.Once);
        _mockBusinessRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnNotFound_WhenBusinessNotFound()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        _mockBusinessRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ReturnsAsync((Business)null);

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.NotFound);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnRequirementNotFound_WhenUserNotFound()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        var business = Fixture.Build<Business>().With(b => b.Id, addEmployeeDto.BusinessId).Create();
        var userResponse = new CommonResponse<User?> { Result = CommonResult.NotFound, Value = null };

        _mockBusinessRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(s => s.GetEntityByPropertyAsync(x => x.Email == addEmployeeDto.UserEmail)).ReturnsAsync(userResponse);

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.RequirementNotFound);
        result.Value.Should().BeNull();
    }
    
    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnRequirementNotFound_WhenUserServiceReturnsSuccessButNullUser()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        var business = Fixture.Build<Business>().With(b => b.Id, addEmployeeDto.BusinessId).Create();
        // Simulate UserService returning Success but with a null User value
        var userResponse = new CommonResponse<User?> { Result = CommonResult.Success, Value = null };

        _mockBusinessRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(s => s.GetEntityByPropertyAsync(x => x.Email == addEmployeeDto.UserEmail)).ReturnsAsync(userResponse);

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.RequirementNotFound);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        _mockBusinessRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
    }
    
    [Fact]
    public async Task AddEmployeeAsync_ShouldUseUnitOfWorkRepository_WhenUnitOfWorkIsProvided()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        var business = Fixture.Build<Business>().With(b => b.Id, addEmployeeDto.BusinessId).Create();
        var user = Fixture.Build<User>().With(u => u.Email, addEmployeeDto.UserEmail).Create();
        var userResponse = new CommonResponse<User?> { Result = CommonResult.Success, Value = user };
        
        var mockUowRepository = new Mock<IRepository<Business>>();
        mockUowRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ReturnsAsync(business);
        mockUowRepository.Setup(r => r.UpdateAsync(It.IsAny<Business>())).Returns(Task.CompletedTask);
        
        _mockUnitOfWork.Setup(uow => uow.Repository<Business>()).Returns(mockUowRepository.Object);
        _mockUserService.Setup(s => s.GetEntityByPropertyAsync(x => x.Email == addEmployeeDto.UserEmail)).ReturnsAsync(userResponse);

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto, _mockUnitOfWork.Object);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Employees.Should().ContainKey(user.Id);
        result.Value.Employees[user.Id].Should().Be(BusinessRole.Manager);
        
        mockUowRepository.Verify(r => r.UpdateAsync(business), Times.Once);
        _mockBusinessRepository.Verify(r => r.SaveChangesAsync(), Times.Never); // Default repo SaveChangesAsync should not be called
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(), Times.Never); // Rollback should not be called on success
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldRollbackUnitOfWork_WhenExceptionIsThrownAndUnitOfWorkIsProvided()
    {
        // arrange
        var addEmployeeDto = Fixture.Create<AddEmployeeToBusinessDto>();
        var mockUowRepository = new Mock<IRepository<Business>>();
        
        _mockUnitOfWork.Setup(uow => uow.Repository<Business>()).Returns(mockUowRepository.Object);
        // Simulate an exception when trying to get the business
        mockUowRepository.Setup(r => r.GetByIdAsync(addEmployeeDto.BusinessId)).ThrowsAsync(new Exception("DB Error"));

        // act
        var result = await _sut.AddEmployeeAsync(addEmployeeDto, _mockUnitOfWork.Object);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(), Times.Once); // Ensure RollbackAsync is called
        _mockBusinessRepository.Verify(r => r.SaveChangesAsync(), Times.Never); 
    }
}
