using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StreamHub.Data;
using StreamHub.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- Database setup ----
// On Render, a Postgres DB provides a DATABASE_URL like: postgres://user:pass@host:port/dbname
// Locally (no DATABASE_URL set), we fall back to SQLite so you can run this with zero setup.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        options.UseNpgsql(BuildNpgsqlConnectionString(databaseUrl));
    }
    else
    {
        var sqliteConn = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=streamhub.db";
        options.UseSqlite(sqliteConn);
    }
});

// ---- Identity setup ----
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relaxed password rules — fine for a demo/college project, not for real production use.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create the database schema on startup if it doesn't exist yet.
// (Using EnsureCreated instead of EF migrations keeps setup simple for this project —
// no need to install the dotnet-ef tool.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serves everything under wwwroot, including wwwroot/videos.
// ASP.NET Core's static file middleware supports HTTP Range requests out of the box,
// so <video> seeking/scrubbing works correctly without any custom streaming code.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string BuildNpgsqlConnectionString(string databaseUrl)
{
    // Converts Render's postgres://user:pass@host:port/dbname format
    // into the key=value format Npgsql expects.
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var database = uri.AbsolutePath.TrimStart('/');

    // Render's Internal Database URL often omits the port since it's the Postgres
    // default. Uri.Port returns -1 in that case (since "postgresql" isn't a scheme
    // .NET recognizes with a built-in default), so we fall back to 5432 ourselves.
    var port = uri.Port == -1 ? 5432 : uri.Port;

    return $"Host={uri.Host};Port={port};Database={database};" +
           $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}