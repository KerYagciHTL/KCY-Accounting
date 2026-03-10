namespace KCY_Accounting.Infrastructure;

/// <summary>
/// Ensures the SQLite database and schema exist on first launch.
/// Called once at application startup before the UI is shown.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(AppDbContext context)
    {
        // Creates the database file and all tables if they do not exist yet.
        // EnsureCreated is intentionally used here (no migration overhead for a desktop app).
        await context.Database.EnsureCreatedAsync();
    }
}

