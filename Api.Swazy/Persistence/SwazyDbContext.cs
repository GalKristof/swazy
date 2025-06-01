using Api.Swazy.Models.Base;
using Api.Swazy.Models.Entities;
using Api.Swazy.Types;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Text.Json;

namespace Api.Swazy.Persistence;
public class SwazyDbContext(DbContextOptions<SwazyDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessService> BusinessServices { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Model.GetEntityTypes().ToList().ForEach(entity =>
        {
            entity.SetTableName(entity.DisplayName().Pluralize());
        });

        modelBuilder.Entity<Business>()
                .Property(b => b.Employees)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<Dictionary<Guid, BusinessRole>>(v, new JsonSerializerOptions()) ?? new()
                ).Metadata.SetValueComparer(new ValueComparer<Dictionary<Guid, BusinessRole>>(
                    (c1, c2) => c1.SequenceEqual(c2), // Compare dictionaries
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key, v.Value.GetHashCode())), // Hashing for EF tracking
                    c => c.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) // Deep copy
                ));

        modelBuilder.Entity<Business>()
            .HasMany(b => b.Services)
            .WithOne(bs => bs.Business)
            .HasForeignKey(bs => bs.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<BusinessService>()
            .HasOne(bs => bs.Business)
            .WithMany(b => b.Services)
            .HasForeignKey(bs => bs.BusinessId);

        modelBuilder.Entity<BusinessService>()
            .HasOne(bs => bs.Service)
            .WithMany()
            .HasForeignKey(bs => bs.ServiceId);
        
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.BusinessService)
            .WithMany()
            .HasForeignKey(b => b.BusinessServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Employee)
            .WithMany()
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Business>()
            .Property(b => b.BusinessType)
            .HasConversion<int>();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<int>();

        modelBuilder.Entity<BusinessService>()
            .Property(bs => bs.Price)
            .HasPrecision(10, 2);

        // Exclude Already Soft Deleted Files From Queries
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }
    
    private static LambdaExpression GetSoftDeleteFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
        return filter;
    }
}