namespace Api.Swazy.Models.Results;

public enum CommonResult
{
    Success = 1,
    NotFound = 2,
    UnknownError = 3,
    RequirementNotFound = 4,
    InvalidCredentials = 5,
    DatabaseError = 6,
    AlreadyIncluded = 7,
    UserAlreadyExists = 8 // Added UserAlreadyExists
}