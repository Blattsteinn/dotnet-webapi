using Microsoft.EntityFrameworkCore;
using FoodOrderingApi.Data;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Places;
using System.Text.Json.Serialization;
using FoodOrderingApi.Services.Accounts;

using FoodOrderingApi.Models.Users;

using FoodOrderingApi.Authorization;

// Auth imports ----
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
// ----------------

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// also ignores JSON cycling

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Injections?
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
// ------------------------------------
// ------------------------------------
// Added this for auth.
builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    .AddAuthentication()
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
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            )
        };

        // A JWT is a snapshot: it keeps claiming "Owner" long after the role was
        // taken away. Re-checking the security stamp against the store turns
        // that snapshot into something that can be revoked.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();

                var userId = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    context.Fail("Token has no user id claim.");
                    return;
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    context.Fail("User no longer exists.");
                    return;
                }

                var stampInToken = context.Principal?
                    .FindFirst(AppClaimTypes.SecurityStamp)?.Value;

                var currentStamp = await userManager.GetSecurityStampAsync(user);

                if (stampInToken != currentStamp)
                    context.Fail("Roles have changed since this token was issued.");
            }
        };
    });

builder.Services.AddAuthorization();

//------------------------------------------------- 
//------------------------------------------------- 

builder.Services.AddScoped<PlaceService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<OwnerService>();
builder.Services.AddScoped<ManagerService>();


var app = builder.Build();


// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// -----------------------------------------------
// Seed Identity roles, then make sure the single admin account exists so the
// API is usable out of the box. The Admin role is modelled as a role (rather
// than a special-cased user) so the check is uniform: user.IsAdmin().
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var roleName in Roles.All)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    if (app.Environment.IsDevelopment())
    {
        var adminEmail =
            builder.Configuration["Seed:AdminEmail"] ?? "admin@example.com";
        var adminPassword =
            builder.Configuration["Seed:AdminPassword"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(admin, adminPassword);
        }

        if (admin != null && !await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
// -----------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
