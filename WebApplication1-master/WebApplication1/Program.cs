using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
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
        // Using SQLite for all other cases
        // Използване на SQLite за всички други случаи
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Register application services
// Регистриране на услуги на приложението
builder.Services.AddScoped<PlotManagementService>();
builder.Services.AddScoped<MemberManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Настройка HTTP заявките
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
