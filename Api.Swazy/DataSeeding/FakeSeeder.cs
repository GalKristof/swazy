using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence;
using Api.Swazy.Types;
using Bogus;

namespace Api.Swazy.DataSeeding;

public static class FakeSeeder
{
    public static List<User> GetFakeUsers(int amount)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PhoneNumber, f => f.GenerateHungarianPhoneNumber())
            .RuleFor(u => u.HashedPassword, f => f.Internet.Password())
            .RuleFor(u => u.Role, f => f.PickRandom<UserRole>());

        return userFaker.Generate(amount);
    }
    
    public static List<Business> GetFakeBusinesses(int amount)
    {
        var businessFaker = new Faker<Business>()
            .RuleFor(b => b.Name, f => f.Company.CompanyName())
            .RuleFor(b => b.Address, f => f.Address.FullAddress())
            .RuleFor(b => b.PhoneNumber, f => f.GenerateHungarianPhoneNumber())
            .RuleFor(b => b.Email, f => f.Internet.Email())
            .RuleFor(b => b.BusinessType, f => f.PickRandom<BusinessType>())
            .RuleFor(b => b.Employees, f => [])
            .RuleFor(b => b.WebsiteUrl, f => f.Internet.Url());

        return businessFaker.Generate(amount);
    }

    public static async Task SeedAsync(SwazyDbContext context)
    {
        if (!context.Users.Any())
        {
            var fakeUsers = GetFakeUsers(50);
            await context.Users.AddRangeAsync(fakeUsers);
        }

        if (!context.Businesses.Any())
        {
            var fakeBusinesses = GetFakeBusinesses(50);
            await context.Businesses.AddRangeAsync(fakeBusinesses);
        }

        await context.SaveChangesAsync();
    }
}

public static class FakerExtensions
{
    public static string GenerateHungarianPhoneNumber(this Faker faker)
    {
        return faker.Random.Bool()
            ? $"06{faker.PickRandom(new[] { "20", "30", "50", "70" })}{faker.Random.ReplaceNumbers("#######")}"
            : (faker.Random.Bool()
                ? $"+36{faker.PickRandom(new[] { "20", "30", "50", "70" })}{faker.Random.ReplaceNumbers("#######")}"
                : $"+361{faker.Random.ReplaceNumbers("#######")}");
    }
}