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
    public DbSet<BusinessEmployee> BusinessEmployees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Model.GetEntityTypes().ToList().ForEach(entity =>
        {
            entity.SetTableName(entity.DisplayName().Pluralize());
        });

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

        modelBuilder.Entity<BusinessEmployee>(entity =>
        {
            // Composite primary key
            entity.HasKey(be => new { be.BusinessId, be.UserId });

            // Relationship to Business
            entity.HasOne(be => be.Business)
                .WithMany(b => b.BusinessEmployees)
                .HasForeignKey(be => be.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to User (Employee)
            entity.HasOne(be => be.User)
                .WithMany(u => u.BusinessEmployments)
                .HasForeignKey(be => be.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to User (HiredBy)
            entity.HasOne(be => be.HiredByUser)
                .WithMany(u => u.HiredEmployees)
                .HasForeignKey(be => be.HiredBy)
                .OnDelete(DeleteBehavior.Restrict); // Or NoAction, to prevent cycles

            entity.Property(be => be.Role).HasConversion<int>();
        });

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