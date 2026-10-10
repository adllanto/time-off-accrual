using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Data;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Tests;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }

    public TestDb()
    {
        // In-memory database lives as long as this connection stays open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    // Adds an agent with a balance and returns the user's Id
    public int AddAgent(string email, decimal earned, decimal taken = 0)
    {
        var user = new User { Name = email, Email = email, PasswordHash = "x", Role = Role.Agent };
        Db.Users.Add(user);
        Db.SaveChanges();

        Db.CreditBalances.Add(new CreditBalance { UserId = user.Id, EarnedHours = earned, TakenHours = taken });
        Db.SaveChanges();
        return user.Id;
    }

    public int AddAdmin()
    {
        var admin = new User { Name = "Admin", Email = "admin@test.com", PasswordHash = "x", Role = Role.Admin };
        Db.Users.Add(admin);
        Db.SaveChanges();
        return admin.Id;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}