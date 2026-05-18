using SpiEyes.Models;

namespace SpiEyes.DAL;

public static class DbInitializer
{
    public static void Initialize(DatabaseContext databaseContext)
    {
        if (databaseContext.Users.Any()) return;

        var adminUser = new User() { Id = Guid.Parse("9ad428eb-4ad3-420e-ad45-ae2f333b6f02"), Username = "admin" };
        databaseContext.Users.Add(adminUser);
        databaseContext.SaveChanges();
    }
}
