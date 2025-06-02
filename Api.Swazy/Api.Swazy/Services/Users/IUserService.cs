using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Generic;

namespace Api.Swazy.Services.Users;

public interface IUserService : IGenericService<User, CreateUserDto, UpdateUserDto>;