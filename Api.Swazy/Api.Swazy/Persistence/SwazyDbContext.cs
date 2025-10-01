using Api.Swazy.Models.Base;
using Api.Swazy.Models.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Api.Swazy.Persistence;
public class SwazyDbContext(DbContextOptions<SwazyDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessService> BusinessServices { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserBusinessAccess> UserBusinessAccesses { get; set; }
    public DbSet<EmployeeWeeklySchedule> EmployeeWeeklySchedules { get; set; }
    public DbSet<EmployeeDaySchedule> EmployeeDaySchedules { get; set; }

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
        
        modelBuilder.Entity<Business>()
            .Property(b => b.BusinessType)
            .HasConversion<int>();
        
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
        
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.BookedByUser)
            .WithMany()
            .HasForeignKey(b => b.BookedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .Property(u => u.SystemRole)
            .HasConversion<int>();
        

        modelBuilder.Entity<BusinessService>()
            .Property(bs => bs.Price)
            .HasPrecision(10, 2);
        
        modelBuilder.Entity<UserBusinessAccess>()
            .HasOne(uba => uba.User)
            .WithMany(u => u.BusinessAccesses)
            .HasForeignKey(uba => uba.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserBusinessAccess>()
            .HasOne(uba => uba.Business)
            .WithMany(b => b.UserAccesses)
            .HasForeignKey(uba => uba.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserBusinessAccess>()
            .HasIndex(uba => new { uba.UserId, uba.BusinessId })
            .IsUnique();

        modelBuilder.Entity<EmployeeWeeklySchedule>()
            .HasOne(ews => ews.User)
            .WithMany()
            .HasForeignKey(ews => ews.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeWeeklySchedule>()
            .HasOne(ews => ews.Business)
            .WithMany()
            .HasForeignKey(ews => ews.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeWeeklySchedule>()
            .HasIndex(ews => new { ews.UserId, ews.BusinessId })
            .IsUnique();

        modelBuilder.Entity<EmployeeDaySchedule>()
            .HasOne(eds => eds.EmployeeWeeklySchedule)
            .WithMany(ews => ews.DaySchedules)
            .HasForeignKey(eds => eds.EmployeeWeeklyScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeDaySchedule>()
            .HasIndex(eds => new { eds.EmployeeWeeklyScheduleId, eds.DayOfWeek })
            .IsUnique();

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