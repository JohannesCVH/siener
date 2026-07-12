using System;
using Microsoft.EntityFrameworkCore;
using Siener.Data.Entities;
using Siener.Models;

namespace Siener.Data;

public class DatabaseContext : DbContext
{
    // public DbSet<User> Users { get; set; }
    // public DbSet<PushSubscription> PushSubscriptions { get; set; }

    public DbSet<Event> Events { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>().ToTable("events");

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Camera).HasColumnName("camera").IsRequired();
            entity.Property(e => e.DetectionTypes).HasColumnName("detection_types").IsRequired();
            entity.Property(e => e.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Notified).HasColumnName("notified").IsRequired();
        });
    }
}