using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
// Добавяне на MVC услуги
builder.Services.AddControllersWithViews();

// Configure database with connection string
// Конфигуриране на базата данни с низ за връзка
var connectionString = builder.Configuration.GetConnectionString("GardenDbConnection")
    ?? "Data Source=CommunityGarden.db";

// Use SQLite for better compatibility (or SQL Server if LocalDB is available)
// Използване на SQLite за по-добра съвместимост (или SQL Server, ако LocalDB е наличен)
builder.Services.AddDbContext<CommunityGardenDatabase>(options =>
{
    if (connectionString.Contains("localdb") || connectionString.Contains("sqlexpress", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
        // Using SQLite for all other cases.
        // Използване на SQLite за всички други случаи.
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Configure ASP.NET Core Identity with ApplicationUser and IdentityRole
// Конфигуриране на ASP.NET Core Identity с ApplicationUser и IdentityRole
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy settings
        // Настройки на паролната политика
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // User settings
        // Настройки на потребителя
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CommunityGardenDatabase>()
    .AddDefaultTokenProviders();

// Configure the login/logout cookie paths
// Конфигуриране на пътищата на бисквитките за вход/изход
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Register application services
// Регистриране на услуги на приложението.
builder.Services.AddScoped<PlotManagementService>();
builder.Services.AddScoped<MemberManagementService>();

var app = builder.Build();

// Seed roles and default users on startup
// Начални роли и потребители при стартиране
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure Admin and User roles exist
    // Гарантиране на съществуването на роли Admin и User
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed Admin account
    // Начален администраторски акаунт
    const string adminEmail = "admin@garden.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Garden Administrator",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed regular User account
    // Начален потребителски акаунт
    const string userEmail = "user@garden.com";
    if (await userManager.FindByEmailAsync(userEmail) == null)
    {
        var user = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            FullName = "Garden User",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "User123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "User");
    }

    // Seed garden plots, members, harvest records, and announcements
    // Начални данни за парцели, членове, реколти и обявления
    var gardenDb = scope.ServiceProvider.GetRequiredService<CommunityGardenDatabase>();
    await DbSeeder.SeedAsync(gardenDb);
}

// Configure the HTTP request pipeline.
// Настройка HTTP заявките
if (!app.Environment.IsDevelopment())
{
    // Use custom 500 error page for unhandled exceptions in production
    // Използване на персонализирана страница за грешки 500 в продукция
    app.UseExceptionHandler("/Error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Use custom error page in development too (developer exception page still applies for unhandled exceptions)
    // Използване на персонализирана страница за грешки и в режим на разработка
    app.UseExceptionHandler("/Error/500");
}

// Redirect status code responses (404, 403, etc.) to custom error pages
// Пренасочване на отговори с код на грешка към персонализирани страници
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
// Redirect HTTP requests to HTTPS
// Пренасочване на HTTP заявки към HTTPS

// Add security headers to every response
// Добавяне на заглавки за сигурност към всеки отговор
app.Use(async (context, next) =>
{
    // Prevent MIME-type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Prevent the app from being embedded in iframes (clickjacking protection)
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // Content Security Policy:
    //   script-src 'self'          – only scripts from this origin (no inline scripts, no CDNs)
    //   style-src  'self' 'unsafe-inline' – same-origin stylesheets + inline <style> blocks used in views
    //   img-src    'self' data:    – same-origin images and embedded data URIs
    //   font-src   'self'          – same-origin fonts only
    //   form-action 'self'         – forms may only submit to this origin
    //   frame-ancestors 'none'     – redundant with X-Frame-Options but explicit for CSP-aware browsers
    //   default-src 'self'         – catch-all for any directive not listed above
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'");

    await next();
});

app.UseRouting();
// Enable routing
// Активиране на маршрутизация

app.UseAuthentication();
// Enable authentication (must come before UseAuthorization)
// Активиране на удостоверяване (трябва да е преди UseAuthorization)
app.UseAuthorization();
// Enable authorization
// Активиране на авторизация

app.MapStaticAssets();
// Map static assets (custom extension)
// Свързване на статични ресурси (потребителско разширение)

// Area route must be registered before the default route
// Маршрутът на областта трябва да е регистриран преди основния маршрут
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
// Default controller route with static assets
// Основен маршрут на контролера със статични ресурси

app.Run();
