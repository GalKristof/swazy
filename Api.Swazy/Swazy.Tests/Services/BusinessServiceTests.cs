using Api.Swazy.Models.DTOs.BusinessEmployees;
using Api.Swazy.Models.DTOs.Users; // Added for User DTOs if userService returns them
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Businesses;
using Api.Swazy.Services.Users;
using Api.Swazy.Types;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query; // For IIncludableQueryable

public class BusinessServiceTests
{
    private readonly Mock<IRepository<Business>> _mockBusinessRepo;
    private readonly Mock<IRepository<BusinessEmployee>> _mockBusinessEmployeeRepo;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly BusinessService _businessService;

    public BusinessServiceTests()
    {
        _mockBusinessRepo = new Mock<IRepository<Business>>();
        _mockBusinessEmployeeRepo = new Mock<IRepository<BusinessEmployee>>();
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Setup UoW to return specific repository mocks
        _mockUnitOfWork.Setup(uow => uow.Repository<Business>()).Returns(_mockBusinessRepo.Object);
        _mockUnitOfWork.Setup(uow => uow.Repository<BusinessEmployee>()).Returns(_mockBusinessEmployeeRepo.Object);
        // If IUserService is based on a generic User repository that could be part of UoW:
        // var mockUserRepo = new Mock<IRepository<User>>();
        // _mockUnitOfWork.Setup(uow => uow.Repository<User>()).Returns(mockUserRepo.Object);


        _businessService = new BusinessService(
            _mockBusinessRepo.Object,
            _mockBusinessEmployeeRepo.Object, // Corrected order based on BusinessService constructor
            _mockUserService.Object,
            _mockMapper.Object
        );
    }

    // --- AddEmployeeAsync Tests ---

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "test@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid();
        var business = new Business { Id = createDto.BusinessId, Name = "Test Business" };
        var userToAdd = new User { Id = Guid.NewGuid(), Email = createDto.UserEmail, FirstName = "Test", LastName = "User" };
        var performingUser = new User { Id = performingUserId, FirstName = "Admin", LastName = "User" };

        var createdEmployeeEntity = new BusinessEmployee
        {
            Id = Guid.NewGuid(),
            BusinessId = createDto.BusinessId,
            UserId = userToAdd.Id,
            Role = createDto.Role,
            HiredBy = performingUserId,
            User = userToAdd, // For mapper
            HiredByUser = performingUser // For mapper
        };
        var expectedDto = new BusinessEmployeeDto { BusinessId = createDto.BusinessId, UserId = userToAdd.Id, Role = createDto.Role };

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(createDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((Expression<Func<User, bool>> predicate) => {
                // Simulate finding userToAdd by email and performingUser by ID
                if (predicate.Compile().Invoke(new User { Email = createDto.UserEmail })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = userToAdd };
                if (predicate.Compile().Invoke(new User { Id = performingUserId })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = performingUser };
                return new CommonResponse<User?> { Result = CommonResult.NotFound };
            });

        // Mocking IRepository<BusinessEmployee>.GetQueryable()
        var mockBeQueryable = new List<BusinessEmployee>().AsQueryable();
        _mockBusinessEmployeeRepo.Setup(repo => repo.GetQueryable()).Returns(mockBeQueryable);
        // Mocking AddAsync for BusinessEmployee
        _mockBusinessEmployeeRepo.Setup(repo => repo.AddAsync(It.IsAny<BusinessEmployee>()))
            .ReturnsAsync((BusinessEmployee be) => {
                be.Id = createdEmployeeEntity.Id; // Simulate Id generation
                // Simulate that after AddAsync, the createdEmployeeEntity with includes is fetched:
                 var listWithEntity = new List<BusinessEmployee> { createdEmployeeEntity }.AsQueryable();
                _mockBusinessEmployeeRepo.Setup(r => r.GetQueryable()).Returns(listWithEntity); // Update queryable to return the added entity for the subsequent fetch
                return be;
            });


        _mockMapper.Setup(m => m.Map<BusinessEmployeeDto>(It.IsAny<BusinessEmployee>())).Returns(expectedDto);

        // Act
        var result = await _businessService.AddEmployeeAsync(createDto, performingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommonResult.Success, result.Result);
        Assert.Equal(expectedDto, result.Value);
        _mockBusinessEmployeeRepo.Verify(repo => repo.AddAsync(It.Is<BusinessEmployee>(be =>
            be.BusinessId == createDto.BusinessId &&
            be.UserId == userToAdd.Id &&
            be.Role == createDto.Role &&
            be.HiredBy == performingUserId
        )), Times.Once);
        _mockBusinessEmployeeRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once); // Assuming no UoW
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnNotFound_WhenBusinessNotFound()
    {
        // Arrange
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "test@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid();

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(createDto.BusinessId)).ReturnsAsync((Business)null);

        // Act
        var result = await _businessService.AddEmployeeAsync(createDto, performingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommonResult.NotFound, result.Result);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnRequirementNotFound_WhenUserToAssignNotFound()
    {
        // Arrange
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "nonexistent@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid();
        var business = new Business { Id = createDto.BusinessId, Name = "Test Business" };

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(createDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.Is<Expression<Func<User, bool>>>(expr =>
            expr.Compile().Invoke(new User { Email = createDto.UserEmail }) // Check if the expression targets the email
        ))).ReturnsAsync(new CommonResponse<User?> { Result = CommonResult.NotFound });


        // Act
        var result = await _businessService.AddEmployeeAsync(createDto, performingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommonResult.RequirementNotFound, result.Result);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnRequirementNotFound_WhenPerformingUserNotFound()
    {
        // Arrange
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "test@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid(); // This user won't be found
        var business = new Business { Id = createDto.BusinessId, Name = "Test Business" };
        var userToAssign = new User { Id = Guid.NewGuid(), Email = createDto.UserEmail };

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(createDto.BusinessId)).ReturnsAsync(business);
        // User to assign is found
        _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.Is<Expression<Func<User, bool>>>(expr =>
            expr.Compile().Invoke(new User { Email = createDto.UserEmail })
        ))).ReturnsAsync(new CommonResponse<User?> { Result = CommonResult.Success, Value = userToAssign });
        // Performing user is NOT found
        _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.Is<Expression<Func<User, bool>>>(expr =>
            expr.Compile().Invoke(new User { Id = performingUserId })
        ))).ReturnsAsync(new CommonResponse<User?> { Result = CommonResult.NotFound });

        // Act
        var result = await _businessService.AddEmployeeAsync(createDto, performingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommonResult.RequirementNotFound, result.Result);
        Assert.Null(result.Value);
    }


    [Fact]
    public async Task AddEmployeeAsync_ShouldReturnAlreadyIncluded_WhenEmployeeAlreadyExistsInBusiness()
    {
        // Arrange
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "test@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid();
        var business = new Business { Id = createDto.BusinessId };
        var userToAdd = new User { Id = Guid.NewGuid(), Email = createDto.UserEmail };
        var performingUser = new User { Id = performingUserId };
        var existingEmployee = new BusinessEmployee { BusinessId = createDto.BusinessId, UserId = userToAdd.Id, IsDeleted = false };

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(createDto.BusinessId)).ReturnsAsync(business);
        _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((Expression<Func<User, bool>> predicate) => {
                if (predicate.Compile().Invoke(new User { Email = createDto.UserEmail })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = userToAdd };
                if (predicate.Compile().Invoke(new User { Id = performingUserId })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = performingUser };
                return new CommonResponse<User?> { Result = CommonResult.NotFound };
            });

        var mockBeQueryable = new List<BusinessEmployee> { existingEmployee }.AsQueryable();
        _mockBusinessEmployeeRepo.Setup(repo => repo.GetQueryable()).Returns(mockBeQueryable);

        // Act
        var result = await _businessService.AddEmployeeAsync(createDto, performingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CommonResult.AlreadyIncluded, result.Result);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task AddEmployeeAsync_ShouldUseUnitOfWorkRepositoriesAndSaveChanges_WhenUnitOfWorkIsProvided()
    {
        // Arrange
        var uow = _mockUnitOfWork.Object; // Use the class field _mockUnitOfWork
        var createDto = new CreateBusinessEmployeeDto { BusinessId = Guid.NewGuid(), UserEmail = "test@example.com", Role = BusinessRole.Employee };
        var performingUserId = Guid.NewGuid();
        var business = new Business { Id = createDto.BusinessId };
        var userToAdd = new User { Id = Guid.NewGuid(), Email = createDto.UserEmail };
        var performingUser = new User { Id = performingUserId };
        var createdEmployeeEntity = new BusinessEmployee { Id = Guid.NewGuid(), BusinessId = createDto.BusinessId, UserId = userToAdd.Id, User = userToAdd, HiredByUser = performingUser };
        var expectedDto = new BusinessEmployeeDto();

        // UoW setup for Business repo
        _mockUnitOfWork.Setup(u => u.Repository<Business>().GetByIdAsync(createDto.BusinessId)).ReturnsAsync(business);

        // Standard User Service mocks (not through UoW for this test's focus unless IUserService itself uses UoW)
         _mockUserService.Setup(us => us.GetEntityByPropertyAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((Expression<Func<User, bool>> predicate) => {
                if (predicate.Compile().Invoke(new User { Email = createDto.UserEmail })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = userToAdd };
                if (predicate.Compile().Invoke(new User { Id = performingUserId })) return new CommonResponse<User?> { Result = CommonResult.Success, Value = performingUser };
                return new CommonResponse<User?> { Result = CommonResult.NotFound };
            });

        // UoW setup for BusinessEmployee repo
        var mockBeQueryableEmpty = new List<BusinessEmployee>().AsQueryable(); // No existing employee
        _mockUnitOfWork.Setup(u => u.Repository<BusinessEmployee>().GetQueryable()).Returns(mockBeQueryableEmpty);
        _mockUnitOfWork.Setup(u => u.Repository<BusinessEmployee>().AddAsync(It.IsAny<BusinessEmployee>()))
             .ReturnsAsync((BusinessEmployee be) => {
                be.Id = createdEmployeeEntity.Id; // Simulate Id generation
                var listWithEntity = new List<BusinessEmployee> { createdEmployeeEntity }.AsQueryable();
                // This setup for GetQueryable needs to be on the mock returned by uow.Repository<BusinessEmployee>()
                _mockBusinessEmployeeRepo.Setup(r => r.GetQueryable()).Returns(listWithEntity); // This should be on the UoW's instance mock
                return be;
            });


        _mockMapper.Setup(m => m.Map<BusinessEmployeeDto>(It.IsAny<BusinessEmployee>())).Returns(expectedDto);

        // Act
        await _businessService.AddEmployeeAsync(createDto, performingUserId, uow);

        // Assert
        _mockUnitOfWork.Verify(u => u.Repository<Business>(), Times.Once); // Verify Business repo was obtained from UoW
        _mockUnitOfWork.Verify(u => u.Repository<BusinessEmployee>(), Times.Exactly(2)); // Once for check, once for add, once for fetch after add
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once); // SaveChangesAsync on UoW
        _mockBusinessEmployeeRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never); // Not on individual repo
    }

    // --- GetBusinessEmployeesAsync Tests ---
    [Fact]
    public async Task GetBusinessEmployeesAsync_ShouldReturnEmployees_WhenBusinessExistsAndHasEmployees()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var business = new Business { Id = businessId };
        var employees = new List<BusinessEmployee>
        {
            new BusinessEmployee { Id = Guid.NewGuid(), BusinessId = businessId, UserId = Guid.NewGuid(), User = new User(), HiredByUser = new User() },
            new BusinessEmployee { Id = Guid.NewGuid(), BusinessId = businessId, UserId = Guid.NewGuid(), User = new User(), HiredByUser = new User() }
        };
        var employeeDtos = new List<BusinessEmployeeDto> { new BusinessEmployeeDto(), new BusinessEmployeeDto() };

        _mockBusinessRepo.Setup(repo => repo.GetByIdAsync(businessId)).ReturnsAsync(business);
        var mockBeQueryable = employees.AsQueryable();
        _mockBusinessEmployeeRepo.Setup(repo => repo.GetQueryable()).Returns(mockBeQueryable);
        _mockMapper.Setup(m => m.Map<IEnumerable<BusinessEmployeeDto>>(It.Is<IEnumerable<BusinessEmployee>>(e => e.Count() == 2))).Returns(employeeDtos);

        // Act
        var result = await _businessService.GetBusinessEmployeesAsync(businessId);

        // Assert
        Assert.Equal(CommonResult.Success, result.Result);
        Assert.Equal(employeeDtos, result.Value);
    }

    // --- RemoveEmployeeAsync Tests ---
    [Fact]
    public async Task RemoveEmployeeAsync_ShouldReturnTrueAndCallSoftDeleteAsync_WhenEmployeeExists()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var performingUserId = Guid.NewGuid();
        var employeeToDelete = new BusinessEmployee { Id = Guid.NewGuid(), BusinessId = businessId, UserId = userId, IsDeleted = false };

        var mockBeQueryable = new List<BusinessEmployee> { employeeToDelete }.AsQueryable();
        _mockBusinessEmployeeRepo.Setup(repo => repo.GetQueryable()).Returns(mockBeQueryable);
        _mockBusinessEmployeeRepo.Setup(repo => repo.SoftDeleteAsync(employeeToDelete)).ReturnsAsync(employeeToDelete);

        // Act
        var result = await _businessService.RemoveEmployeeAsync(businessId, userId, performingUserId);

        // Assert
        Assert.True(result.Value);
        Assert.Equal(CommonResult.Success, result.Result);
        _mockBusinessEmployeeRepo.Verify(repo => repo.SoftDeleteAsync(employeeToDelete), Times.Once);
        _mockBusinessEmployeeRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    // ... More tests will be added here, space permitting ...
    // It's very likely I won't be able to fit all tests in one go.
}
