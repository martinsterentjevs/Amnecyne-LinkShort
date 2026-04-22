using System.Text;
using Amnecyne.LinkShort.Data;
using Amnecyne.LinkShort.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ValidateEnvironment();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DBStorageService>();
builder.Services.AddScoped<ShortLinkService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();
//Rate-limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("public_access", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromHours(1);
        opt.SegmentsPerWindow = 1;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(requireAuthPolicy);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();

app.MapHealthChecks("/health").AllowAnonymous();

//app.UseHttpsRedirection();
app.MapGet("/{code}", async (string code, ShortLinkService linkService) =>
{
    try
    {
        var link = await linkService.GetShortLinkAsync(code);
        return Results.Redirect(link.RedirectUrl);
    }
    catch
    {
        return Results.NotFound();
    }
}).AllowAnonymous();

app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

void ValidateEnvironment()
{
    var requiredVariables = new[] { "Jwt:Key", "Jwt:Issuer", "Jwt:Audience" };
    foreach (var variable in requiredVariables)
        if (string.IsNullOrEmpty(builder.Configuration[variable]))
            throw new Exception($"Environment variable '{variable}' is not set.");
}