using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDB")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});
builder.Services.AddHttpContextAccessor();

// Custom services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management system. " +
                      "Supports Organiser and Participant roles via server-side sessions. " +
                      "Use POST /api/auth/login to authenticate, then session cookies will be " +
                      "used automatically for subsequent requests.",
        Contact = new OpenApiContact { Name = "RaceDay Development Team" }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Other" };
    });

    c.DocInclusionPredicate((name, api) => true);
});

var app = builder.Build();

// Auto-migrate and seed on startup (relational providers only — skips InMemory used by tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }

    await DbSeeder.SeedAsync(db, hasher);
}

// Swagger — always enabled so marker can test without env config
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "RaceDay API Documentation";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
});

app.UseHttpsRedirection();

// Order matters: Session before anything reading HttpContext.Session
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "RaceDay API",
    timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Required for WebApplicationFactory<Program> in tests
public partial class Program { }