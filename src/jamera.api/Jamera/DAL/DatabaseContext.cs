using System;
using Microsoft.EntityFrameworkCore;
using Jamera.Models;

namespace Jamera.DAL;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<PushSubscription> PushSubscriptions { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions) { }
}