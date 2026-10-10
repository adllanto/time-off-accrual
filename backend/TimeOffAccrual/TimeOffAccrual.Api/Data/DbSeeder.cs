using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        // Seed only once: if any user exists, do nothing
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();

        var admin = new User { Name = "Alex Admin", Email = "admin@sample.com", Role = Role.Admin };
        var agent1 = new User { Name = "John Agent", Email = "agent1@sample.com", Role = Role.Agent };
        var agent2 = new User { Name = "Jane Agent", Email = "agent2@sample.com", Role = Role.Agent };
        var agent3 = new User { Name = "Smith Agent", Email = "agent3@sample.com", Role = Role.Agent };

        var users = new[] { admin, agent1, agent2, agent3 };

        // Demo password only. Each hash is salted, so identical passwords produce different hashes.
        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, "Temp#12345");
        }

        db.Users.AddRange(users);
        await db.SaveChangesAsync(); // saves users first so they get their Ids

        db.CreditBalances.AddRange(
            new CreditBalance { UserId = agent1.Id, EarnedHours = 80, TakenHours = 16 },
            new CreditBalance { UserId = agent2.Id, EarnedHours = 120, TakenHours = 0 },
            new CreditBalance { UserId = agent3.Id, EarnedHours = 8, TakenHours = 0 } // low balance, for demoing the block
        );
        await db.SaveChangesAsync();
    }
}