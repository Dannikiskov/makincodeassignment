using GrpcService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().HasKey(u => u.Uuid);
        modelBuilder.Entity<NoteEntity>().HasKey(n => n.Uuid);

        modelBuilder.Entity<NoteEntity>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserUuid)
            .IsRequired();
    }
}
