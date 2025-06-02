using Api.Swazy.Models.Base;
using Api.Swazy.Models.DTOs;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Generic;
using AutoFixture;
using FluentAssertions;
using Moq;

namespace Swazy.Tests;

public class GenericServiceTests : BaseTest
{
    private readonly Mock<IRepository<TestEntity>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly GenericService<TestEntity, TestCreateDto, TestUpdateDto> _sut;

    public GenericServiceTests()
    {
        _mockRepository = new Mock<IRepository<TestEntity>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _sut = new GenericService<TestEntity, TestCreateDto, TestUpdateDto>(
            _mockRepository.Object,
            Mapper);
    }

    // Dummy classes for testing GenericService
    public class BaseEntity { public Guid Id { get; set; } } // Added this for TestEntity
    public class BaseUpdateDto { public Guid Id { get; set; } } // Added this dummy DTO
    public class TestEntity : BaseEntity { public string Name { get; set; } }
    public class TestCreateDto { public string Name { get; set; } }
    public class TestUpdateDto : BaseUpdateDto { public string Name { get; set; } }

    [Fact]
    public async Task CreateEntityAsync_ShouldReturnSuccess_WhenEntityIsCreated()
    {
        // arrange
        var createDto = Fixture.Create<TestCreateDto>();
        var entity = Mapper.Map<TestEntity>(createDto);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<TestEntity>())).ReturnsAsync(entity);

        // act
        var result = await _sut.CreateEntityAsync(createDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(createDto.Name);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEntityAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var createDto = Fixture.Create<TestCreateDto>();
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<TestEntity>())).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.CreateEntityAsync(createDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never); // SaveChangesAsync should not be called if an exception occurs
    }

    [Fact]
    public async Task CreateEntityAsync_ShouldUseUnitOfWorkRepository_WhenUnitOfWorkIsProvided()
    {
        // arrange
        var createDto = Fixture.Create<TestCreateDto>();
        var entity = Mapper.Map<TestEntity>(createDto);
        var mockUowRepository = new Mock<IRepository<TestEntity>>();
        mockUowRepository.Setup(r => r.AddAsync(It.IsAny<TestEntity>())).ReturnsAsync(entity);
        _mockUnitOfWork.Setup(uow => uow.Repository<TestEntity>()).Returns(mockUowRepository.Object);

        // act
        var result = await _sut.CreateEntityAsync(createDto, _mockUnitOfWork.Object);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        mockUowRepository.Verify(r => r.AddAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<TestEntity>()), Times.Never); // Default repository should not be used
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never); // SaveChangesAsync should not be called on default repo when UoW is used
    }

    [Fact]
    public async Task GetAllEntitiesAsync_ShouldReturnAllEntities_WhenEntitiesExist()
    {
        // arrange
        var entities = Fixture.CreateMany<TestEntity>(3).ToList();
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        // act
        var result = await _sut.GetAllEntitiesAsync();

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeEquivalentTo(entities);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllEntitiesAsync_ShouldReturnEmptyList_WhenNoEntitiesExist()
    {
        // arrange
        var entities = new List<TestEntity>();
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        // act
        var result = await _sut.GetAllEntitiesAsync();

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllEntitiesAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.GetAllEntitiesAsync();

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().NotBeNull(); // GenericService initializes Value to an empty list in case of error
        result.Value.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldReturnSuccess_WhenEntityIsDeleted()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        _mockRepository.Setup(r => r.SoftDeleteAsync(entity)).ReturnsAsync(entity);

        // act
        var result = await _sut.DeleteEntityAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(entityId);
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.SoftDeleteAsync(entity), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // arrange
        var entityId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync((TestEntity)null);

        // act
        var result = await _sut.DeleteEntityAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.NotFound);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<TestEntity>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var entityId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.DeleteEntityAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<TestEntity>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldUseUnitOfWorkRepository_WhenUnitOfWorkIsProvided()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();
        var mockUowRepository = new Mock<IRepository<TestEntity>>();
        mockUowRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        mockUowRepository.Setup(r => r.SoftDeleteAsync(entity)).ReturnsAsync(entity);
        _mockUnitOfWork.Setup(uow => uow.Repository<TestEntity>()).Returns(mockUowRepository.Object);

        // act
        var result = await _sut.DeleteEntityAsync(entityId, _mockUnitOfWork.Object);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        mockUowRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        mockUowRepository.Verify(r => r.SoftDeleteAsync(entity), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never); // SaveChangesAsync should not be called on default repo when UoW is used
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldReturnSuccess_WhenEntityIsUpdated()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var updateDto = Fixture.Build<TestUpdateDto>().With(dto => dto.Id, entityId).Create();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();

        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>())).Returns(Task.CompletedTask); // UpdateAsync might not return a value

        // act
        var result = await _sut.UpdateEntityAsync(updateDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(entityId);
        result.Value.Name.Should().Be(updateDto.Name); // Assuming automapper maps Name
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var updateDto = Fixture.Build<TestUpdateDto>().With(dto => dto.Id, entityId).Create();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync((TestEntity)null);

        // act
        var result = await _sut.UpdateEntityAsync(updateDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.NotFound);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldReturnUnknownError_WhenExceptionIsThrownDuringGet()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var updateDto = Fixture.Build<TestUpdateDto>().With(dto => dto.Id, entityId).Create();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.UpdateEntityAsync(updateDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task UpdateEntityAsync_ShouldReturnUnknownError_WhenExceptionIsThrownDuringUpdate()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var updateDto = Fixture.Build<TestUpdateDto>().With(dto => dto.Id, entityId).Create();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>())).ThrowsAsync(new Exception("Test exception"));
        
        // act
        var result = await _sut.UpdateEntityAsync(updateDto);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldUseUnitOfWorkRepository_WhenUnitOfWorkIsProvided()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var updateDto = Fixture.Build<TestUpdateDto>().With(dto => dto.Id, entityId).Create();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();
        var mockUowRepository = new Mock<IRepository<TestEntity>>();
        
        mockUowRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        mockUowRepository.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.Repository<TestEntity>()).Returns(mockUowRepository.Object);

        // act
        var result = await _sut.UpdateEntityAsync(updateDto, _mockUnitOfWork.Object);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(entityId);
        mockUowRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        mockUowRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never); // SaveChangesAsync should not be called on default repo when UoW is used
    }

    [Fact]
    public async Task GetSingleEntityByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // arrange
        var entityId = Guid.NewGuid();
        var entity = Fixture.Build<TestEntity>().With(e => e.Id, entityId).Create();
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Id == entityId)).ReturnsAsync(entity);

        // act
        var result = await _sut.GetSingleEntityByIdAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(entityId);
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Id == entityId), Times.Once);
    }

    [Fact]
    public async Task GetSingleEntityByIdAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // arrange
        var entityId = Guid.NewGuid();
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Id == entityId)).ReturnsAsync((TestEntity)null);

        // act
        var result = await _sut.GetSingleEntityByIdAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.NotFound);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Id == entityId), Times.Once);
    }

    [Fact]
    public async Task GetSingleEntityByIdAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var entityId = Guid.NewGuid();
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Id == entityId)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.GetSingleEntityByIdAsync(entityId);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Id == entityId), Times.Once);
    }

    [Fact]
    public async Task GetEntityByPropertyAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // arrange
        var entityName = "TestName";
        var entity = Fixture.Build<TestEntity>().With(e => e.Name, entityName).Create();
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Name == entityName)).ReturnsAsync(entity);

        // act
        var result = await _sut.GetEntityByPropertyAsync(x => x.Name == entityName);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.Success);
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(entityName);
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Name == entityName), Times.Once);
    }

    [Fact]
    public async Task GetEntityByPropertyAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // arrange
        var entityName = "TestName";
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Name == entityName)).ReturnsAsync((TestEntity)null);

        // act
        var result = await _sut.GetEntityByPropertyAsync(x => x.Name == entityName);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.NotFound);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Name == entityName), Times.Once);
    }

    [Fact]
    public async Task GetEntityByPropertyAsync_ShouldReturnUnknownError_WhenExceptionIsThrown()
    {
        // arrange
        var entityName = "TestName";
        _mockRepository.Setup(r => r.SingleOrDefaultAsync(x => x.Name == entityName)).ThrowsAsync(new Exception("Test exception"));

        // act
        var result = await _sut.GetEntityByPropertyAsync(x => x.Name == entityName);

        // assert
        result.Should().NotBeNull();
        result.Result.Should().Be(CommonResult.UnknownError);
        result.Value.Should().BeNull();
        _mockRepository.Verify(r => r.SingleOrDefaultAsync(x => x.Name == entityName), Times.Once);
    }
}
