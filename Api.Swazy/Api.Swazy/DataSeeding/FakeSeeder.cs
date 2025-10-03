using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence;
using Api.Swazy.Types;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Swazy.DataSeeding;

public static class FakeSeeder
{
        public static async Task SeedAsync(SwazyDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var fakeUsers = GetFakeUsers(50);
            await context.Users.AddRangeAsync(fakeUsers);
            await context.SaveChangesAsync();
        }

        if (!await context.Businesses.AnyAsync())
        {
            var fakeBusinesses = GetFakeBusinesses(20);
            await context.Businesses.AddRangeAsync(fakeBusinesses);
            await context.SaveChangesAsync();
        }

        if (!await context.UserBusinessAccesses.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var businesses = await context.Businesses.ToListAsync();
            var random = new Random();

            var userBusinessAccesses = new List<UserBusinessAccess>();

            foreach (var business in businesses)
            {
                var numberOfEmployees = random.Next(1, 6);
                var selectedUsers = users.OrderBy(x => random.Next()).Take(numberOfEmployees).ToList();

                foreach (var user in selectedUsers)
                {
                    var role = selectedUsers.IndexOf(user) == 0 
                        ? BusinessRole.Owner 
                        : (BusinessRole)random.Next(0, 3);

                    userBusinessAccesses.Add(new UserBusinessAccess
                    {
                        UserId = user.Id,
                        BusinessId = business.Id,
                        Role = role
                    });
                }
            }

            await context.UserBusinessAccesses.AddRangeAsync(userBusinessAccesses);
            await context.SaveChangesAsync();
        }

        if (!await context.Services.AnyAsync())
        {
            var fakeServices = GetFakeServices(30);
            await context.Services.AddRangeAsync(fakeServices);
            await context.SaveChangesAsync();
        }

        if (!await context.BusinessServices.AnyAsync())
        {
            var businesses = await context.Businesses.ToListAsync();
            var services = await context.Services.ToListAsync();
            var random = new Random();

            var businessServices = new List<BusinessService>();

            foreach (var business in businesses)
            {
                var numberOfServices = random.Next(2, 6);
                var selectedServices = services
                    .Where(s => s.BusinessType == business.BusinessType || s.BusinessType == BusinessType.Other)
                    .OrderBy(x => random.Next())
                    .Take(numberOfServices)
                    .ToList();

                foreach (var service in selectedServices)
                {
                    businessServices.Add(new BusinessService
                    {
                        BusinessId = business.Id,
                        ServiceId = service.Id,
                        Price = random.Next(1000, 10000) / 100m,
                        Duration = (ushort)random.Next(15, 121)
                    });
                }
            }

            await context.BusinessServices.AddRangeAsync(businessServices);
            await context.SaveChangesAsync();
        }

        if (!await context.Bookings.AnyAsync())
        {
            var businessServices = await context.BusinessServices
                .Include(bs => bs.Business)
                    .ThenInclude(b => b.UserAccesses)
                .ToListAsync();
            var users = await context.Users.ToListAsync();
            var random = new Random();

            var bookings = new List<Booking>();
            var faker = new Faker();

            for (int i = 0; i < 100; i++)
            {
                var businessService = businessServices[random.Next(businessServices.Count)];
                var business = businessService.Business;
                
                var bookedByUser = random.Next(100) < 70 
                    ? users[random.Next(users.Count)] 
                    : null;

                var employees = business.UserAccesses
                    .Where(ua => ua.Role == BusinessRole.Employee || ua.Role == BusinessRole.Manager)
                    .ToList();
                var employeeId = employees.Any() 
                    ? employees[random.Next(employees.Count)].UserId 
                    : (Guid?)null;

                bookings.Add(new Booking
                {
                    BookingDate = DateTimeOffset.UtcNow.AddDays(random.Next(-30, 60)),
                    Notes = random.Next(100) < 30 ? faker.Lorem.Sentence() : null,
                    FirstName = bookedByUser?.FirstName ?? faker.Name.FirstName(),
                    LastName = bookedByUser?.LastName ?? faker.Name.LastName(),
                    Email = bookedByUser?.Email ?? faker.Internet.Email(),
                    PhoneNumber = bookedByUser?.PhoneNumber ?? faker.GenerateHungarianPhoneNumber(),
                    BusinessServiceId = businessService.Id,
                    EmployeeId = employeeId,
                    BookedByUserId = bookedByUser?.Id
                });
            }

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
        }
    }

    private static List<User> GetFakeUsers(int amount)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PhoneNumber, f => f.GenerateHungarianPhoneNumber())
            .RuleFor(u => u.HashedPassword, f => f.Internet.Password())
            .RuleFor(u => u.SystemRole, f => f.PickRandom<UserRole>());

        return userFaker.Generate(amount);
    }

    private static List<Business> GetFakeBusinesses(int amount)
    {
        var businessFaker = new Faker<Business>()
            .RuleFor(b => b.Name, f => f.Company.CompanyName())
            .RuleFor(b => b.Address, f => f.Address.FullAddress())
            .RuleFor(b => b.PhoneNumber, f => f.GenerateHungarianPhoneNumber())
            .RuleFor(b => b.Email, f => f.Internet.Email())
            .RuleFor(b => b.BusinessType, f => BusinessType.BarberSalon)
            .RuleFor(b => b.WebsiteUrl, f => f.Internet.Url());

        return businessFaker.Generate(amount);
    }

    private static List<Service> GetFakeServices(int amount)
    {
        var barberServices = new List<(string Tag, string Value)>
        {
            ("Hajvágás", "Férfi hajvágás"),
            ("Hajvágás", "Gyerek hajvágás"),
            ("Hajvágás", "Diák hajvágás"),
            ("Szakáll", "Szakállvágás"),
            ("Szakáll", "Szakálligazítás"),
            ("Szakáll", "Szakállformázás"),
            ("Borotválás", "Borotválás"),
            ("Borotválás", "Kontúr borotválás"),
            ("Trimmelés", "Hajvégek igazítása"),
            ("Trimmelés", "Kontúr igazítás"),
            ("Trimmelés", "Nyakszirt igazítás"),
            ("Hajmosás", "Hajmosás"),
            ("Festés", "Hajfestés"),
            ("Festés", "Ősz fedés"),
            ("Mintázás", "Hajmintázás"),
            ("Mintázás", "Szakállmintázás"),
            ("Kombi", "Hajvágás + Szakáll"),
            ("Kombi", "Hajvágás + Borotválás"),
            ("Szemöldök", "Szemöldök rendezés"),
            ("Orrszőr", "Orrszőr eltávolítás")
        };

        var services = new List<Service>();

        for (int i = 0; i < Math.Min(amount, barberServices.Count); i++)
        {
            services.Add(new Service
            {
                Tag = barberServices[i].Tag,
                Value = barberServices[i].Value,
                BusinessType = BusinessType.BarberSalon
            });
        }

        return services;
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